
using System.Text.Json;

namespace MedicineInventory.Services;

public class JsonFileService
{
    private readonly string _dataDirectory;
    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public JsonFileService(IWebHostEnvironment environment)
    {
        _dataDirectory = Path.Combine(
            environment.ContentRootPath, "Data");

        Directory.CreateDirectory(_dataDirectory);

        EnsureFile("medicines.json");
        EnsureFile("sales.json");
    }

    private string GetPath(string fileName)
    {
        return Path.Combine(_dataDirectory, fileName);
    }

    private void EnsureFile(string fileName)
    {
        string path = GetPath(fileName);

        if (!File.Exists(path))
        {
            File.WriteAllText(path, "[]");
        }
    }

    public async Task<List<T>> ReadAsync<T>(string fileName)
    {
        string json = await File.ReadAllTextAsync(GetPath(fileName));

        return JsonSerializer.Deserialize<List<T>>(json, _options)
               ?? new List<T>();
    }

    public async Task WriteAsync<T>(
        string fileName, List<T> data)
    {
        string json = JsonSerializer.Serialize(data, _options);

        await File.WriteAllTextAsync(GetPath(fileName), json);
    }
}
