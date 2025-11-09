public class UpdateCategoryCommand : ICommand
{
    private readonly IFinancialService _financialService;
    private readonly Guid _categoryId;
    private readonly string _newName;
    private string _oldName;
    private Category _category;

    public UpdateCategoryCommand(IFinancialService financialService, Guid categoryId, string newName)
    {
        _financialService = financialService;
        _categoryId = categoryId;
        _newName = newName;
    }

    public void Execute()
    {
        _category = _financialService.GetCategory(_categoryId);
        if (_category == null)
            throw new InvalidOperationException("Категория не найдена");

        _oldName = _category.Name;
        _category.Name = _newName;
    }

    public void Undo()
    {
        if (_category != null)
        {
            _category.Name = _oldName;
        }
    }
}