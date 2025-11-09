public class TimingCommandDecorator : ICommand
{
    private readonly ICommand _command;

    public TimingCommandDecorator(ICommand command)
    {
        _command = command;
    }

    public void Execute()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _command.Execute();
        stopwatch.Stop();
        Console.WriteLine($"Время выполнения: {stopwatch.ElapsedMilliseconds}ms");
    }

    public void Undo()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        _command.Undo();
        stopwatch.Stop();
        Console.WriteLine($"Время отмены: {stopwatch.ElapsedMilliseconds}ms");
    }
}