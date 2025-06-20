using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Identity101.Data.Dto;
using Mvc.Identity101.Data.Entites;
using Mvc.Identity101.Models;
using Mvc.Identity101.Services.Abstract;

namespace Mvc.Identity101.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailService _emailService;

    public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager, IEmailService emailService)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    [AllowAnonymous] 
    [HttpGet]
    public IActionResult SignUp()
    {
        if (User.Identity.IsAuthenticated && User.Identity != null)
        {
            return RedirectToAction("Index");
        }

        return View();
    }
    [ValidateAntiForgeryToken] // Bu gerekli işlemlerin tek koldan ypaıldıgını dogrulayarak
                               // CSRF saldirilarina korunaklı hale getirir
    [HttpPost]
    public async Task<IActionResult> SignUp(SignUpDto dto)
    {
        if (ModelState.IsValid)
        {
            var result = await _userManager.CreateAsync(new AppUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.Phone
            }, password: dto.Password);

            if (result.Succeeded)
            {
                TempData["Message"] = $"User : {dto.UserName} created successfully";
                return RedirectToAction("SignUp");
            }

            foreach (IdentityError error in result.Errors)
            {
                //string empty deme sebebimiz direk all errors divinde göstermek 
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> SignIn()
    {
        if (User.Identity.IsAuthenticated && User.Identity != null)
        {
            return RedirectToAction("index");
        }

        return View();
    }
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> SignIn(SignInDto dto, string? returnURL) // Simdilik Return url ile ugrasmicaz
    {
        if (ModelState.IsValid)
        {
            var entity = await _userManager.FindByEmailAsync(dto.Email);
            if (entity == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı Bulunamad<UNK>");
                return View(dto);
            }

            var result = await _signInManager.PasswordSignInAsync(entity, dto.Password, dto.RememberMe, true);
            if (result.Succeeded)
            {
                // return Redirect(returnURL ?? "/"); // Bu yanlıs phising olabileceği için isLocalUrl kullancaz
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnURL) && Url.IsLocalUrl(returnURL))
                    {
                        return Redirect(returnURL);
                    }

                    return RedirectToAction("Index", "Home");
                }
            }
            else if (result.IsLockedOut)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(entity);
                var remaining = lockoutEnd.Value.UtcDateTime - DateTime.UtcNow;
                var minutes = (int)Math.Ceiling(remaining.TotalMinutes);

                ModelState.AddModelError(string.Empty,
                    $"Hesabınız kilitlenmiştir. {minutes} dakika sonra tekrar deneyiniz.");
            }
            else
            {
                ModelState.AddModelError("", "Geçersiz giriş denemesi.");
            }
        }

        return View(dto);
    }


    public async Task<IActionResult> ForgotPassword()
    {
        return View();
    }
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request)
    {
        if (ModelState.IsValid)
        {
            var hasUser = await _userManager.FindByEmailAsync(request.Email);
            if (hasUser == null)
            {
                ModelState.AddModelError("", "Bu e postaya ait bir hesap bulunamamiştir ");
                return View(request);
            }

            string pwToken = await _userManager.GeneratePasswordResetTokenAsync(hasUser);
            string pwResetLink = Url.Action("ResetPassword", "Home", new { userId = hasUser.Id, token = pwToken },
                protocol: HttpContext.Request.Scheme);

            await _emailService.SendResetPasswordEmailAsync(request.Email, pwResetLink);
            TempData["Message"] = "Maile sifre yenileme linki iletildi";
            return RedirectToAction(nameof(ForgotPassword));
        }

        return View();
    }

        public async Task<IActionResult> ResetPassword(string userId, string token)
        {
            var model = new ResetPasswordDto()
            {
                UserId = userId,
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
        {
            if (request.Token != null)
            {
                var hasUser = await _userManager.FindByIdAsync(request.UserId);
                if (hasUser != null)
                {
                    var result = await _userManager.ResetPasswordAsync(hasUser, request.Token, request.Password);

                    if (result.Succeeded)
                    {
                        TempData["Message"] = "Sifreniz Basariyla Değiştirilmiştir";
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        TempData["Message"] = "Hay Aksi bir sorun oluştu ";

                    }

                    return View();
                }

                ModelState.AddModelError(key: string.Empty, "Kullanıcı bulunamadi.");
                return View(request);
            }

            var dto = request;
            
            return View();
        }


    public async Task<IActionResult> AccessDenied()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}