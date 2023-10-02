using System.Collections.Generic;
using JJMasterData.Commons.Localization;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class ValidationSummaryFactory : IComponentFactory<JJValidationSummary>
{
    private readonly IStringLocalizer<JJMasterDataResources> _stringLocalizer;

    public ValidationSummaryFactory(IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        _stringLocalizer = stringLocalizer;
    }

    public JJValidationSummary Create()
    {
        return new JJValidationSummary
        {
            MessageTitle = _stringLocalizer["Invalid data"]
        };
    }
    
    public JJValidationSummary Create(IDictionary<string, string>errors)
    {
        var validation = Create();
        validation.SetErrors(errors);
        return validation;
    }
    
    public JJValidationSummary Create(IEnumerable<string> errors)
    {
        var validation = Create();
        validation.Errors.AddRange(errors);
        return validation;
    }
    
    public JJValidationSummary Create(string error)
    {
        var validation = Create();
        validation.Errors.Add(error);
        return validation;
    }
}