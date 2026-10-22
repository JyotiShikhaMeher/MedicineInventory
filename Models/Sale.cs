using System.ComponentModel.DataAnnotations;

namespace MedicineInventory.Models
{
    public class Sale
    {
        public int Id { get; set; }

        public int MedicineId { get; set; }

        public string MedicineName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int QuantitySold { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime SaleDate { get; set; }
    }
}
