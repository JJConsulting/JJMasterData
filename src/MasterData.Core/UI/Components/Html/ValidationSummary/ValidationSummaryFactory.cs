using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class ValidationSummaryFactory(IStringLocalizer<MasterDataResources> stringLocalizer) : IComponentFactory<JJValidationSummary>
{
    public JJValidationSummary Create()
    {
        return new JJValidationSummary
        {
            MessageTitle = stringLocalizer["Invalid data"]
        };
    }
    
    public JJValidationSummary Create(Dictionary<string, string>errors)
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