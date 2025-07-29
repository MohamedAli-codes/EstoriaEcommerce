using E_commerce.Models;
using E_commerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace E_commerce.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationContext context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly decimal shippingFee;

        public CartController(ApplicationContext context , UserManager<ApplicationUser> userManager , IConfiguration config)
        {
            this.context = context;
            this.userManager = userManager;
            this.shippingFee = config.GetValue<decimal>("shippingFee");
        }
        public async Task<IActionResult> Index()
        {
            List<OrderItem> cartItems = await CartHelper.GetOrderItemsAsync(Request, Response, context);
            decimal subTotal = CartHelper.GetSubTotal(cartItems);

            ViewBag.CartItems = cartItems;
            ViewBag.SubTotal = subTotal;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.Total = subTotal+shippingFee;

            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CheckOut(CheckOutDTO model) {
            List<OrderItem> cartItems = await CartHelper.GetOrderItemsAsync(Request, Response, context);
            decimal subTotal = CartHelper.GetSubTotal(cartItems);
            ViewBag.CartItems = cartItems;
            ViewBag.SubTotal = subTotal;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.Total = subTotal + shippingFee;

            if (!ModelState.IsValid) { 
                return View("Index",model);
            }

            if (cartItems.Count == 0)
            {
                ViewBag.ErrorMessages = "Your cart is empty!";
                return View("Index",model);
            }

            TempData["DeliveryAddress"] = model.ShippingAddress;
            TempData["PaymentMethod"] = model.PaymentMethod;
            if (model.PaymentMethod == "creditCard" || model.PaymentMethod == "paypal")
            {
                return RedirectToAction("Index", "ChecKout");
            }

            return RedirectToAction("Confirm");
        }

        public async Task<IActionResult> Confirm()
        {
            List<OrderItem> cartItems = await CartHelper.GetOrderItemsAsync(Request, Response, context);
            decimal total = CartHelper.GetSubTotal(cartItems) +shippingFee;
            decimal cartSize = 0;
            foreach(var item in cartItems)
            {
                cartSize += item.Quantity;
            }

            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            string paymentMethod = TempData["PaymentMethod"] as string ?? "";
            TempData.Keep();

            if(cartSize==0 || cartItems.Count == 0 || deliveryAddress.Length ==0 || paymentMethod.Length == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.CartItems = cartItems;
            ViewBag.CartSize = cartSize;
            ViewBag.DeliveryAddress = deliveryAddress;
            ViewBag.PaymentMethod = paymentMethod;
            ViewBag.Total = total;
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Confirm(int any) {
            List<OrderItem> cartItems = await CartHelper.GetOrderItemsAsync(Request, Response, context);
            decimal total = CartHelper.GetSubTotal(cartItems) + shippingFee;

            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            string paymentMethod = TempData["PaymentMethod"] as string ?? "";
            TempData.Keep();

            if (cartItems.Count == 0 || deliveryAddress.Length == 0 || paymentMethod.Length == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var currentUser =  await userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Home");
            }


            Order newOrder = new Order()
            {
                ClientId = currentUser.Id,
                DeliveryAddress= deliveryAddress,
                PaymentMethod = paymentMethod,
                PaymentDetails="",
                PaymentStatus= "Pending",
                Client=currentUser,
                Items= cartItems,
                OrderStatus= "Created",
                ShippingFee = shippingFee,
                TotalPrice= total,
                CreatedAt = DateTime.Now,
            };

            await context.Orders.AddAsync(newOrder);
            context.SaveChanges();

            //delete shopping cart
            Response.Cookies.Delete("shopping_cart");
            ViewBag.SuccessMessage = "Order placed successfully ✔";

            return View();
        }
    }
}
