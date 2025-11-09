public class ImportDataCommand : ICommand
{
    private readonly IFinancialService _financialService;
    private readonly DataSerializer _serializer;
    private readonly string _filePath;
    private List<BankAccount> _backupAccounts;
    private List<Category> _backupCategories;
    private List<Operation> _backupOperations;

    public ImportDataCommand(IFinancialService financialService, DataSerializer serializer, string filePath)
    {
        _financialService = financialService;
        _serializer = serializer;
        _filePath = filePath;
    }

    public void Execute()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException($"Файл не найден: {_filePath}");

        // Создаем резервную копию текущих данных
        _backupAccounts = new List<BankAccount>(_financialService.GetAllAccounts());
        _backupCategories = new List<Category>(_financialService.GetAllCategories());
        _backupOperations = new List<Operation>(_financialService.GetAllOperations());

        // Импортируем новые данные
        var importData = _serializer.ImportData(_filePath);

        // Очищаем текущие данные
        ClearAllData();

        // Добавляем импортированные данные через методы сервиса
        foreach (var account in importData.Accounts)
        {
            _financialService.CreateAccount(account.Name, account.Balance);
        }

        foreach (var category in importData.Categories)
        {
            _financialService.CreateCategory(category.Type, category.Name);
        }

        foreach (var operation in importData.Operations)
        {
            _financialService.CreateOperation(operation.Type, operation.BankAccountId,
                operation.Amount, operation.CategoryId, operation.Description);
        }

        Console.WriteLine($"Данные импортированы из: {_filePath}");
        Console.WriteLine($"   Счетов: {importData.Accounts.Count}");
        Console.WriteLine($"   Категорий: {importData.Categories.Count}");
        Console.WriteLine($"   Операций: {importData.Operations.Count}");
    }

    public void Undo()
    {
        // Восстанавливаем данные из резервной копии
        ClearAllData();

        // Восстанавливаем через методы сервиса
        foreach (var account in _backupAccounts)
        {
            _financialService.CreateAccount(account.Name, account.Balance);
        }

        foreach (var category in _backupCategories)
        {
            _financialService.CreateCategory(category.Type, category.Name);
        }

        foreach (var operation in _backupOperations)
        {
            _financialService.CreateOperation(operation.Type, operation.BankAccountId,
                operation.Amount, operation.CategoryId, operation.Description);
        }

        Console.WriteLine("Импорт отменен, данные восстановлены");
    }

    private void ClearAllData()
    {
        // Очищаем данные через методы удаления
        var accounts = _financialService.GetAllAccounts();
        var categories = _financialService.GetAllCategories();
        var operations = _financialService.GetAllOperations();

        // Удаляем в правильном порядке (сначала операции, потом категории и счета)
        foreach (var operation in operations.ToList())
        {
            _financialService.DeleteOperation(operation.Id);
        }

        foreach (var category in categories.ToList())
        {
            _financialService.DeleteCategory(category.Id);
        }

        foreach (var account in accounts.ToList())
        {
            _financialService.DeleteAccount(account.Id);
        }
    }
}