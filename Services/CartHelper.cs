using System.Text.Json;
using Azure.Core;
using E_commerce.Models;

namespace E_commerce.Services
{
    public class CartHelper
    {
        public static Dictionary<int,int> GetCart(HttpRequest request , HttpResponse response)
        {
            var cookieValue = request.Cookies["shopping_cart"] ?? "";
            try
            {
                var cart = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cookieValue));
                Console.WriteLine($"Cart Helper cart= {cookieValue} -> {cart}");
                var dictionary = JsonSerializer.Deserialize<Dictionary<int,int>>(cart);
                if (dictionary != null) { 
                    return dictionary;
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
            if (cookieValue.Length > 0) { 
                //cookie not valid
                response.Cookies.Delete("shopping_cart");
            }
            return new Dictionary<int, int>();
        }

        public static int getCartSize(HttpRequest request, HttpResponse response) {
            Dictionary<int, int> cart = GetCart(request, response);
            int cartSize = 0;
            foreach (var keyValuePair in cart) {
                cartSize += keyValuePair.Value;
            }
            return cartSize;
        }

        public static async Task<List<OrderItem>> GetOrderItemsAsync(HttpRequest request, HttpResponse response, ApplicationContext context)
        {
            Dictionary<int,int> cart =GetCart(request, response);
            var cartItems = new List<OrderItem>();
            foreach (var keyValuePair in cart) {
                int quantity = keyValuePair.Value;
                int productId = keyValuePair.Key;
                var product = await context.Products.FindAsync(productId);
                if (product == null)
                    continue;
                OrderItem orderItem = new OrderItem()
                {
                    UnitPrice = product.Price,  
                    Product= product,
                    Quantity= quantity,
                };
                cartItems.Add(orderItem);
            }
            return cartItems;
        }

        public static decimal GetSubTotal(List<OrderItem> orderItems)
        {
            decimal subTotal = 0;
            foreach (var orderItem in orderItems) {
                subTotal += (orderItem.Quantity* orderItem.UnitPrice);
            }
            return subTotal;
        }
    }
}
