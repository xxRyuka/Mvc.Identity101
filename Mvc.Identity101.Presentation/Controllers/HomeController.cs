using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Identity101.Data.Dto;
using Mvc.Identity101.Data.Entites;
using Mvc.Identity101.Models;

namespace Mvc.Identity101.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        if (User.Identity.IsAuthenticated && User.Identity != null)
        {
            return RedirectToAction("Index");
        }

        return View();
    }

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

    [HttpPost]
    public async Task<IActionResult> SignIn(SignInDto dto, string? returnUrl) // Simdilik Return url ile ugrasmicaz
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
                return Redirect(returnUrl ?? "/");
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


    [HttpGet]
    public async Task<IActionResult> SignOut()
    {
         await _signInManager.SignOutAsync();
        return RedirectToAction("Index");
    }
    
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}