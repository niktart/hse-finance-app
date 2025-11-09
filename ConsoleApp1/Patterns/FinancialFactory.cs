public class FinancialFactory
{
    public BankAccount CreateBankAccount(string name, decimal initialBalance = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name cannot be empty");

        if (initialBalance < 0)
            throw new ArgumentException("Initial balance cannot be negative");

        return new BankAccount(name, initialBalance);
    }

    public Category CreateCategory(OperationType type, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name cannot be empty");

        return new Category(type, name);
    }

    public Operation CreateOperation(OperationType type, Guid bankAccountId,
                                   decimal amount, Guid categoryId, string description = "")
    {
        return new Operation(type, bankAccountId, amount, categoryId, description);
    }
}