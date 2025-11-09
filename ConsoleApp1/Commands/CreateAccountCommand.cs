public class CreateAccountCommand : ICommand
{
    private readonly IFinancialService _service;
    private readonly string _name;
    private readonly decimal _balance;
    private BankAccount _createdAccount;

    public CreateAccountCommand(IFinancialService service, string name, decimal balance = 0)
    {
        _service = service;
        _name = name;
        _balance = balance;
    }

    public void Execute()
    {
        _createdAccount = _service.CreateAccount(_name, _balance);
        if (_createdAccount == null)
        {
            throw new InvalidOperationException("Не удалось создать счет");
        }
        Console.WriteLine($"Счет создан: {_createdAccount.Name} (Баланс: {_createdAccount.Balance})");
    }

    public void Undo()
    {
        if (_createdAccount != null)
        {
            _service.DeleteAccount(_createdAccount.Id);
            Console.WriteLine($"Счет удален: {_createdAccount.Name}");
        }
    }
}