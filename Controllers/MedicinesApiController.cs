//namespace MedicineInventory.Controllers
//{
//    public class MedicinesApiController
//    {
//    }
//}



using MedicineInventory.Models;
using MedicineInventory.Services;
using Microsoft.AspNetCore.Mvc;

namespace MedicineInventory.Controllers;

[ApiController]
[Route("api/medicines")]
public class MedicinesApiController : ControllerBase
{
    private readonly JsonFileService _json;

    public MedicinesApiController(JsonFileService json)
    {
        _json = json;
    }

    // GET: /api/medicines?search=paracetamol
    [HttpGet]
    public async Task<ActionResult<List<Medicine>>> GetMedicines(
        [FromQuery] string? search)
    {
        var medicines =
            await _json.ReadAsync<Medicine>("medicines.json");

        if (!string.IsNullOrWhiteSpace(search))
        {
            medicines = medicines
                .Where(m => m.FullName.Contains(
                    search.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return Ok(medicines);
    }

    // GET: /api/medicines/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Medicine>> GetMedicine(int id)
    {
        var medicines =
            await _json.ReadAsync<Medicine>("medicines.json");

        var medicine = medicines.FirstOrDefault(m => m.Id == id);

        if (medicine == null)
            return NotFound(new { message = "Medicine not found." });

        return Ok(medicine);
    }

    // POST: /api/medicines
    [HttpPost]
    public async Task<ActionResult<Medicine>> AddMedicine(
        [FromBody] Medicine medicine)
    {
        if (medicine.ExpiryDate == default)
            return BadRequest(new { message = "Expiry date is required." });

        if (medicine.ExpiryDate.Kind == DateTimeKind.Utc)
            medicine.ExpiryDate = medicine.ExpiryDate.Date;

        var medicines =
            await _json.ReadAsync<Medicine>("medicines.json");

        medicine.Id = medicines.Count == 0
            ? 1
            : medicines.Max(m => m.Id) + 1;

        medicines.Add(medicine);

        await _json.WriteAsync("medicines.json", medicines);

        return CreatedAtAction(
            nameof(GetMedicine),
            new { id = medicine.Id },
            medicine);
    }

    // PUT: /api/medicines/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMedicine(
        int id, [FromBody] Medicine updated)
    {
        if (updated.ExpiryDate == default)
            return BadRequest(new { message = "Expiry date is required." });

        var medicines =
            await _json.ReadAsync<Medicine>("medicines.json");

        var medicine = medicines.FirstOrDefault(m => m.Id == id);

        if (medicine == null)
            return NotFound(new { message = "Medicine not found." });

        medicine.FullName = updated.FullName;
        medicine.Notes = updated.Notes;
        medicine.ExpiryDate = updated.ExpiryDate;
        medicine.Quantity = updated.Quantity;
        medicine.Price = updated.Price;
        medicine.Brand = updated.Brand;

        await _json.WriteAsync("medicines.json", medicines);

        return NoContent();
    }

    // DELETE: /api/medicines/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMedicine(int id)
    {
        var medicines =
            await _json.ReadAsync<Medicine>("medicines.json");

        var medicine = medicines.FirstOrDefault(m => m.Id == id);

        if (medicine == null)
            return NotFound(new { message = "Medicine not found." });

        medicines.Remove(medicine);

        await _json.WriteAsync("medicines.json", medicines);

        return NoContent();
    }
}
