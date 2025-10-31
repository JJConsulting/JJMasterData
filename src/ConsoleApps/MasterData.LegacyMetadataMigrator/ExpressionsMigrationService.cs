using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.LegacyMetadataMigrator;

public class ExpressionsMigrationService(
    IDataDictionaryRepository dataDictionaryRepository)
{
    private IDataDictionaryRepository DataDictionaryRepository { get; } = dataDictionaryRepository;

    public void Migrate()
    {
        var start = DateTime.Now;

        var formElements = DataDictionaryRepository.GetFormElementListAsync().GetAwaiter().GetResult();
        
        foreach (var formElement in formElements)
        {
            foreach (var field in formElement.Fields)
            {
                FixFieldExpressions(field);
            }

            foreach (var panel in formElement.Panels)
            {
                FixPanelExpressions(panel);
            }
            
            foreach (var action in formElement.Options.GridTableActions)
            {
                FixActionExpressions(action);
            }
            
            foreach (var action in formElement.Options.GridToolbarActions)
            {
                FixActionExpressions(action);
            }
            
            foreach (var action in formElement.Options.FormToolbarActions)
            {
                FixActionExpressions(action);
            }
            
            DataDictionaryRepository.InsertOrReplace(formElement);
        }
        
        Console.WriteLine("Process started: {0}", start);
        Console.WriteLine("Process finished: {0}", DateTime.Now);
    }

    private void FixFieldExpressions(FormElementField field)
    {
        var visibleExpressionBefore = field.VisibleExpression;

        field.VisibleExpression = FixQuotationMarks(visibleExpressionBefore);

        if (visibleExpressionBefore != field.VisibleExpression)
        {
            Console.WriteLine("{0} VisibleExpression Before: {1}", field.Name,
                visibleExpressionBefore);
            Console.WriteLine("{0} VisibleExpression After: {1}", field.Name,
                field.VisibleExpression);
        }

        var enableExpressionBefore = field.EnableExpression;

        field.EnableExpression = FixQuotationMarks(enableExpressionBefore);

        if (enableExpressionBefore != field.EnableExpression)
        {
            Console.WriteLine("{0} EnableExpression Before: {1}", field.Name,
                enableExpressionBefore);
            Console.WriteLine("{0} EnableExpression After: {1}", field.Name, field.EnableExpression);
        }

        foreach (var action in field.Actions)
        {
            FixActionExpressions(action);
        }
    }

    // ReSharper disable once MemberCanBeMadeStatic.Local
#pragma warning disable CA1822
    private void FixPanelExpressions(FormElementPanel panel)
#pragma warning restore CA1822
    {
        var visibleExpressionBefore = panel.VisibleExpression;

        panel.VisibleExpression = FixQuotationMarks(visibleExpressionBefore);

        if (visibleExpressionBefore != panel.VisibleExpression)
        {
            Console.WriteLine("Panel {0} VisibleExpression Before: {1}", panel.PanelId,
                visibleExpressionBefore);
            Console.WriteLine("Panel {0} VisibleExpression After: {1}", panel.PanelId,
                panel.VisibleExpression);
        }

        var enableExpressionBefore = panel.EnableExpression;

        panel.EnableExpression = FixQuotationMarks(enableExpressionBefore);

        if (enableExpressionBefore != panel.EnableExpression)
        {
            Console.WriteLine("Panel {0} EnableExpression Before: {1}", panel.PanelId,
                enableExpressionBefore);
            Console.WriteLine("Panel {0} EnableExpression After: {1}", panel.PanelId,
                panel.EnableExpression);
        }
    }

    // ReSharper disable once MemberCanBeMadeStatic.Local
#pragma warning disable CA1822
    private void FixActionExpressions(BasicAction action)
#pragma warning restore CA1822
    {
        var visibleExpressionBefore = action.VisibleExpression;

        action.VisibleExpression = FixQuotationMarks(visibleExpressionBefore);

        if (visibleExpressionBefore != action.VisibleExpression)
        {
            Console.WriteLine("Action {0} VisibleExpression Before: {1}", action.Name,
                visibleExpressionBefore);
            Console.WriteLine("Action {0} VisibleExpression After: {1}", action.Name,
                action.VisibleExpression);
        }

        var enableExpressionBefore = action.EnableExpression;

        action.EnableExpression = FixQuotationMarks(enableExpressionBefore);

        if (enableExpressionBefore != action.EnableExpression)
        {
            Console.WriteLine("Action {0} EnableExpression Before: {1}", action.Name,
                enableExpressionBefore);
            Console.WriteLine("Action {0} EnableExpression After: {1}", action.Name,
                action.EnableExpression);
        }
    }

    private static string FixQuotationMarks(string? expression)
    {
        if (string.IsNullOrEmpty(expression))
            return "val:1";
        
        var quotedValues = StringManager.FindValuesByInterval(expression, '\'','\'').ToList();
        var bracedValues = StringManager.FindValuesByInterval(expression, '{','}').ToList();

        var newExpression = expression;
        
        foreach (var bracedValue in bracedValues)
        {
            var hasQuotes = quotedValues.Any(quotedValue => quotedValue.Contains(bracedValue));
            
            if(hasQuotes)
                continue;

            newExpression = expression.Replace($"{{{bracedValue}}}", $"'{{{bracedValue}}}'");
        }

        return newExpression;
    }
}