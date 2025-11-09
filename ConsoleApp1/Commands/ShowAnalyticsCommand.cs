using System;
using System.Collections.Generic;
using System.Linq;

public class ShowAnalyticsCommand : ICommand
{
    private readonly IFinancialService _financialService;
    private readonly DateTime? _startDate;
    private readonly DateTime? _endDate;
    private readonly AnalyticsType _analyticsType;

    public ShowAnalyticsCommand(IFinancialService financialService, AnalyticsType analyticsType,
                              DateTime? startDate = null, DateTime? endDate = null)
    {
        _financialService = financialService;
        _analyticsType = analyticsType;
        _startDate = startDate;
        _endDate = endDate;
    }

    public void Execute()
    {
        switch (_analyticsType)
        {
            case AnalyticsType.Full:
                ShowFullAnalytics();
                break;
            case AnalyticsType.TopExpenses:
                ShowTopExpenses();
                break;
            case AnalyticsType.Monthly:
                ShowMonthlyAnalytics();
                break;
            case AnalyticsType.Distribution:
                ShowDistribution();
                break;
        }
    }

    public void Undo()
    {
        Console.WriteLine("Отмена аналитики не поддерживается");
    }

    private void ShowFullAnalytics()
    {
        var analytics = _financialService.GetFullAnalytics(_startDate, _endDate);

        Console.WriteLine("\nПОЛНАЯ ФИНАНСОВАЯ АНАЛИТИКА");
        Console.WriteLine($"Период: {analytics.PeriodStart:dd.MM.yyyy} - {analytics.PeriodEnd:dd.MM.yyyy}");
        Console.WriteLine(new string('=', 50));

        Console.WriteLine($"Общие доходы: {analytics.TotalIncome:F2} руб.");
        Console.WriteLine($"Общие расходы: {analytics.TotalExpense:F2} руб.");
        Console.WriteLine($"Чистый доход: {analytics.NetIncome:F2} руб.");
        Console.WriteLine($"Норма сбережений: {analytics.SavingsRate:F1}%");
        Console.WriteLine();

        Console.WriteLine("ДОХОДЫ ПО КАТЕГОРИЯМ:");
        if (analytics.IncomeByCategory.Any())
        {
            foreach (var category in analytics.IncomeByCategory)
            {
                Console.WriteLine($"  - {category.CategoryName}: {category.TotalAmount:F2} руб. ({category.Percentage:F1}%) - {category.OperationsCount} операций");
            }
        }
        else
        {
            Console.WriteLine("  Нет данных о доходах");
        }
        Console.WriteLine();

        Console.WriteLine("РАСХОДЫ ПО КАТЕГОРИЯМ:");
        if (analytics.ExpenseByCategory.Any())
        {
            foreach (var category in analytics.ExpenseByCategory)
            {
                Console.WriteLine($"  - {category.CategoryName}: {category.TotalAmount:F2} руб. ({category.Percentage:F1}%) - {category.OperationsCount} операций");
            }
        }
        else
        {
            Console.WriteLine("  Нет данных о расходах");
        }
    }

    private void ShowTopExpenses()
    {
        var topExpenses = _financialService.GetTopExpenseCategories(5, _startDate, _endDate);

        Console.WriteLine("\nТОП-5 КАТЕГОРИЙ РАСХОДОВ");
        if (_startDate.HasValue && _endDate.HasValue)
            Console.WriteLine($"Период: {_startDate:dd.MM.yyyy} - {_endDate:dd.MM.yyyy}");

        Console.WriteLine(new string('=', 40));

        if (topExpenses.Any())
        {
            for (int i = 0; i < topExpenses.Count; i++)
            {
                var category = topExpenses[i];
                Console.WriteLine($"{i + 1}. {category.CategoryName}: {category.TotalAmount:F2} руб. ({category.OperationsCount} операций)");
            }
        }
        else
        {
            Console.WriteLine("Нет данных о расходах");
        }
    }

    private void ShowMonthlyAnalytics()
    {
        var monthly = _financialService.GetMonthlyAnalytics();

        Console.WriteLine("\nМЕСЯЧНАЯ СТАТИСТИКА");
        Console.WriteLine(new string('=', 60));

        foreach (var month in monthly.OrderBy(m => m.Key))
        {
            var analytics = month.Value;
            Console.WriteLine($"{month.Key:MMMM yyyy}:");
            Console.WriteLine($"  Доходы: {analytics.TotalIncome:F2} руб. | Расходы: {analytics.TotalExpense:F2} руб. | Чистый: {analytics.NetIncome:F2} руб.");
        }
    }

    private void ShowDistribution()
    {
        var distribution = _financialService.GetOperationTypeDistribution();

        Console.WriteLine("\nРАСПРЕДЕЛЕНИЕ ПО ТИПАМ ОПЕРАЦИЙ");
        Console.WriteLine(new string('=', 40));

        foreach (var item in distribution)
        {
            var typeName = item.Key == OperationType.Income ? "Доходы" : "Расходы";
            Console.WriteLine($"{typeName}: {item.Value:F1}%");
        }
    }
}