using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using System.Collections;
using System.Collections.Generic;
using JJMasterData.Commons.Data.Entity.Abstractions;

namespace JJMasterData.Core.DataManager;

public class ExpressionOptions
{
    /// <summary>
    /// User specified values.
    /// Use to replace values that support expressions.
    /// </summary>
    /// <remarks>
    /// Key = Field name, Value= Field value
    /// </remarks>
    public IDictionary<string,dynamic> UserValues { get; set; }

    public IDictionary<string,dynamic> FormValues { get; set; }

    public PageState PageState { get; set; }

    public IEntityRepository EntityRepository { get; set; }

    public ExpressionOptions(IDictionary<string,dynamic> userValues, IDictionary<string,dynamic>formValues, PageState pageState, IEntityRepository entityRepository)
    {
        UserValues = userValues.DeepCopy();
        FormValues = formValues;
        PageState = pageState;
        EntityRepository = entityRepository;
    }
}