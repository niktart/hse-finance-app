public class Category
{
    public Guid Id { get; set; }
    public OperationType Type { get; set; }
    public string Name { get; set; }

    public Category() { }

    public Category(OperationType type, string name)
    {
        Id = Guid.NewGuid();
        Type = type;
        Name = name;
    }
}