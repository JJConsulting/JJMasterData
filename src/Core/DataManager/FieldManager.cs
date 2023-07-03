using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using System;
using System.Collections.Generic;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.DataManager.Services.Abstractions;

namespace JJMasterData.Core.DataManager;

public class FieldManager
{
    #region "Properties"

    private string Name { get; set; }
    
    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }

    /// <summary>
    /// Objeto responsável por parsear expressoões
    /// </summary>
    public IExpressionsService ExpressionManager { get; private set; } = JJService.Provider.GetScopedDependentService<IExpressionsService>();

    internal IFieldEvaluationService FieldEvaluationService { get; } =
        JJService.Provider.GetScopedDependentService<IFieldEvaluationService>();
    
    #endregion

    #region "Constructors"

    public FieldManager(FormElement formElement)
    {
        FormElement = formElement ?? throw new ArgumentNullException(nameof(formElement));
        Name = "jjpanel_" + formElement.Name.ToLower();
    }
    
    public FieldManager(string name, FormElement formElement) : this(formElement)
    {
        Name = name;
    }

    #endregion

    public JJBaseControl GetField(FormElementField field, PageState pageState, IDictionary<string,dynamic> formValues,IDictionary<string,dynamic> userValues, object value = null)
    {
        if (pageState == PageState.Filter && field.Filter.Type == FilterMode.Range)
        {
            return JJTextRange.GetInstance(field, formValues);
        }
        
        var expOptions = new ExpressionOptions(userValues, formValues, pageState, JJService.EntityRepository);
        var controlFactory = new WebControlFactory(FormElement, expOptions, Name);
        var control = controlFactory.CreateControl(field, value);

        control.Enabled = FieldEvaluationService.IsEnabled(field, pageState, formValues);

        return control;
    }

    public bool IsRange(FormElementField field, PageState pageState)
    {
        return pageState == PageState.Filter & field.Filter.Type == FilterMode.Range;
    }
}
