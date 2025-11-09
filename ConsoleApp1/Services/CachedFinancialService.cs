public class CachedFinancialService : IFinancialService
{
    private readonly IFinancialService _realService;
    private List<BankAccount> _cachedAccounts;
    private List<Category> _cachedCategories;
    private List<Operation> _cachedOperations;
    private bool _accountsDirty = true;
    private bool _categoriesDirty = true;
    private bool _operationsDirty = true;

    public CachedFinancialService(IFinancialService realService)
    {
        _realService = realService;
    }

    public List<BankAccount> GetAllAccounts()
    {
        if (_accountsDirty || _cachedAccounts == null)
        {
            _cachedAccounts = _realService.GetAllAccounts();
            _accountsDirty = false;
        }
        return _cachedAccounts;
    }

    public List<Category> GetAllCategories()
    {
        if (_categoriesDirty || _cachedCategories == null)
        {
            _cachedCategories = _realService.GetAllCategories();
            _categoriesDirty = false;
        }
        return _cachedCategories;
    }

    public List<Operation> GetAllOperations()
    {
        if (_operationsDirty || _cachedOperations == null)
        {
            _cachedOperations = _realService.GetAllOperations();
            _operationsDirty = false;
        }
        return _cachedOperations;
    }

    // Методы для получения отдельных объектов
    public BankAccount GetAccount(Guid id) => GetAllAccounts().FirstOrDefault(a => a.Id == id);
    public Category GetCategory(Guid id) => GetAllCategories().FirstOrDefault(c => c.Id == id);
    public Operation GetOperation(Guid id) => GetAllOperations().FirstOrDefault(o => o.Id == id);

    // Методы создания (вызываем реальный сервис и инвалидируем кэш0
    public BankAccount? CreateAccount(string name, decimal initialBalance = 0)
    {
        var result = _realService.CreateAccount(name, initialBalance);
        if (result != null)
        {
            InvalidateAccounts();
        }
        return result;
    }

    public Category? CreateCategory(OperationType type, string name)
    {
        var result = _realService.CreateCategory(type, name);
        if (result != null)
        {
            InvalidateCategories();
        }
        return result;
    }

    public Operation? CreateOperation(OperationType type, Guid accountId, decimal amount, Guid categoryId, string description = "")
    {
        var result = _realService.CreateOperation(type, accountId, amount, categoryId, description);
        if (result != null)
        {
            InvalidateAccounts();
            InvalidateOperations();
        }
        return result;
    }

    // Методы удаления
    public void DeleteAccount(Guid id)
    {
        _realService.DeleteAccount(id);
        InvalidateAccounts();
        InvalidateOperations();
    }

    public void DeleteCategory(Guid id)
    {
        _realService.DeleteCategory(id);
        InvalidateCategories();
        InvalidateOperations();
    }

    public void DeleteOperation(Guid id)
    {
        _realService.DeleteOperation(id);
        InvalidateAccounts();
        InvalidateOperations();
    }

    // Остальные методы делегируем сервису
    public decimal GetTotalBalance() => _realService.GetTotalBalance();
    public (decimal incomes, decimal expenses) GetFinancialSummary() => _realService.GetFinancialSummary();
    public FinancialAnalytics GetFullAnalytics(DateTime? startDate = null, DateTime? endDate = null) => _realService.GetFullAnalytics(startDate, endDate);
    public List<CategoryAnalytics> GetTopExpenseCategories(int topCount = 5, DateTime? startDate = null, DateTime? endDate = null) => _realService.GetTopExpenseCategories(topCount, startDate, endDate);
    public Dictionary<DateTime, FinancialAnalytics> GetMonthlyAnalytics() => _realService.GetMonthlyAnalytics();
    public Dictionary<OperationType, decimal> GetOperationTypeDistribution() => _realService.GetOperationTypeDistribution();
    public void RecalculateBalances() => _realService.RecalculateBalances();
    public bool CheckDataIntegrity() => _realService.CheckDataIntegrity();

    // Методы для инвалидации кэша
    public void InvalidateAccounts() => _accountsDirty = true;
    public void InvalidateCategories() => _categoriesDirty = true;
    public void InvalidateOperations() => _operationsDirty = true;
}