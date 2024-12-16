using ProniaApplication.Models;

namespace ProniaApplication.Areas.Admin.Models
{
    public class AdminOrder:BaseEntity
    {
        public string ProductName { get; set; }

        public int ProductPrice { get; set; }

        public int Count { get; set; }

        

        public decimal TotalPrice { get; set; }
    }
}
