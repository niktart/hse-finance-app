public class DeleteAccountCommand : ICommand
{
    private readonly IFinancialService _financialService;
    private readonly Guid _accountId;
    private BankAccount _deletedAccount;
    private List<Operation> _relatedOperations;

    public DeleteAccountCommand(IFinancialService financialService, Guid accountId)
    {
        _financialService = financialService;
        _accountId = accountId;
    }

    public void Execute()
    {
        var account = _financialService.GetAccount(_accountId);
        if (account == null)
            throw new InvalidOperationException("Счет не найден");

        // Сохраняю связанные операции для отката
        _relatedOperations = _financialService.GetAllOperations()
            .Where(o => o.BankAccountId == _accountId)
            .ToList();

        _deletedAccount = account;
        _financialService.DeleteAccount(_accountId);
    }

    public void Undo()
    {
        if (_deletedAccount != null)
        {
            Console.WriteLine("Восстановление счета после удаления не поддерживается");
        }
    }
}