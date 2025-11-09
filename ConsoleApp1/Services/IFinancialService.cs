public interface IFinancialService
{
    // Методы создания
    BankAccount? CreateAccount(string name, decimal initialBalance = 0);
    Category? CreateCategory(OperationType type, string name);
    Operation? CreateOperation(OperationType type, Guid accountId, decimal amount, Guid categoryId, string description = "");

    // Методы получения данных
    List<BankAccount> GetAllAccounts();
    BankAccount GetAccount(Guid id);
    List<Category> GetAllCategories();
    Category GetCategory(Guid id);
    List<Operation> GetAllOperations();
    Operation GetOperation(Guid id);

    // Методы удаления
    void DeleteAccount(Guid id);
    void DeleteCategory(Guid id);
    void DeleteOperation(Guid id);

    // Базовые методы
    decimal GetTotalBalance();
    (decimal incomes, decimal expenses) GetFinancialSummary();

    // Методы аналитики
    FinancialAnalytics GetFullAnalytics(DateTime? startDate = null, DateTime? endDate = null);
    List<CategoryAnalytics> GetTopExpenseCategories(int topCount = 5, DateTime? startDate = null, DateTime? endDate = null);
    Dictionary<DateTime, FinancialAnalytics> GetMonthlyAnalytics();
    Dictionary<OperationType, decimal> GetOperationTypeDistribution();

    // Управление данными
    void RecalculateBalances();
    bool CheckDataIntegrity();
}   