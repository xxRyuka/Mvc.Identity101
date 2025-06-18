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

    public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
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
    
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}