using E_commerce.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using E_commerce.Models;

namespace E_commerce.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationContext _context;

        public NotificationsController(ApplicationContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult Subscribe([FromBody] Dictionary<string, int> data)
        {
            var productId = data["productId"];
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var alreadySubscribed = _context.ProductSubscriptions
                .Any(s => s.ProductId == productId && s.UserId == userId);

            if (!alreadySubscribed)
            {
                _context.ProductSubscriptions.Add(new ProductSubscription
                {
                    ProductId = productId,
                    UserId = userId,
                    SubscribedAt = DateTime.Now
                });
                _context.SaveChanges();
            }

            return Json(new { success = true, message = "Subscribed successfully!" });
        }
        [HttpGet]
        public IActionResult Count()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var count = _context.ProductSubscriptions.Count(p => p.UserId == userId && p.Notified);
            return Json(count);
        }

        [HttpGet]
        public IActionResult Dropdown()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var notifications = _context.ProductSubscriptions
                .Include(p => p.Product)
                .Where(p => p.UserId == userId && p.Notified)
                .OrderByDescending(p => p.SubscribedAt)
                .Take(5)
                .ToList();

            return PartialView("_NotifDropdownPartial", notifications);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var notif = await _context.ProductSubscriptions.FindAsync(id);
            if (notif == null)
            {
                return NotFound();
            }

            _context.ProductSubscriptions.Remove(notif);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
