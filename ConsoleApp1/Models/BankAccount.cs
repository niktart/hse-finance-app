public class BankAccount
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Balance { get; set; }
    public BankAccount() { }

    public BankAccount(string name, decimal initialBalance = 0)
    {
        Id = Guid.NewGuid();
        Name = name;
        Balance = initialBalance;
    }

    public void UpdateBalance(decimal amount)
    {
        Balance += amount;
    }
}