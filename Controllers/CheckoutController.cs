using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Core;
using E_commerce.Models;
using E_commerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.Extensions.Hosting;
using static System.Net.Mime.MediaTypeNames;

namespace E_commerce.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationContext context;
        private readonly decimal shippingFee;

        private  string PayPalclientId { get; set; } = "";
        private  string PayPalSecretKey { get; set; } = "";
        private  string PayPalUrl { get; set; } = "";

        public CheckoutController(IConfiguration configuration , UserManager<ApplicationUser> userManager , ApplicationContext context)
        {
            
            PayPalclientId = configuration["PayPalSettings:ClientId"]!;
            PayPalSecretKey = configuration["PayPalSettings:SecretKey"]!;
            PayPalUrl = configuration["PayPalSettings:Url"]!;
            this.userManager = userManager;
            this.context = context;
            this.shippingFee = configuration.GetValue<decimal>("shippingFee");
        }
        public async Task<IActionResult> Index()
        {
            List<OrderItem> cartItems = await CartHelper.GetOrderItemsAsync(Request, Response, context);
            decimal totalAmount = CartHelper.GetSubTotal(cartItems) + shippingFee;

            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            TempData.Keep();

            ViewBag.ClientId = PayPalclientId;
            ViewBag.Total = totalAmount;
            ViewBag.DeliveryAddress = deliveryAddress;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder()
        {
            List<OrderItem> cartItems = await CartHelper.GetOrderItemsAsync(Request, Response, context);
            decimal totalAmount = CartHelper.GetSubTotal(cartItems) + shippingFee;

            if (cartItems.Count == 0)
            {
                ViewBag.ErrorMessages = "Your cart is empty!";
                return View("Index");
            }

            string deliveryAddress = TempData["DeliveryAddress"] as string ?? "";
            TempData.Keep();

            if (totalAmount == null)
            {
                return new JsonResult( new {Id=""});
            }
            string url = PayPalUrl + "/v2/checkout/orders";
            string accessToken = await GetPayPalAccessTokenAsync();


            #region /*prepare message body*/
            JsonObject createOrderRequest = new JsonObject();

            JsonObject amount = new JsonObject();
            amount.Add("currency_code", "USD");
            amount.Add("value" , totalAmount);
                
            JsonObject purchaseUnit1 = new JsonObject();
            purchaseUnit1.Add("amount",amount);

            JsonArray purchaseUnits = new JsonArray();

            purchaseUnits.Add(purchaseUnit1);

            createOrderRequest.Add("purchase_units", purchaseUnits);
            createOrderRequest.Add("intent", "CAPTURE");
            #endregion
            
            using(var client = new HttpClient())
            {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);

                httpRequestMessage.Headers.Add("Authorization", "Bearer "+ accessToken);
                httpRequestMessage.Content = new StringContent(createOrderRequest.ToString(), null, "application/json");

                var httpResponse =  await client.SendAsync(httpRequestMessage);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var responseStr = await httpResponse.Content.ReadAsStringAsync();

                    var responseJson = JsonNode.Parse(responseStr);

                    if (responseJson != null)
                    {

                        var paypalId = responseJson["id"]?.ToString()?? "";
                        return new JsonResult(new { Id = paypalId });
                    }
                }
            }
            return new JsonResult(new { Id = "" });
        }
        [HttpPost]
        public async Task<IActionResult> CompleteOrderAsync([FromBody] JsonObject data)
        {
            var orderId = data["orderID"]?.ToString();
            var deliveryAddress = data["deliveryAddress"]?.ToString();

            if (orderId == null)
            {
                return new JsonResult("error");
            }

            //capture payment for order
            using (var client = new HttpClient())
            {
                string url = $"{PayPalUrl}/v2/checkout/orders/{orderId}/capture";
                string accessToken = await GetPayPalAccessTokenAsync();
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Headers.Add("Authorization", "Bearer " + accessToken );
                requestMessage.Content =new StringContent("", null, "application/json");

                var httpresponse = await client.SendAsync(requestMessage );
               
                if (httpresponse.IsSuccessStatusCode)
                {
                    string responseStr = await httpresponse.Content.ReadAsStringAsync();
                    var responseJson = JsonNode.Parse(responseStr);
                    if (responseJson != null) {
                        string paypalOrderStatus = responseJson["status"]?.ToString()?? "";
                        if(paypalOrderStatus== "COMPLETED")
                        {
                            await SaveOrderAsync( responseJson.ToString() , deliveryAddress);
                            return new JsonResult("success");
                        }
                    }
                }
            }
            return new JsonResult("error");

         }

        private async Task SaveOrderAsync(string paypalResponse , string deliveryAddress)
        {
            List<OrderItem> cartItems = await CartHelper.GetOrderItemsAsync(Request, Response, context);
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return ;
            }
            var newOrder = new Order() {
                ClientId = appUser.Id,
                PaymentMethod = "paypal",
                PaymentStatus = "accepted",
                PaymentDetails = paypalResponse,
                CreatedAt = DateTime.Now,
                OrderStatus = "Created",
                ShippingFee = shippingFee,
                Items = cartItems,
                DeliveryAddress = deliveryAddress,
            }; 

            context.Orders.Add(newOrder);
            context.SaveChanges();
            Response.Cookies.Delete("shopping_cart");

        }

        private async Task<string> GetPayPalAccessTokenAsync()
        {
            string accessToken = "";
            using (HttpClient client = new HttpClient())
            {
                string credentials64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                    PayPalclientId + ":" + PayPalSecretKey
                ));

                string url = PayPalUrl + "/v1/oauth2/token";
                var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                    url);

                requestMessage.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials64);

                requestMessage.Content = new StringContent(
                    "grant_type=client_credentials",
                    null,
                    "application/x-www-form-urlencoded");

                var httpResponse = await client.SendAsync(requestMessage);

                var body = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine("Response: " + body);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var json = JsonNode.Parse(body);
                    accessToken = json?["access_token"]?.ToString() ?? "";
                }
                else
                {
                    Console.WriteLine("PayPal ERROR " + httpResponse.StatusCode + ":\n" + body);
                }
            }

            return accessToken;
        }
    }
}
