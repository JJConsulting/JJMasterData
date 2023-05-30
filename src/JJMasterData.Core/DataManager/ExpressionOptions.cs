using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary;
using System.Collections;
using JJMasterData.Commons.Data.Entity.Abstractions;

namespace JJMasterData.Core.DataManager
{
    public class ExpressionOptions
    {
        /// <summary>
        /// User specified values.
        /// Use to replace values that support expressions.
        /// </summary>
        /// <remarks>
        /// Key = Field name, Value= Field value
        /// </remarks>
        public IDictionary UserValues { get; set; }

        public IDictionary FormValues { get; set; }

        public PageState PageState { get; set; }

        public IEntityRepository EntityRepository { get; set; }

        public ExpressionOptions(IDictionary userValues, IDictionary formValues, PageState pageState, IEntityRepository entityRepository)
        {
            UserValues = userValues.DeepCopy();
            FormValues = formValues;
            PageState = pageState;
            EntityRepository = entityRepository;
        }
    }
}
