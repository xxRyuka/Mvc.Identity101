using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mvc.Identity101.Areas.Admin.Data.Dto;
using Mvc.Identity101.Data.Entites;

namespace Mvc.Identity101.Areas.Admin.Controllers;
[Authorize(Roles = "god")]
[Area("Admin")]
public class HomeController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public HomeController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.UserCount = _userManager.Users.Count();
        ViewBag.RoleCount = _roleManager.Roles.Count();
        return View();
    }

    public async Task<IActionResult> UserList()
    {
        var userList = await _userManager.Users.ToListAsync();
        List<UserListDto> DtoUserList = new List<UserListDto>();
        foreach (var user in userList)
        {
            DtoUserList.Add(new()
            {
                UserName = user.UserName,
                Email = user.Email,
                Id = user.Id,
                imgPath = string.IsNullOrWhiteSpace(user.imgPath) ? "/img/default.jpg" : user.imgPath,
                PhoneNumber = user.PhoneNumber,
                Roles =  _userManager.GetRolesAsync(user).Result.ToList()
            });
        }


        return View(DtoUserList);
    }
    
    
    [HttpPost]
    public async Task<IActionResult> RemoveUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        var result = await _userManager.DeleteAsync(user);
        return RedirectToAction("UserList");
    }
}