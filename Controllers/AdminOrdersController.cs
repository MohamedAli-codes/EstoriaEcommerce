using System.Threading.Tasks;
using E_commerce.Models;
using E_commerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;

namespace E_commerce.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/Orders/{action=Index}/{id?}")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationContext context;
        private readonly int pageSize =5;

        public AdminOrdersController(ApplicationContext context)
        {
            this.context = context;
        }
        public async Task<IActionResult> Index(int pageIndex =1)
        {
            IQueryable<Order> query = context.Orders.
                Include(o => o.Items).
                Include(o => o.Client).
                OrderByDescending(o => o.Id);
            int totalCount = await query.CountAsync();
            int pagesCount = (int)Math.Ceiling((double)totalCount / pageSize);
            if (pagesCount == 0)
            {
                pagesCount = 1;
            }
            if(pageIndex < 1 || pageIndex > pagesCount)
            {
                pageIndex = 1;
            }
            ViewBag.PagesCount = pagesCount;
            ViewBag.PageIndex = pageIndex;
            query= query.Skip((pageIndex-1)* pageSize).Take(pageSize);

            var orders = await query.
                ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var order = await context.Orders
                .Include(o => o.Client)
                .Include(o => o.Items)
                .ThenInclude(i=>i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }
            var clientOrdersCount = context.Orders.Where(o => o.ClientId == order.ClientId).Count();
            ViewBag.ClientOrders = clientOrdersCount;
            return View(order);
        }

        public async Task<IActionResult> Edit(int? id, string? order_status, string? payment_status) {
            if (id==null)
            {
                return RedirectToAction("Index");
            }

            var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) {
                return RedirectToAction("Index");
            }
            if(order_status==null && payment_status == null)
                return RedirectToAction("Details", new {id});

            if (order_status != null)
                order.OrderStatus = order_status;

            if (payment_status != null)
                order.PaymentStatus= payment_status;

            context.Orders.Update(order);

            context.SaveChanges();
            return RedirectToAction("Details", new {id});

        }

        public IActionResult DashBoard()
        {
            var totalProducts = context.Products.Count();
            var totalUsers = context.Users.Count();
            var totalOrders = context.Orders.Count();
            var revenue = context.Orders.Sum(o => o.TotalPrice);

            ViewData["TotalProducts"] = totalProducts;
            ViewData["TotalUsers"] = totalUsers;
            ViewData["TotalOrders"] = totalOrders;
            ViewData["Revenue"] = revenue;

            return View();
        }
    }
}
