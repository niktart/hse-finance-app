public class CreateOperationCommand : ICommand
{
    private readonly IFinancialService _financialService;
    private readonly OperationType _type;
    private readonly Guid _accountId;
    private readonly decimal _amount;
    private readonly Guid _categoryId;
    private readonly string _description;
    private Operation _createdOperation;

    public CreateOperationCommand(IFinancialService financialService, OperationType type,
        Guid accountId, decimal amount, Guid categoryId, string description = "")
    {
        _financialService = financialService;
        _type = type;
        _accountId = accountId;
        _amount = amount;
        _categoryId = categoryId;
        _description = description;
    }

    public void Execute()
    {
        var category = _financialService.GetAllCategories()
            .FirstOrDefault(c => c.Id == _categoryId);

        if (category != null && category.Type != _type)
        {
            throw new InvalidOperationException($"Тип операции ({_type}) не соответствует типу категории ({category.Type})");
        }

        // проверка баланса
        if (_type == OperationType.Expense)
        {
            var account = _financialService.GetAccount(_accountId);
            if (account != null && account.Balance < _amount)
            {
                throw new InvalidOperationException($"Недостаточно средств на счете. Баланс: {account.Balance} руб., требуется: {_amount} руб.");
            }
        }

        _createdOperation = _financialService.CreateOperation(_type, _accountId, _amount, _categoryId, _description);

        if (_createdOperation == null)
        {
            throw new InvalidOperationException("Не удалось создать операцию");
        }
    }

    public void Undo()
    {
        if (_createdOperation != null)
        {
            // Откатываю изменение баланса 
            var account = _financialService.GetAccount(_accountId);
            if (account != null)
            {
                if (_type == OperationType.Income)
                    account.UpdateBalance(-_amount);
                else
                    account.UpdateBalance(_amount);
            }

            _financialService.DeleteOperation(_createdOperation.Id);
            Console.WriteLine($"Операция отменена: {_type} - {_amount}");
        }
    }
}