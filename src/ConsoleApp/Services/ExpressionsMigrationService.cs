using System.Text.RegularExpressions;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Util;
using JJMasterData.ConsoleApp.Repository;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.ConsoleApp.Services;

public class ExpressionsMigrationService
{
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private ILogger<ExpressionsMigrationService> Logger { get; }
    private MasterDataCoreOptions Options { get; }
    public ExpressionsMigrationService(
        IEntityRepository entityRepository, 
        IDataDictionaryRepository dataDictionaryRepository,
        IOptions<MasterDataCoreOptions> options,
        ILogger<ExpressionsMigrationService> logger
        )
    {
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        Logger = logger;
        Options = options.Value;
    }
    
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
            
            DataDictionaryRepository.InsertOrReplaceAsync(formElement).GetAwaiter().GetResult();
        }
        
        Logger.LogInformation("Process started: {Start}", start);
        Logger.LogInformation("Process finished: {Finish}", DateTime.Now);
    }

    private void FixFieldExpressions(FormElementField field)
    {
        var visibleExpressionBefore = field.VisibleExpression;

        field.VisibleExpression = FixQuotationMarks(visibleExpressionBefore);

        if (visibleExpressionBefore != field.VisibleExpression)
        {
            Logger.LogInformation("{Field} VisibleExpression Before: {ExpressionBefore}", field.Name,
                visibleExpressionBefore);
            Logger.LogInformation("{Field} VisibleExpression After: {ExpressionBefore}", field.Name,
                field.VisibleExpression);
        }

        var enableExpressionBefore = field.EnableExpression;

        field.EnableExpression = FixQuotationMarks(enableExpressionBefore);

        if (enableExpressionBefore != field.EnableExpression)
        {
            Logger.LogInformation("{Field} EnableExpression Before: {ExpressionBefore}", field.Name,
                enableExpressionBefore);
            Logger.LogInformation("{Field} EnableExpression After: {ExpressionBefore}", field.Name, field.EnableExpression);
        }

        foreach (var action in field.Actions)
        {
            FixActionExpressions(action);
        }
    }

    private void FixPanelExpressions(FormElementPanel panel)
    {
        var visibleExpressionBefore = panel.VisibleExpression;

        panel.VisibleExpression = FixQuotationMarks(visibleExpressionBefore);

        if (visibleExpressionBefore != panel.VisibleExpression)
        {
            Logger.LogInformation("Panel {PanelId} VisibleExpression Before: {ExpressionBefore}", panel.PanelId,
                visibleExpressionBefore);
            Logger.LogInformation("Panel {PanelId} VisibleExpression After: {ExpressionBefore}", panel.PanelId,
                panel.VisibleExpression);
        }

        var enableExpressionBefore = panel.EnableExpression;

        panel.EnableExpression = FixQuotationMarks(enableExpressionBefore);

        if (enableExpressionBefore != panel.EnableExpression)
        {
            Logger.LogInformation("Panel {PanelId} EnableExpression Before: {ExpressionBefore}", panel.PanelId,
                enableExpressionBefore);
            Logger.LogInformation("Panel {PanelId} EnableExpression After: {ExpressionBefore}", panel.PanelId,
                panel.EnableExpression);
        }
    }

    private void FixActionExpressions(BasicAction action)
    {
        var visibleExpressionBefore = action.VisibleExpression;

        action.VisibleExpression = FixQuotationMarks(visibleExpressionBefore);

        if (visibleExpressionBefore != action.VisibleExpression)
        {
            Logger.LogInformation("Action {ActionName} VisibleExpression Before: {ExpressionBefore}", action.Name,
                visibleExpressionBefore);
            Logger.LogInformation("Action {ActionName} VisibleExpression After: {ExpressionBefore}", action.Name,
                action.VisibleExpression);
        }

        var enableExpressionBefore = action.EnableExpression;

        action.EnableExpression = FixQuotationMarks(enableExpressionBefore);

        if (enableExpressionBefore != action.EnableExpression)
        {
            Logger.LogInformation("Action {ActionName} EnableExpression Before: {ExpressionBefore}", action.Name,
                enableExpressionBefore);
            Logger.LogInformation("Action {ActionName} EnableExpression After: {ExpressionBefore}", action.Name,
                action.EnableExpression);
        }
    }

    private static string FixQuotationMarks(string expression)
    {
        var quotedValues = StringManager.FindValuesByInterval(expression, '\'','\'');
        var bracedValues = StringManager.FindValuesByInterval(expression, '{','}');

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