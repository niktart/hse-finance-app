public interface ICommand
{
    void Execute();  // Выполнить команду
    void Undo();     // Отменить команду
}