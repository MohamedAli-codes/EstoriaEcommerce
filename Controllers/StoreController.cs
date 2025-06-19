using E_commerce.Models;
using E_commerce.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Controllers
{
    public class StoreController : Controller
    {
        private ApplicationContext context;
        private readonly int pageSize = 8;
        public StoreController(ApplicationContext context)
        {
            this.context = context;
        }

        //lazem trteb query search fn -> pagination l2n 3dd results mbny 3l search
        public IActionResult Index(int pageIndex , string? searchText , string? brand  , string? category , string? sort )
        {
            IQueryable<Product> query = context.Products;
            
            // Apply search functionality
            if (!string.IsNullOrEmpty(searchText) && searchText.Length > 0)
            {
                query = query.Where(p => p.Name.Contains(searchText) || p.Brand.Contains(searchText));
            }

            /*3) Filter functionality */
            //3.1 get brand names & category from db
            var brands =  context.Products.Select(p=>p.Brand).Distinct().OrderBy(p=>p).ToList();
            var categories = context.Products.Select(p => p.Category).Distinct().OrderBy(p => p).ToList();
            //3.2 send brandNames & categories to the view
            ViewBag.Brands = brands;
            ViewBag.categories = categories;
            // Apply brand &category filter
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Brand == brand);
            }
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }
            /*------*/

            /*-----sort functionality-----*/
            //            < option value = "Newest" > Latest products </ option >

            //< option value = "Price_asc" > Price: Low to High </ option >

            //< option value = "Price_desc" > P
            if ( sort== "Price_asc")
            {
                query = query.OrderBy(p=>p.Price);
            }
            else if (sort == "Price_desc")
            {
                query = query.OrderByDescending(p=>p.Price);
            }
            else
            {
                query = query.OrderByDescending(p => p.CreatedAt);
            }


            //1.1=>pagination functionality
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            decimal productsCount = query.Count();
            int totalPages = (int)Math.Ceiling(productsCount / pageSize);

            //if (pageIndex > totalPages)
            //{
            //    pageIndex = totalPages;
            //}
            /*----------End of pagination fn--------*/



            var products = query.Skip((pageIndex-1)*pageSize).Take(pageSize).ToList();


            /*------- Data needed by view -------*/
            //gm3t kol prop fel model w hb3toh ll view
            //ViewData["brand"] = brand; //needed for selected attribute
            //ViewData["category"] = category; //needed for selected attribute
            //ViewData["searchText"] = searchText;
            StoreSearchModel storeSearchModel = new StoreSearchModel()
            {
                Brand = brand,
                SearchText = searchText,
                Category = category,
                Sort = sort,
            };
            ViewData["totalPages"]= totalPages;
            ViewData["pageIndex"]= pageIndex;
            ViewBag.Products = products;

            return View(storeSearchModel);
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
                return BadRequest();
            var product = context.Products.Find(id);
            if (product == null)
                return RedirectToAction("index");
            return View(product);
        }
    }
}
