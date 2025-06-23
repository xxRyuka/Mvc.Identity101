using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mvc.Identity101.Areas.Admin.Data.Dto;
using Mvc.Identity101.Data.Entites;

namespace Mvc.Identity101.Areas.Admin.Controllers;

[Area("Admin")]
public class HomeController : Controller
{
    private readonly UserManager<AppUser> _userManager;

    public HomeController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> UserList()
    {
        var userList = await _userManager.Users.ToListAsync();
        List<UserListDto> DtoUserList = new List<UserListDto>();
        foreach (var user in userList)
        {
           
            DtoUserList.Add( new ()
            {
                UserName = user.UserName,
                Email = user.Email,
                Id = user.Id,
                imgPath = string.IsNullOrWhiteSpace(user.imgPath) ?  "/img/default.jpg" : user.imgPath,
                PhoneNumber = user.PhoneNumber,
            });
        }

        return View(DtoUserList);
    }
}