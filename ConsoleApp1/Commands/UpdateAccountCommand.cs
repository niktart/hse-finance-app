public class UpdateAccountCommand : ICommand
{
    private readonly IFinancialService _financialService;
    private readonly Guid _accountId;
    private readonly string _newName;
    private string _oldName;
    private BankAccount _account;

    public UpdateAccountCommand(IFinancialService financialService, Guid accountId, string newName)
    {
        _financialService = financialService;
        _accountId = accountId;
        _newName = newName;
    }

    public void Execute()
    {
        _account = _financialService.GetAccount(_accountId);
        if (_account == null)
            throw new InvalidOperationException("Счет не найден");

        _oldName = _account.Name;
        _account.Name = _newName;
    }

    public void Undo()
    {
        if (_account != null)
        {
            _account.Name = _oldName;
        }
    }
}