using E_commerce.Models;
using E_commerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;

namespace E_commerce.Controllers
{
    [Authorize(Roles ="admin")]
    //[Route("Admin/[controller]/[Action]")]  //pattern dah h y poplulated by tag helpers
    [Route("Admin/[controller]/{action=Index}/{id?}")] 
        //dah hy5ly action b set beh b default w id? optional
    public class ProductController : Controller
    {
        private ApplicationContext context;
        int pageSize = 5;
        public ProductController(ApplicationContext context)
        {
            this.context = context;
        }

        //populate mock data 30 rows
        //public IActionResult PopulateMockData()
        //{
        //    var fakeProducts = ProductSeeder.GenerateFakeProducts();
        //    context.Products.AddRange(fakeProducts);
        //    context.SaveChanges();
        //    return  Content("data created");
        //}
        public IActionResult Index(int pageIndex , string? search , string? column, string? orderBy)
        {
            if (pageIndex < 1)
                pageIndex = 1;
            IQueryable<Product> query = context.Products;

            //search functionality
            if (search != null)
            {
                query = query.Where(p => p.Name.Contains(search) || p.Brand.Contains(search) );
            }
            //sort functionality
            string[] validColumnNames = { "Id", "Name" , "Brand", "Category", "Price" , "creationDate" };
            string[] validOrderBy = { "desc", "asc" };

            if( !validColumnNames.Contains(column))
            {
                column = "Id"; //default column for sorting
            }
            if (!validOrderBy.Contains(orderBy))
            {
                orderBy = "asc"; //default order for sorting
            }

            if (column == "Name")
            {
                if (orderBy == "asc")
                    query = query.OrderBy(p => p.Name);
                else
                    query = query.OrderByDescending(p => p.Name);
            }
            else if (column == "Brand")
            {
                if (orderBy == "asc")
                    query = query.OrderBy(p => p.Brand);
                else
                    query = query.OrderByDescending(p => p.Brand);
            }
            else if (column == "Category")
            {
                if (orderBy == "asc")
                    query = query.OrderBy(p => p.Category);
                else
                    query = query.OrderByDescending(p => p.Category);
            }
            else if (column == "Price")
            {
                if (orderBy == "asc")
                    query = query.OrderBy(p => p.Price);
                else
                    query = query.OrderByDescending(p => p.Price);
            }
            else if (column == "creationDate")
            {
                if (orderBy == "asc")
                    query = query.OrderBy(p => p.CreatedAt);
                else
                    query = query.OrderByDescending(p => p.CreatedAt);
            }
            else
            {
                if (orderBy == "asc")
                    query = query.OrderBy(p => p.Id);
                else
                    query = query.OrderByDescending(p => p.Id);
            }
            //--------end of sort functionality----------//

            //pagination calculations
            decimal totalProducts = query.Count();
            int totalPages = (int)Math.Ceiling(totalProducts/pageSize);
            query = query.Skip(pageSize * (pageIndex - 1)).Take(pageSize);

            var products = query.ToList();
            ViewData["pageIndex"] = pageIndex;
            ViewData["totalPages"] = totalPages;
            ViewData["search"] = search;
            ViewData["column"] = column;
            ViewData["orderBy"] = orderBy;

            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await context.Products.Select( p=>p.Category).Distinct().ToListAsync();
            ViewData["Categories"] = categories;
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDto productDto)
        {
            if( productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The Image file is required.");
            }
            
            if (ModelState.IsValid)
            {
                Product product = new Product();
                product.Name = productDto.Name;
                product.Brand = productDto.Brand;
                product.Category = productDto.Category;
                product.Price = productDto.Price;
                product.Description = productDto.Description;
                product.Stock = productDto.Stock;
                if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(productDto.ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        productDto.ImageFile.CopyTo(stream);
                    }
                    product.ImageUrl = $"/images/{fileName}";
                }
                context.Add(product);
                context.SaveChanges();
            }
            else
            {
                return View(productDto);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit([FromRoute] int? id)
        {
            if (id == null)
                return BadRequest();
            var product = context.Products.FirstOrDefault( p=>p.Id==id );
            var categories = await context.Products.Select(p => p.Category).Distinct().ToListAsync();
            if (product == null )
                return NotFound();


            var productDto = new ProductDto()
            {
                Name = product.Name,
                Brand = product.Brand,
                Category = product.Category,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock
            };

            ViewData["productId"] = product.Id;
            ViewData["productImage"] = product.ImageUrl;
            ViewData["createdAt"] = product.CreatedAt.ToString("d");
            ViewData["Categories"] = categories;

            return View(productDto);
        }

        private void NotifySubscribers(int productId)
        {
            var subs = context.ProductSubscriptions
                        .Where(s => s.ProductId == productId && !s.Notified)
                        .ToList();

            foreach (var sub in subs)
            {
                sub.Notified = true;
            }

            context.SaveChanges();
        }

        [HttpPost]
        public IActionResult Edit([FromRoute] int id , ProductDto productDto)
        {
            var product = context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null )
                return NotFound();

            if (ModelState.IsValid)
            {
                var oldfileName = product.ImageUrl;
                var newfileName = string.Empty;
                //hna b2oloh lw htet sora gdeda e3ml update kda kda el sora el adema fe db
                if (productDto.ImageFile != null && productDto.ImageFile.Length > 0)
                {
                    newfileName = Path.GetFileName(productDto.ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", newfileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        productDto.ImageFile.CopyTo(stream);
                    }
                    product.ImageUrl = $"/images/{newfileName}";

                    //delete old image if exists on wwwroot
                    string oldImageFullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", oldfileName);
                    if ( System.IO.File.Exists(oldImageFullPath) )
                              System.IO.File.Delete(oldImageFullPath);
                }


                if (product.Stock == 0 && productDto.Stock > 0)
                {
                    NotifySubscribers(product.Id);
                }

                //update product in db
                product.Name = productDto.Name;
                product.Brand = productDto.Brand;
                product.Category = productDto.Category;
                product.Price = productDto.Price;
                product.Description = productDto.Description;
                product.Stock = productDto.Stock;

                context.SaveChanges();
            }
            else
            {
                ViewData["productId"] = product.Id;
                ViewData["productImage"] = product.ImageUrl;
                ViewData["createdAt"] = product.CreatedAt.ToString("d");
                return View(productDto);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Delete([FromRoute] int? id)
        {
            if (id == null)
                return BadRequest();
            var product = context.Products.Find(id);
            if (product == null)
                return NotFound();
            else
                return View(product);
        }

        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed([FromRoute] int? id)
        {
            if (id == null)
                return BadRequest();
            var product = context.Products.Find(id);
            if (product == null)
                return NotFound();
            else
            {
                // delete image if exists in local folder
                string oldImageFullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", product.ImageUrl);
                if ( System.IO.File.Exists(oldImageFullPath) )
                     System.IO.File.Delete(oldImageFullPath);
                //---//
                context.Products.Remove(product);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
           
        }


        public ActionResult ProductDetailsPartial(int? id)
        {
            if (id == null)
                return BadRequest();
            var product = context.Products.Find(id);
            if (product == null)
                return RedirectToAction("index");
            return PartialView("ProductDetailsPartial", product);
        }
    }
}
