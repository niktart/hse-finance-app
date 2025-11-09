using System.Text.Json;

public interface IDataVisitor
{
    void Visit(BankAccount account);
    void Visit(Category category);
    void Visit(Operation operation);
}

public class JsonExportVisitor : IDataVisitor
{
    private readonly JsonSerializerOptions _options;
    private readonly List<object> _data = new();

    public JsonExportVisitor()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public void Visit(BankAccount account)
    {
        _data.Add(new
        {
            Type = "Account",
            Id = account.Id,
            Name = account.Name,
            Balance = account.Balance
        });
    }

    public void Visit(Category category)
    {
        _data.Add(new
        {
            Type = "Category",
            Id = category.Id,
            Name = category.Name,
            CategoryType = category.Type
        });
    }

    public void Visit(Operation operation)
    {
        _data.Add(new
        {
            Type = "Operation",
            Id = operation.Id,
            OperationType = operation.Type,
            Amount = operation.Amount,
            Date = operation.Date,
            Description = operation.Description
        });
    }

    public string GetJson() => JsonSerializer.Serialize(_data, _options);
}