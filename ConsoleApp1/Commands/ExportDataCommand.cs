public class ExportDataCommand : ICommand
{
    private readonly IFinancialService _financialService;
    private readonly DataSerializer _serializer;
    private readonly string _filePath;

    public ExportDataCommand(IFinancialService financialService, DataSerializer serializer, string filePath)
    {
        _financialService = financialService;
        _serializer = serializer;
        _filePath = filePath;
    }

    public void Execute()
    {
        var exportData = new ExportData
        {
            Accounts = _financialService.GetAllAccounts(),
            Categories = _financialService.GetAllCategories(),
            Operations = _financialService.GetAllOperations()
        };

        _serializer.ExportData(exportData, _filePath);
        Console.WriteLine($"Данные экспортированы в: {_filePath}");
    }

    public void Undo()
    {
        // Отмена экспорта - удаляем файл
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
            Console.WriteLine($"Файл удален: {_filePath}");
        }
    }
}