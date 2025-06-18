using Microsoft.AspNetCore.Mvc;

namespace Mvc.Identity101.Areas.Admin.Controllers;

public class HomeController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}