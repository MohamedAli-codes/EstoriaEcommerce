using System.Diagnostics;
using E_commerce.Models;
using E_commerce.Services;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationContext context;

        public HomeController(ApplicationContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            var newestProducts = context.Products.OrderByDescending(p => p.CreatedAt).Take(10).ToList();
            return View(newestProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
