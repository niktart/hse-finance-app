public class CreateCategoryCommand : ICommand
{
    private readonly IFinancialService _financialService;
    private readonly OperationType _type;
    private readonly string _name;
    private Category _createdCategory;

    public CreateCategoryCommand(IFinancialService financialService, OperationType type, string name)
    {
        _financialService = financialService;
        _type = type;
        _name = name;
    }

    public void Execute()
    {
        _createdCategory = _financialService.CreateCategory(_type, _name);
        if (_createdCategory == null)
        {
            throw new InvalidOperationException("Не удалось создать категорию");
        }
    }

    public void Undo()
    {
        if (_createdCategory != null)
        {
            _financialService.DeleteCategory(_createdCategory.Id);
        }
    }
}