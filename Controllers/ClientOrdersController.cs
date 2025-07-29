using System.Collections.Immutable;
using System.Drawing.Printing;
using System.Threading.Tasks;
using E_commerce.Models;
using E_commerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Controllers
{
    [Route("/Client/Orders/{action=index}/{id?}")]
    [Authorize(Roles = "client")]
    public class ClientOrdersController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationContext context;
        private readonly int pageSize =5;

        public ClientOrdersController(UserManager<ApplicationUser> userManager , ApplicationContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }
        public async Task<IActionResult> Index(int pageIndex = 1)
        {
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }
            IQueryable<Order> query = context.Orders
                .Where(o=>o.ClientId==currentUser.Id)
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt);

            int totalCount = await query.CountAsync();
            int pagesCount = (int)Math.Ceiling((double)totalCount / pageSize);
            if (pagesCount == 0)
            {
                pagesCount = 1;
            }
            if (pageIndex < 1 || pageIndex > pagesCount)
            {
                pageIndex = 1;
            }
            ViewBag.PagesCount = pagesCount;
            ViewBag.PageIndex = pageIndex;

            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

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
            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await context.Orders
                .Include(o => o.Items)
                .ThenInclude(i=>i.Product)
                .Where(o => o.ClientId==currentUser.Id)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
