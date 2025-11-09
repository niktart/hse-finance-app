using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

class Program
{
    private static DIContainer _container = new DIContainer();
    private static IFinancialService _financialService;
    private static List<ICommand> _commandHistory = new List<ICommand>();

    static void Main(string[] args)
    {
        // Настройка DI-контейнера
        ConfigureContainer();

        // Получаем сервис через интерфейс
        _financialService = _container.Resolve<IFinancialService>();

        Console.WriteLine("ВШЭ-Банк - Учет финансов");
        Console.WriteLine("------------------------");

        InitializeDefaultCategories();

        while (true)
        {
            ShowMenu();
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    CreateAccount();
                    break;
                case "2":
                    CreateCategory();
                    break;
                case "3":
                    CreateOperation();
                    break;
                case "4":
                    ShowAccounts();
                    break;
                case "5":
                    ShowAnalytics();
                    break;
                case "6":
                    UndoLastCommand();
                    break;
                case "7":
                    ManageAccounts();
                    break;
                case "8":
                    ManageCategories();
                    break;
                case "9":
                    ManageOperations();
                    break;
                case "10":
                    ShowAdvancedAnalytics();
                    break;
                case "11":
                    ManageImportExport();
                    break;
                case "12":
                    ManageData();
                    break;
                case "0":
                    Console.WriteLine("До свидания!");
                    return;
                default:
                    Console.WriteLine("Неверный выбор");
                    break;
            }

            Console.WriteLine("\n\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    static void ConfigureContainer()
    {
        // Регистрируем через интерфейс
        _container.Register<IFinancialService>(() =>
        {
            var realService = new FinancialService();
            var cachedService = new CachedFinancialService(realService);
            return cachedService;
        });

        _container.Register(() => new FinancialFactory());
    }

    static void ShowMenu()
    {
        Console.WriteLine("\nГлавное меню:");
        Console.WriteLine("1. Создать счет");
        Console.WriteLine("2. Создать категорию");
        Console.WriteLine("3. Создать операцию");
        Console.WriteLine("4. Показать счета");
        Console.WriteLine("5. Показать аналитику");
        Console.WriteLine("6. Отменить последнюю команду");
        Console.WriteLine("7. Управление счетами");
        Console.WriteLine("8. Управление категориями");
        Console.WriteLine("9. Управление операциями");
        Console.WriteLine("10. Расширенная аналитика");
        Console.WriteLine("11. Импорт/Экспорт данных");
        Console.WriteLine("12. Управление данными");
        Console.WriteLine("0. Выход");
        Console.Write("Выберите действие: ");
    }

    static void ShowAdvancedAnalytics()
    {
        Console.WriteLine("\nРАСШИРЕННАЯ АНАЛИТИКА");
        Console.WriteLine("1. Полная аналитика");
        Console.WriteLine("2. Топ категорий расходов");
        Console.WriteLine("3. Месячная статистика");
        Console.WriteLine("4. Распределение по типам");
        Console.WriteLine("5. Аналитика за период");
        Console.Write("Выберите тип аналитики: ");

        var choice = Console.ReadLine();
        DateTime? startDate = null;
        DateTime? endDate = null;

        if (choice == "1" || choice == "2" || choice == "5")
        {
            while (true)
            {
                Console.Write("Начальная дата (дд.мм.гггг или Enter для всей истории): ");
                var startInput = Console.ReadLine();
                if (!string.IsNullOrEmpty(startInput))
                {
                    if (DateTime.TryParse(startInput, out var start))
                        startDate = start;
                    else
                    {
                        Console.WriteLine("Неверный формат даты. Попробуйте снова.");
                        continue;
                    }
                }

                Console.Write("Конечная дата (дд.мм.гггг или Enter для текущей даты): ");
                var endInput = Console.ReadLine();
                if (!string.IsNullOrEmpty(endInput))
                {
                    if (DateTime.TryParse(endInput, out var end))
                        endDate = end;
                    else
                    {
                        Console.WriteLine("Неверный формат даты. Попробуйте снова.");
                        continue;
                    }
                }
                else if (startDate.HasValue)
                {
                    endDate = DateTime.Now;
                }

                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    Console.WriteLine("Ошибка: дата начала не может быть позже даты окончания!");
                    Console.WriteLine("Попробуйте ввести даты заново.\n");
                    startDate = null;
                    endDate = null;
                    continue;
                }

                break;
            }
        }

        var command = choice switch
        {
            "1" => new ShowAnalyticsCommand(_financialService, AnalyticsType.Full, startDate, endDate),
            "2" => new ShowAnalyticsCommand(_financialService, AnalyticsType.TopExpenses, startDate, endDate),
            "3" => new ShowAnalyticsCommand(_financialService, AnalyticsType.Monthly),
            "4" => new ShowAnalyticsCommand(_financialService, AnalyticsType.Distribution),
            "5" => new ShowAnalyticsCommand(_financialService, AnalyticsType.Full, startDate, endDate),
            _ => null
        };

        if (command != null)
        {
            var timedCommand = new TimingCommandDecorator(command);
            timedCommand.Execute();
        }
        else
        {
            Console.WriteLine("Неверный выбор");
        }
    }

    static void InitializeDefaultCategories()
    {
        _financialService.CreateCategory(OperationType.Income, "Зарплата");
        _financialService.CreateCategory(OperationType.Income, "Кэшбэк");
        _financialService.CreateCategory(OperationType.Expense, "Еда");
        _financialService.CreateCategory(OperationType.Expense, "Транспорт");
    }

    static void CreateAccount()
    {
        while (true)
        {
            Console.Write("Введите название счета: ");
            var name = Console.ReadLine();

            Console.Write("Введите начальный баланс: ");
            var balanceInput = Console.ReadLine();

            if (!decimal.TryParse(balanceInput, out decimal balance))
            {
                Console.WriteLine("Неверная сумма. Попробуйте еще раз.");
                continue;
            }

            try
            {
                var command = new CreateAccountCommand(_financialService, name, balance);
                var timedCommand = new TimingCommandDecorator(command);
                timedCommand.Execute();
                _commandHistory.Add(timedCommand);
                break;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message} Попробуйте еще раз.");
            }
            catch (Exception)
            {
                Console.WriteLine("Неизвестная ошибка при создании счета. Попробуйте еще раз.");
            }
        }
    }

    static void CreateCategory()
    {
        OperationType type;
        string typeChoice;

        while (true)
        {
            Console.Write("Тип категории (1-Доход, 2-Расход): ");
            typeChoice = Console.ReadLine();

            if (typeChoice == "1")
            {
                type = OperationType.Income;
                break;
            }
            else if (typeChoice == "2")
            {
                type = OperationType.Expense;
                break;
            }
            else
            {
                Console.WriteLine("Неверный выбор! Введите 1 для Дохода или 2 для Расхода.");
                Console.WriteLine("Попробуйте еще раз...\n");
            }
        }

        string name;
        while (true)
        {
            Console.Write("Введите название категории: ");
            name = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(name))
            {
                break;
            }
            else
            {
                Console.WriteLine("Название категории не может быть пустым!");
                Console.WriteLine("Попробуйте еще раз...\n");
            }
        }

        var command = new CreateCategoryCommand(_financialService, type, name);
        var timedCommand = new TimingCommandDecorator(command);
        timedCommand.Execute();
        _commandHistory.Add(timedCommand);
    }

    static void CreateOperation()
    {
        var accounts = _financialService.GetAllAccounts();
        if (!accounts.Any())
        {
            Console.WriteLine("Нет доступных счетов. Сначала создайте счет.");
            return;
        }

        Console.WriteLine("Доступные счета:");
        for (int i = 0; i < accounts.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {accounts[i].Name} (Баланс: {accounts[i].Balance})");
        }

        Console.Write("Выберите счет: ");
        if (int.TryParse(Console.ReadLine(), out int accountIndex) && accountIndex > 0 && accountIndex <= accounts.Count)
        {
            var selectedAccount = accounts[accountIndex - 1];

            var categories = _financialService.GetAllCategories();
            if (!categories.Any())
            {
                Console.WriteLine("Нет доступных категорий. Сначала создайте категорию.");
                return;
            }

            Console.WriteLine("Доступные категории:");
            for (int i = 0; i < categories.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {categories[i].Name} ({categories[i].Type})");
            }

            Console.Write("Выберите категорию: ");
            if (int.TryParse(Console.ReadLine(), out int categoryIndex) && categoryIndex > 0 && categoryIndex <= categories.Count)
            {
                var selectedCategory = categories[categoryIndex - 1];
                OperationType type = selectedCategory.Type;

                Console.Write("Введите сумму: ");
                if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0)
                {
                    if (type == OperationType.Expense && selectedAccount.Balance < amount)
                    {
                        Console.WriteLine($"Недостаточно средств на счете '{selectedAccount.Name}'. Баланс: {selectedAccount.Balance} руб., требуется: {amount} руб.");
                        return;
                    }

                    Console.Write("Описание (необязательно): ");
                    var description = Console.ReadLine();

                    try
                    {
                        var command = new CreateOperationCommand(_financialService, type,
                            selectedAccount.Id, amount, selectedCategory.Id, description);
                        var timedCommand = new TimingCommandDecorator(command);
                        timedCommand.Execute();
                        _commandHistory.Add(timedCommand);
                        Console.WriteLine($"Операция создана: {selectedCategory.Name} на сумму {amount} руб.");
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Неизвестная ошибка: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Неверная сумма. Должна быть положительным числом.");
                }
            }
            else
            {
                Console.WriteLine("Неверный выбор категории");
            }
        }
        else
        {
            Console.WriteLine("Неверный выбор счета");
        }
    }

    static void ShowAccounts()
    {
        var accounts = _financialService.GetAllAccounts();
        Console.WriteLine("\nВаши счета:");

        if (!accounts.Any())
        {
            Console.WriteLine("Счетов нет");
            return;
        }

        foreach (var account in accounts)
        {
            Console.WriteLine($"- {account.Name}: {account.Balance} руб.");
        }

        Console.WriteLine($"\nОбщий баланс: {_financialService.GetTotalBalance()} руб.");
    }

    static void ShowAnalytics()
    {
        var command = new ShowAnalyticsCommand(_financialService, AnalyticsType.Full);
        var timedCommand = new TimingCommandDecorator(command);
        timedCommand.Execute();
    }

    static void UndoLastCommand()
    {
        if (_commandHistory.Any())
        {
            var lastCommand = _commandHistory.Last();
            lastCommand.Undo();
            _commandHistory.Remove(lastCommand);
        }
        else
        {
            Console.WriteLine("Нет команд для отмены");
        }
    }

    static void ManageAccounts()
    {
        var accounts = _financialService.GetAllAccounts();
        if (!accounts.Any())
        {
            Console.WriteLine("Нет доступных счетов.");
            return;
        }

        Console.WriteLine("\nВаши счета:");
        for (int i = 0; i < accounts.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {accounts[i].Name} (Баланс: {accounts[i].Balance} руб.)");
        }

        Console.WriteLine("\nДействия:");
        Console.WriteLine("1. Редактировать счет");
        Console.WriteLine("2. Удалить счет");
        Console.Write("Выберите действие: ");

        var action = Console.ReadLine();
        Console.Write("Выберите счет: ");

        if (int.TryParse(Console.ReadLine(), out int accountIndex) && accountIndex > 0 && accountIndex <= accounts.Count)
        {
            var selectedAccount = accounts[accountIndex - 1];

            switch (action)
            {
                case "1":
                    Console.Write("Введите новое название счета: ");
                    var newName = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        var command = new UpdateAccountCommand(_financialService, selectedAccount.Id, newName);
                        var timedCommand = new TimingCommandDecorator(command);
                        timedCommand.Execute();
                        _commandHistory.Add(timedCommand);
                        Console.WriteLine($"Счет обновлен: {selectedAccount.Name} → {newName}");
                    }
                    break;

                case "2":
                    var relatedOperations = _financialService.GetAllOperations()
                        .Where(o => o.BankAccountId == selectedAccount.Id)
                        .ToList();

                    if (relatedOperations.Any())
                    {
                        Console.WriteLine($"Внимание! У счета есть {relatedOperations.Count} связанных операций. Они также будут удалены.");
                        Console.Write("Продолжить? (y/n): ");
                        var confirm = Console.ReadLine();

                        if (confirm?.ToLower() != "y")
                            return;
                    }

                    var deleteCommand = new DeleteAccountCommand(_financialService, selectedAccount.Id);
                    var timedDeleteCommand = new TimingCommandDecorator(deleteCommand);
                    timedDeleteCommand.Execute();
                    _commandHistory.Add(timedDeleteCommand);
                    Console.WriteLine($"Счет '{selectedAccount.Name}' удален");
                    break;

                default:
                    Console.WriteLine("Неверное действие");
                    break;
            }
        }
    }

    static void ManageCategories()
    {
        var categories = _financialService.GetAllCategories();
        if (!categories.Any())
        {
            Console.WriteLine("Нет доступных категорий.");
            return;
        }

        Console.WriteLine("\nВаши категории:");
        for (int i = 0; i < categories.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {categories[i].Name} ({categories[i].Type})");
        }

        Console.WriteLine("\nДействия:");
        Console.WriteLine("1. Редактировать категорию");
        Console.WriteLine("2. Удалить категорию");
        Console.Write("Выберите действие: ");

        var action = Console.ReadLine();
        Console.Write("Выберите категорию: ");

        if (int.TryParse(Console.ReadLine(), out int categoryIndex) && categoryIndex > 0 && categoryIndex <= categories.Count)
        {
            var selectedCategory = categories[categoryIndex - 1];

            switch (action)
            {
                case "1":
                    Console.Write("Введите новое название категории: ");
                    var newName = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        var command = new UpdateCategoryCommand(_financialService, selectedCategory.Id, newName);
                        var timedCommand = new TimingCommandDecorator(command);
                        timedCommand.Execute();
                        _commandHistory.Add(timedCommand);
                        Console.WriteLine($"Категория обновлена: {selectedCategory.Name} → {newName}");
                    }
                    break;

                case "2":
                    var relatedOperations = _financialService.GetAllOperations()
                        .Where(o => o.CategoryId == selectedCategory.Id)
                        .ToList();

                    if (relatedOperations.Any())
                    {
                        Console.WriteLine($"Внимание! У категории есть {relatedOperations.Count} связанных операций. Они также будут удалены.");
                        Console.Write("Продолжить? (y/n): ");
                        var confirm = Console.ReadLine();

                        if (confirm?.ToLower() != "y")
                            return;
                    }

                    var deleteCommand = new DeleteCategoryCommand(_financialService, selectedCategory.Id);
                    var timedDeleteCommand = new TimingCommandDecorator(deleteCommand);
                    timedDeleteCommand.Execute();
                    _commandHistory.Add(timedDeleteCommand);
                    Console.WriteLine($"Категория '{selectedCategory.Name}' удалена");
                    break;

                default:
                    Console.WriteLine("Неверное действие");
                    break;
            }
        }
    }

    static void ManageOperations()
    {
        var operations = _financialService.GetAllOperations();
        if (!operations.Any())
        {
            Console.WriteLine("Нет доступных операций.");
            return;
        }

        Console.WriteLine("\nВаши операции:");
        for (int i = 0; i < operations.Count; i++)
        {
            var op = operations[i];
            var account = _financialService.GetAccount(op.BankAccountId);
            var category = _financialService.GetCategory(op.CategoryId);

            Console.WriteLine($"{i + 1}. {op.Date:dd.MM.yyyy} - {category?.Name} - {op.Amount} руб. ({op.Type}) - {account?.Name}");
        }

        Console.WriteLine("\nДействия:");
        Console.WriteLine("1. Удалить операцию");
        Console.Write("Выберите действие: ");

        var action = Console.ReadLine();
        Console.Write("Выберите операцию: ");

        if (int.TryParse(Console.ReadLine(), out int operationIndex) && operationIndex > 0 && operationIndex <= operations.Count)
        {
            var selectedOperation = operations[operationIndex - 1];

            switch (action)
            {
                case "1":
                    var account = _financialService.GetAccount(selectedOperation.BankAccountId);
                    var category = _financialService.GetCategory(selectedOperation.CategoryId);

                    Console.WriteLine($"Вы уверены, что хотите удалить операцию?");
                    Console.WriteLine($"{selectedOperation.Date:dd.MM.yyyy}");
                    Console.WriteLine($"Счет: {account?.Name}");
                    Console.WriteLine($"Категория: {category?.Name}");
                    Console.WriteLine($"Сумма: {selectedOperation.Amount} руб. ({selectedOperation.Type})");
                    Console.Write("Подтвердите удаление (y/n): ");

                    var confirm = Console.ReadLine();
                    if (confirm?.ToLower() == "y")
                    {
                        var command = new DeleteOperationCommand(_financialService, selectedOperation.Id);
                        var timedCommand = new TimingCommandDecorator(command);
                        timedCommand.Execute();
                        _commandHistory.Add(timedCommand);
                        Console.WriteLine("Операция удалена");
                    }
                    break;

                default:
                    Console.WriteLine("Неверное действие");
                    break;
            }
        }
    }

    static void ManageImportExport()
    {
        Console.WriteLine("\nИМПОРТ/ЭКСПОРТ ДАННЫХ");
        Console.WriteLine("1. Экспорт в JSON");
        Console.WriteLine("2. Экспорт в CSV");
        Console.WriteLine("3. Импорт из JSON");
        Console.WriteLine("4. Импорт из CSV");
        Console.Write("Выберите действие: ");

        var choice = Console.ReadLine();

        Console.Write("Введите путь к файлу: ");
        var filePath = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(filePath))
        {
            Console.WriteLine("Путь к файлу не может быть пустым");
            return;
        }

        try
        {
            switch (choice)
            {
                case "1":
                    if (!filePath.EndsWith(".json"))
                        filePath += ".json";

                    var jsonSerializer = new JsonDataSerializer();
                    var exportCommand = new ExportDataCommand(_financialService, jsonSerializer, filePath);
                    var timedExportCommand = new TimingCommandDecorator(exportCommand);
                    timedExportCommand.Execute();
                    _commandHistory.Add(timedExportCommand);
                    break;

                case "2":
                    if (!filePath.EndsWith(".csv"))
                        filePath += ".csv";

                    var csvSerializer = new CsvDataSerializer();
                    var exportCsvCommand = new ExportDataCommand(_financialService, csvSerializer, filePath);
                    var timedExportCsvCommand = new TimingCommandDecorator(exportCsvCommand);
                    timedExportCsvCommand.Execute();
                    _commandHistory.Add(timedExportCsvCommand);
                    break;

                case "3":
                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine("Файл не существует");
                        return;
                    }

                    Console.WriteLine("Внимание! Текущие данные будут заменены.");
                    Console.Write("Продолжить? (y/n): ");
                    var confirmJson = Console.ReadLine();

                    if (confirmJson?.ToLower() == "y")
                    {
                        var importSerializer = new JsonDataSerializer();
                        var importCommand = new ImportDataCommand(_financialService, importSerializer, filePath);
                        var timedImportCommand = new TimingCommandDecorator(importCommand);
                        timedImportCommand.Execute();
                        _commandHistory.Add(timedImportCommand);
                    }
                    break;

                case "4":
                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine("Файл не существует");
                        return;
                    }

                    Console.WriteLine("Внимание! Текущие данные будут заменены.");
                    Console.Write("Продолжить? (y/n): ");
                    var confirmCsv = Console.ReadLine();

                    if (confirmCsv?.ToLower() == "y")
                    {
                        var csvImportSerializer = new CsvDataSerializer();
                        var importCsvCommand = new ImportDataCommand(_financialService, csvImportSerializer, filePath);
                        var timedImportCsvCommand = new TimingCommandDecorator(importCsvCommand);
                        timedImportCsvCommand.Execute();
                        _commandHistory.Add(timedImportCsvCommand);
                    }
                    break;

                default:
                    Console.WriteLine("Неверный выбор");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static void ManageData()
    {
        Console.WriteLine("\nУПРАВЛЕНИЕ ДАННЫМИ");
        Console.WriteLine("1. Проверить целостность данных");
        Console.WriteLine("2. Пересчитать балансы");
        Console.Write("Выберите действие: ");

        var choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                var isIntegrityOk = _financialService.CheckDataIntegrity();
                if (isIntegrityOk)
                    Console.WriteLine("Целостность данных в порядке");
                else
                    Console.WriteLine("Обнаружены несоответствия в данных. Рекомендуется пересчитать балансы.");
                break;

            case "2":
                _financialService.RecalculateBalances();
                Console.WriteLine("Балансы пересчитаны");
                break;

            default:
                Console.WriteLine("Неверный выбор");
                break;
        }
    }
}