public class FinancialService : IFinancialService
{
    private readonly List<BankAccount> _accounts = new();
    private readonly List<Category> _categories = new();
    private readonly List<Operation> _operations = new();
    private readonly FinancialFactory _factory = new();

    // Методы создания
    public BankAccount? CreateAccount(string name, decimal initialBalance = 0)
    {
        var account = _factory.CreateBankAccount(name, initialBalance);
        if (account != null)
        {
            _accounts.Add(account);
        }
        return account;
    }

    public Category? CreateCategory(OperationType type, string name)
    {
        var category = _factory.CreateCategory(type, name);
        if (category != null)
        {
            _categories.Add(category);
        }
        return category;
    }

    public Operation? CreateOperation(OperationType type, Guid accountId,
                                   decimal amount, Guid categoryId, string description = "")
    {
        var account = GetAccount(accountId);
        if (account == null)
        {
            Console.WriteLine("❌ Счет не найден");
            return null;
        }

        var operation = _factory.CreateOperation(type, accountId, amount, categoryId, description);
        if (operation == null)
        {
            Console.WriteLine("❌ Не удалось создать операцию: неверные данные");
            return null;
        }

        if (type == OperationType.Income)
            account.UpdateBalance(amount);
        else
            account.UpdateBalance(-amount);

        _operations.Add(operation);
        return operation;
    }

    // Методы получения данных
    public List<BankAccount> GetAllAccounts() => _accounts;
    public BankAccount GetAccount(Guid id) => _accounts.FirstOrDefault(a => a.Id == id);
    public List<Category> GetAllCategories() => _categories;
    public Category GetCategory(Guid id) => _categories.FirstOrDefault(c => c.Id == id);
    public List<Operation> GetAllOperations() => _operations;
    public Operation GetOperation(Guid id) => _operations.FirstOrDefault(o => o.Id == id);

    // Методы удаления
    public void DeleteAccount(Guid id) => _accounts.RemoveAll(a => a.Id == id);
    public void DeleteCategory(Guid id) => _categories.RemoveAll(c => c.Id == id);
    public void DeleteOperation(Guid id) => _operations.RemoveAll(o => o.Id == id);

    // Базовые методы
    public decimal GetTotalBalance() => _accounts.Sum(a => a.Balance);

    public (decimal incomes, decimal expenses) GetFinancialSummary()
    {
        var incomes = _operations
            .Where(o => o.Type == OperationType.Income)
            .Sum(o => o.Amount);

        var expenses = _operations
            .Where(o => o.Type == OperationType.Expense)
            .Sum(o => o.Amount);

        return (incomes, expenses);
    }

    // аналитика
    public FinancialAnalytics GetFullAnalytics(DateTime? startDate = null, DateTime? endDate = null)
    {
        var operations = GetAllOperations();

        if (startDate.HasValue)
            operations = operations.Where(o => o.Date >= startDate.Value).ToList();
        if (endDate.HasValue)
            operations = operations.Where(o => o.Date <= endDate.Value).ToList();

        if (!operations.Any())
        {
            return new FinancialAnalytics
            {
                TotalIncome = 0,
                TotalExpense = 0,
                NetIncome = 0,
                SavingsRate = 0,
                IncomeByCategory = new List<CategoryAnalytics>(),
                ExpenseByCategory = new List<CategoryAnalytics>(),
                PeriodStart = startDate ?? DateTime.Now,
                PeriodEnd = endDate ?? DateTime.Now
            };
        }

        var incomes = operations.Where(o => o.Type == OperationType.Income);
        var expenses = operations.Where(o => o.Type == OperationType.Expense);

        var totalIncome = incomes.Sum(o => o.Amount);
        var totalExpense = expenses.Sum(o => o.Amount);
        var netIncome = totalIncome - totalExpense;
        var savingsRate = totalIncome > 0 ? (netIncome / totalIncome) * 100 : 0;

        var incomeByCategory = incomes
            .GroupBy(o => o.CategoryId)
            .Select(g => new CategoryAnalytics
            {
                CategoryName = GetCategory(g.Key)?.Name ?? "Неизвестная категория",
                CategoryType = OperationType.Income,
                TotalAmount = g.Sum(o => o.Amount),
                OperationsCount = g.Count(),
                Percentage = totalIncome > 0 ? (g.Sum(o => o.Amount) / totalIncome) * 100 : 0
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        var expenseByCategory = expenses
            .GroupBy(o => o.CategoryId)
            .Select(g => new CategoryAnalytics
            {
                CategoryName = GetCategory(g.Key)?.Name ?? "Неизвестная категория",
                CategoryType = OperationType.Expense,
                TotalAmount = g.Sum(o => o.Amount),
                OperationsCount = g.Count(),
                Percentage = totalExpense > 0 ? (g.Sum(o => o.Amount) / totalExpense) * 100 : 0
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();

        return new FinancialAnalytics
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetIncome = netIncome,
            SavingsRate = savingsRate,
            IncomeByCategory = incomeByCategory,
            ExpenseByCategory = expenseByCategory,
            PeriodStart = startDate ?? operations.Min(o => o.Date),
            PeriodEnd = endDate ?? operations.Max(o => o.Date)
        };
    }

    public List<CategoryAnalytics> GetTopExpenseCategories(int topCount = 5, DateTime? startDate = null, DateTime? endDate = null)
    {
        var operations = GetAllOperations();

        if (startDate.HasValue)
            operations = operations.Where(o => o.Date >= startDate.Value).ToList();
        if (endDate.HasValue)
            operations = operations.Where(o => o.Date <= endDate.Value).ToList();

        var expenses = operations.Where(o => o.Type == OperationType.Expense);

        if (!expenses.Any())
            return new List<CategoryAnalytics>();

        return expenses
            .GroupBy(o => o.CategoryId)
            .Select(g => new CategoryAnalytics
            {
                CategoryName = GetCategory(g.Key)?.Name ?? "Неизвестная категория",
                CategoryType = OperationType.Expense,
                TotalAmount = g.Sum(o => o.Amount),
                OperationsCount = g.Count()
            })
            .OrderByDescending(c => c.TotalAmount)
            .Take(topCount)
            .ToList();
    }

    public Dictionary<DateTime, FinancialAnalytics> GetMonthlyAnalytics()
    {
        var operations = GetAllOperations();

        if (!operations.Any())
            return new Dictionary<DateTime, FinancialAnalytics>();

        return operations
            .GroupBy(o => new DateTime(o.Date.Year, o.Date.Month, 1))
            .ToDictionary(
                g => g.Key,
                g => GetFullAnalytics(g.Key, g.Key.AddMonths(1).AddDays(-1))
            );
    }

    public Dictionary<OperationType, decimal> GetOperationTypeDistribution()
    {
        var operations = GetAllOperations();

        if (!operations.Any())
            return new Dictionary<OperationType, decimal>();

        var total = operations.Sum(o => o.Amount);

        return operations
            .GroupBy(o => o.Type)
            .ToDictionary(
                g => g.Key,
                g => total > 0 ? (g.Sum(o => o.Amount) / total) * 100 : 0
            );
    }


    public void RecalculateBalances()
    {
        foreach (var account in _accounts)
        {
            var calculatedBalance = _operations
                .Where(o => o.BankAccountId == account.Id)
                .Sum(o => o.Type == OperationType.Income ? o.Amount : -o.Amount);

            account.Balance = calculatedBalance;
        }
    }

    public bool CheckDataIntegrity()
    {
        foreach (var account in _accounts)
        {
            var calculatedBalance = _operations
                .Where(o => o.BankAccountId == account.Id)
                .Sum(o => o.Type == OperationType.Income ? o.Amount : -o.Amount);

            if (account.Balance != calculatedBalance)
                return false;
        }
        return true;
    }   
}

