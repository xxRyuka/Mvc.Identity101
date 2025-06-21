using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mvc.Identity101.Data.Dto;
using Mvc.Identity101.Data.Entites;

namespace Mvc.Identity101.Controllers;

[Authorize]
public class MemberController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;

    public MemberController(SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager,
        UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SignOut()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("AccessDenied");
        }

        UserDto dto = new UserDto()
        {
            Email = user.Email,
            UserName = user.UserName,
            Id = user.Id,
            Phone = user.PhoneNumber,
        };

        return View(dto);
    }

    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }
    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);

            var checkPassword = await _userManager.CheckPasswordAsync(user, request.OldPassword);

            if (checkPassword)
            {
                var result =  await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        return View();
                    }
                }

                await _userManager.UpdateSecurityStampAsync(user); // sifre gibi kritik bir veri değiştirdiğimiz için güncelliyoruz
                await _signInManager.SignOutAsync();
                await _signInManager.PasswordSignInAsync(user, request.NewPassword, true, false);
                TempData["Message"] = "Your password has been changed successfully";
                return View();
            }
            ModelState.AddModelError(string.Empty, "The current password is incorrect");
            
        }
        return View();
    }
}