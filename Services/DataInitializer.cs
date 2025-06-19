using System.Threading.Tasks;
using E_commerce.Models;
using Microsoft.AspNetCore.Identity;

namespace E_commerce.Services
{
    public class DataInitializer
    {
        public static async Task SeedUserRolesDataAsync(UserManager<ApplicationUser>? userManager , RoleManager<IdentityRole>? roleManager)
        {
            if(userManager ==null || roleManager == null)
            {
                Console.WriteLine("user manager or role manager ==null =>exit");
                    return;
            }

            //check if admin role exists
            var exists = await roleManager.RoleExistsAsync("Admin");
            if (!exists) {
                Console.WriteLine("admin role is not defined and will be created.");
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            //check if seller role exists
            exists = await roleManager.RoleExistsAsync("Seller");
            if (!exists)
            {
                Console.WriteLine("seller role is not defined and will be created.");
                await roleManager.CreateAsync(new IdentityRole("Seller"));
            }

            //check if client role exists
            exists = await roleManager.RoleExistsAsync("Client");
            if (!exists)
            {
                Console.WriteLine("client role is not defined and will be created.");
                await roleManager.CreateAsync(new IdentityRole("client"));
            }


            /*check if we have at least one admin exists*/
            var adminUsers = await userManager.GetUsersInRoleAsync("Admin");
            if (adminUsers.Any()) {
                Console.WriteLine("admin user already exists");
                return;
            }
            else
            {
                ApplicationUser user = new ApplicationUser()
                {
                    FirstName = "Admin",
                    LastName = "Admin",
                    Email = "admin@admin.com",
                    UserName = "admin@admin.com",
                    CreatedAt = DateTime.Now
                };
                string initialPassword = "admin123";
                var result = await userManager.CreateAsync(user, initialPassword);
                
                //set admin role to user
                if (result.Succeeded) {
                    await userManager.AddToRoleAsync(user, "Admin");
                    Console.WriteLine("Admin user created successfully! Please update the initial password: ");
                    Console.WriteLine($"Email: {user.Email} ");
                    Console.WriteLine($"Initial password: {initialPassword} ");
                }
            }
        }

    }
}
