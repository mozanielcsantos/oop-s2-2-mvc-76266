using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OopS22Mvc76266.Web.Models;

namespace OopS22Mvc76266.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            FriendlyMessage = "Sorry, an unexpected error happened. Please try again later."
        };

        return View(model);
    }
}
