//namespace MedicineInventory.Controllers
//{
//    public class SalesApiController
//    {
//    }
//}



using MedicineInventory.Models;
using MedicineInventory.Services;

using Microsoft.AspNetCore.Mvc;

namespace MedicineInventory.Controllers;

public class CreateSaleRequest
{
    public int MedicineId { get; set; }
    public int QuantitySold { get; set; }
}

[ApiController]
[Route("api/sales")]
public class SalesApiController : ControllerBase
{
    private readonly JsonFileService _json;

    public SalesApiController(JsonFileService json)
    {
        _json = json;
    }

    // GET: /api/sales
    [HttpGet]
    public async Task<ActionResult<List<Sale>>> GetSales()
    {
        var sales = await _json.ReadAsync<Sale>("sales.json");
        return Ok(sales.OrderByDescending(s => s.SaleDate).ToList());
    }

    // POST: /api/sales
    [HttpPost]
    public async Task<IActionResult> RecordSale(
        [FromBody] CreateSaleRequest request)
    {
        if (request.MedicineId <= 0 || request.QuantitySold <= 0)
        {
            return BadRequest(new
            {
                message = "Medicine ID and quantity sold must be positive."
            });
        }

        var medicines =
            await _json.ReadAsync<Medicine>("medicines.json");

        var medicine = medicines.FirstOrDefault(
            m => m.Id == request.MedicineId);

        if (medicine == null)
            return NotFound(new { message = "Medicine not found." });

        if (medicine.Quantity < request.QuantitySold)
        {
            return BadRequest(new
            {
                message = "Insufficient stock available."
            });
        }

        if (medicine.ExpiryDate.Date < DateTime.Today)
        {
            return BadRequest(new
            {
                message = "Expired medicine cannot be sold."
            });
        }

        var sales = await _json.ReadAsync<Sale>("sales.json");

        var sale = new Sale
        {
            Id = sales.Count == 0 ? 1 : sales.Max(s => s.Id) + 1,
            MedicineId = medicine.Id,
            MedicineName = medicine.FullName,
            QuantitySold = request.QuantitySold,
            UnitPrice = medicine.Price,
            TotalAmount = medicine.Price * request.QuantitySold,
            SaleDate = DateTime.Now
        };

        medicine.Quantity -= request.QuantitySold;

        sales.Add(sale);

        // Write the stock update and sale record.
        // See the note below about file-storage limitations.
        await _json.WriteAsync("medicines.json", medicines);
        await _json.WriteAsync("sales.json", sales);

        return Ok(sale);
    }
}

