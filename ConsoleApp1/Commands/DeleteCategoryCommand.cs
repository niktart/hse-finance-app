public class DeleteCategoryCommand : ICommand
{
    private readonly IFinancialService _financialService;
    private readonly Guid _categoryId;
    private Category _deletedCategory;
    private List<Operation> _relatedOperations;

    public DeleteCategoryCommand(IFinancialService financialService, Guid categoryId)
    {
        _financialService = financialService;
        _categoryId = categoryId;
    }

    public void Execute()
    {
        var category = _financialService.GetCategory(_categoryId);
        if (category == null)
            throw new InvalidOperationException("Категория не найдена");

        // Сохраняю связанные операции для отката
        _relatedOperations = _financialService.GetAllOperations()
            .Where(o => o.CategoryId == _categoryId)
            .ToList();

        _deletedCategory = category;
        _financialService.DeleteCategory(_categoryId);
    }

    public void Undo()
    {
        if (_deletedCategory != null)
        {
            // Восстанавливаю категорию и связанные операции
            Console.WriteLine("Восстановление категории после удаления не поддерживается");
        }
    }
}