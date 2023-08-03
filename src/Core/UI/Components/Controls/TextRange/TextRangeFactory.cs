using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Factories;

internal class TextRangeFactory : IControlFactory<JJTextRange>
{
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IHttpContext HttpContext { get; }
    private TextBoxFactory TextBoxFactory { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public TextRangeFactory(IHttpContext httpContext,
                            IExpressionsService expressionsService,
                            IStringLocalizer<JJMasterDataResources> stringLocalizer,
                            JJMasterDataEncryptionService encryptionService)
    {
        EncryptionService = encryptionService;
        HttpContext = httpContext;
        TextBoxFactory = new TextBoxFactory(httpContext, stringLocalizer, expressionsService, EncryptionService);
        StringLocalizer = stringLocalizer;
    }

    public JJTextRange Create()
    {
        return new JJTextRange(HttpContext, TextBoxFactory, StringLocalizer);
    }

    public JJTextRange Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var values = context.FormStateData.FormValues;
        string valueFrom = "";
        if (values != null && values.ContainsKey(field.Name + "_from"))
        {
            valueFrom = values[field.Name + "_from"].ToString();
        }

        var range = Create();
        range.FieldType = field.DataType;
        //todo gustavo: analisar
        range.FromField = TextBoxFactory.Create();
        range.FromField.Text = valueFrom;
        range.FromField.Name = field.Name + "_from";
        range.FromField.PlaceHolder = StringLocalizer["From"];

        string valueTo = "";
        if (values != null && values.ContainsKey(field.Name + "_to"))
        {
            valueTo = values[field.Name + "_to"].ToString();
        }
        //todo gustavo: analisar
        range.ToField = TextBoxFactory.Create();
        range.ToField.Text = valueTo;
        range.ToField.Name = field.Name + "_to";
        range.ToField.PlaceHolder = StringLocalizer["To"];

        return range;
    }
}