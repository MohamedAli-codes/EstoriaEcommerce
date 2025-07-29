using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace E_commerce.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        [Precision(16,2)]
        public decimal UnitPrice { get; set; }

        public virtual Product Product { get; set; } = new Product();
    }
}
