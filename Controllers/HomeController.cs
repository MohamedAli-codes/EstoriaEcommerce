using System.Diagnostics;
using System.Security.Claims;
using E_commerce.Models;
using E_commerce.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var newestProducts = context.Products
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .ToList();

            var today = DateTime.Today;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // ??? ??????? ???? ??????? ?? ????? ?????
            var todayOrderItems = context.Orders
                .Where(o => o.CreatedAt.Date == today && o.ClientId== userId)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .SelectMany(o => o.Items) // ? Flatten ??? OrderItems ?? ?? Order
                .Select(oi => new
                {
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity
                })
                .ToList();

            // ????? ???????? ??? ??? View
            ViewBag.TodayOrders = todayOrderItems;

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
