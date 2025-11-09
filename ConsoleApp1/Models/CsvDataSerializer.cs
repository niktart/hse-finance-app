using System.Globalization;
using System.Text;

public class CsvDataSerializer : DataSerializer
{
    public override void ExportData(ExportData data, string filePath)
    {
        var csvContent = new StringBuilder();

        // Заголовок
        csvContent.AppendLine("ВШЭ-Банк - Экспорт данных");
        csvContent.AppendLine($"Дата экспорта: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        csvContent.AppendLine();

        // Счета
        csvContent.AppendLine("СЧЕТА");
        csvContent.AppendLine("ID,Название,Баланс");
        foreach (var account in data.Accounts)
        {
            csvContent.AppendLine($"{account.Id},{EscapeCsv(account.Name)},{account.Balance.ToString(CultureInfo.InvariantCulture)}");
        }
        csvContent.AppendLine();

        // Категории
        csvContent.AppendLine("КАТЕГОРИИ");
        csvContent.AppendLine("ID,Тип,Название");
        foreach (var category in data.Categories)
        {
            csvContent.AppendLine($"{category.Id},{category.Type},{EscapeCsv(category.Name)}");
        }
        csvContent.AppendLine();

        // Операции
        csvContent.AppendLine("ОПЕРАЦИИ");
        csvContent.AppendLine("ID,Тип,ID_Счета,Сумма,Дата,Описание,ID_Категории");
        foreach (var operation in data.Operations)
        {
            csvContent.AppendLine($"{operation.Id},{operation.Type},{operation.BankAccountId},{operation.Amount.ToString(CultureInfo.InvariantCulture)},{operation.Date:dd.MM.yyyy HH:mm},{EscapeCsv(operation.Description)},{operation.CategoryId}");
        }

        File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
    }

    public override ExportData ImportData(string filePath)
    {
        var exportData = new ExportData();
        var lines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();

        var currentSection = "";
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Определяем раздел
            if (line == "СЧЕТА") { currentSection = "ACCOUNTS"; continue; }
            if (line == "КАТЕГОРИИ") { currentSection = "CATEGORIES"; continue; }
            if (line == "ОПЕРАЦИИ") { currentSection = "OPERATIONS"; continue; }
            if (line.StartsWith("ВШЭ-Банк") || line.StartsWith("Дата экспорта")) continue;

            // Пропускаем заголовки
            if (line == "ID,Название,Баланс" || line == "ID,Тип,Название" ||
                line == "ID,Тип,ID_Счета,Сумма,Дата,Описание,ID_Категории") continue;

            try
            {
                switch (currentSection)
                {
                    case "ACCOUNTS":
                        var accountData = ParseCsvLine(line);
                        if (accountData.Length >= 3)
                        {
                            var account = new BankAccount
                            {
                                Id = Guid.Parse(accountData[0]),
                                Name = UnescapeCsv(accountData[1]),
                                Balance = ParseDecimal(accountData[2])
                            };
                            exportData.Accounts.Add(account);
                        }
                        break;

                    case "CATEGORIES":
                        var categoryData = ParseCsvLine(line);
                        if (categoryData.Length >= 3)
                        {
                            var category = new Category
                            {
                                Id = Guid.Parse(categoryData[0]),
                                Type = (OperationType)Enum.Parse(typeof(OperationType), categoryData[1]),
                                Name = UnescapeCsv(categoryData[2])
                            };
                            exportData.Categories.Add(category);
                        }
                        break;

                    case "OPERATIONS":
                        var operationData = ParseCsvLine(line);
                        if (operationData.Length >= 7)
                        {
                            var operation = new Operation
                            {
                                Id = Guid.Parse(operationData[0]),
                                Type = (OperationType)Enum.Parse(typeof(OperationType), operationData[1]),
                                BankAccountId = Guid.Parse(operationData[2]),
                                Amount = ParseDecimal(operationData[3]),
                                Date = DateTime.Parse(operationData[4]),
                                Description = UnescapeCsv(operationData[5]),
                                CategoryId = Guid.Parse(operationData[6])
                            };
                            exportData.Operations.Add(operation);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка парсинга строки CSV: {line}");
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        return exportData;
    }

    public override string FileExtension => ".csv";

    private decimal ParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        // Удаляем возможные пробелы
        value = value.Trim();

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            return result;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
            return result;

        string alternativeValue = value.Contains('.') ? value.Replace('.', ',') : value.Replace(',', '.');
        if (decimal.TryParse(alternativeValue, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            return result;

        throw new FormatException($"Не удалось преобразовать строку '{value}' в decimal");
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    private string UnescapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.StartsWith("\"") && value.EndsWith("\""))
        {
            value = value.Substring(1, value.Length - 2);
            value = value.Replace("\"\"", "\"");
        }
        return value;
    }

    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var currentField = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++; 
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(c);
            }
        }

        result.Add(currentField.ToString());
        return result.ToArray();
    }
}