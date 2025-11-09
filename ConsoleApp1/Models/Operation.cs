public class Operation
{
    public Guid Id { get; set; }
    public OperationType Type { get; set; }
    public Guid BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public Guid CategoryId { get; set; }

    public Operation() { }

    public Operation(OperationType type, Guid bankAccountId, decimal amount,
                    Guid categoryId, string description = "")
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");

        Id = Guid.NewGuid();
        Type = type;
        BankAccountId = bankAccountId;
        Amount = amount;
        CategoryId = categoryId;
        Description = description;
        Date = DateTime.Now;
    }
}