using System;
using System.Collections.Generic;

// Классы для сериализации/десериализации
public class ExportData
{
    public List<BankAccount> Accounts { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public List<Operation> Operations { get; set; } = new();
    public DateTime ExportDate { get; set; } = DateTime.Now;
    public string Version { get; set; } = "1.0";
}

// Базовый класс для импорта/экспорта
public abstract class DataSerializer
{
    public abstract void ExportData(ExportData data, string filePath);
    public abstract ExportData ImportData(string filePath);
    public abstract string FileExtension { get; }
}