using System.ComponentModel.DataAnnotations;

namespace MedicineInventory.Models
{
    public class Medicine
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(typeof(decimal), "0", "999999999")]
        public decimal Price { get; set; }

        [Required]
        public string Brand { get; set; } = string.Empty;
    }
}
