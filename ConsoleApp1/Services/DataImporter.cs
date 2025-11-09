using System.Text.Json;

public abstract class DataImporter
{
    protected readonly FinancialService _financialService;

    public DataImporter(FinancialService financialService)
    {
        _financialService = financialService;
    }

    public void Import(string filePath) 
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Файл не найден: {filePath}");

        var data = ReadData(filePath);
        ValidateData(data);
        SaveData(data);

        Console.WriteLine($"Данные успешно импортированы: {data.Accounts.Count} счетов, " +
                         $"{data.Categories.Count} категорий, {data.Operations.Count} операций");
    }

    protected abstract ExportData ReadData(string filePath);

    protected virtual void ValidateData(ExportData data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (data.Accounts == null || data.Categories == null || data.Operations == null)
            throw new InvalidDataException("Некорректные данные в файле");
    }

    protected virtual void SaveData(ExportData data)
    {
        // Очищаем текущие данные
        ClearCurrentData();

        // Добавляем импортированные данные
        foreach (var account in data.Accounts)
            _financialService.GetAllAccounts().Add(account);

        foreach (var category in data.Categories)
            _financialService.GetAllCategories().Add(category);

        foreach (var operation in data.Operations)
            _financialService.GetAllOperations().Add(operation);
    }

    private void ClearCurrentData()
    {
        _financialService.GetAllAccounts().Clear();
        _financialService.GetAllCategories().Clear();
        _financialService.GetAllOperations().Clear();
    }
}

public class FinancialJsonDataImporter : DataImporter
{
    public FinancialJsonDataImporter(FinancialService financialService) : base(financialService) { }

    protected override ExportData ReadData(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return JsonSerializer.Deserialize<ExportData>(json, options);
    }
}