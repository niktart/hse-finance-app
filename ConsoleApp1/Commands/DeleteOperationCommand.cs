public class DeleteOperationCommand : ICommand
{
    private readonly IFinancialService _financialService;
    private readonly Guid _operationId;
    private Operation _deletedOperation;
    private BankAccount _affectedAccount;

    public DeleteOperationCommand(IFinancialService financialService, Guid operationId)
    {
        _financialService = financialService;
        _operationId = operationId;
    }

    public void Execute()
    {
        var operation = _financialService.GetAllOperations()
            .FirstOrDefault(o => o.Id == _operationId);

        if (operation == null)
            throw new InvalidOperationException("Операция не найдена");

        _affectedAccount = _financialService.GetAccount(operation.BankAccountId);
        _deletedOperation = operation;

        // Восстанавливаю баланс при удалении операции
        if (operation.Type == OperationType.Income)
            _affectedAccount?.UpdateBalance(-operation.Amount);
        else
            _affectedAccount?.UpdateBalance(operation.Amount);

        _financialService.DeleteOperation(_operationId);
    }

    public void Undo()
    {
        if (_deletedOperation != null && _affectedAccount != null)
        {
            // Восстанавливаю операцию и баланс
            Console.WriteLine("⚠️ Восстановление операции после удаления не поддерживается");
        }
    }
}