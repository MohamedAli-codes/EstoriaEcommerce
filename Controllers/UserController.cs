using System;
using System.Linq;
using System.Threading.Tasks;
using E_commerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Controllers
{
    [Route("/Admin/[Controller]/{action=index}/{id ?}")]
    [Authorize(Roles = "admin")]
    public class UserController : Controller
    {
        private RoleManager<IdentityRole> roleManger;
        private UserManager<ApplicationUser> userManager;
        private readonly int pageSize = 5;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.roleManger = roleManager;
            this.userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index(int? pageIndex)
        {
            IQueryable<ApplicationUser> query = userManager.Users.OrderBy(u=>u.CreatedAt);

            if (pageIndex == null || pageIndex < 1) {
                pageIndex = 1;
            }
            //pagination btns requires
            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count/ pageSize);
            ViewBag.totalPages = totalPages; 
            ViewBag.pageIndex= pageIndex;

            query = query.Skip(((int)pageIndex-1)*pageSize).Take(pageSize);
            var users = await query.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if(id==null || !ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }

            var user = await userManager.FindByIdAsync(id);
            if(user == null)
            {
                return RedirectToAction("Index");
            }
            ViewBag.Roles = await userManager.GetRolesAsync(user);
            return View(user);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if(id==null || !ModelState.IsValid)
            {
                return BadRequest();
            }
            var appUser = await userManager.FindByIdAsync(id); 

            if (appUser == null)
            {
                return RedirectToAction("Index");
            }
            var userRoles = await userManager.GetRolesAsync(appUser);
            if (userRoles.Contains("admin"))
            {
                TempData["ErrorMessages"] = "You can't delete an account with admin role.!";
                return RedirectToAction("Details", new { id });

            }

            var currentUser = await userManager.GetUserAsync(User);
            if (currentUser!.Id== appUser.Id)
            {
                TempData["ErrorMessages"]= "You can't delete your own account.!";
                return RedirectToAction("Details", new {id});
            }
            var result = await userManager.DeleteAsync(appUser);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            TempData["ErrorMessages"] = "Unable to delete this account: "+ result.Errors.First().Description;
            return RedirectToAction("Details", new { id });

        }
    }
}
