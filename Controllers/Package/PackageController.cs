using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using backend.Package.Models;

namespace backend.Package.Controllers;

public class PackageController : Controller
{
   

    public IActionResult Index()
    {
        return View("~/Views/Package/Home/Index.cshtml");
    }

    public IActionResult Privacy()
    {
        return View("~/Views/Package/Home/Privacy.cshtml");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View( "~/Views/Package/Shared/Error.cshtml",new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

}
