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

    public MemberController(SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
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
        var user =  await _userManager.GetUserAsync(User);
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
}