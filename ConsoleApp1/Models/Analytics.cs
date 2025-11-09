using System;
using System.Collections.Generic;

public class CategoryAnalytics
{
    public string CategoryName { get; set; }
    public OperationType CategoryType { get; set; }
    public decimal TotalAmount { get; set; }
    public int OperationsCount { get; set; }
    public decimal Percentage { get; set; }
}

public class FinancialAnalytics
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetIncome { get; set; }
    public decimal SavingsRate { get; set; }
    public List<CategoryAnalytics> IncomeByCategory { get; set; }
    public List<CategoryAnalytics> ExpenseByCategory { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

// Типы аналитики
public enum AnalyticsType
{
    Full,
    TopExpenses,
    Monthly,
    Distribution
}