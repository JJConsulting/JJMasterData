using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Components;

public abstract class ControlBase : AsyncComponent
{
    private string _text;

    /// <summary>
    /// Obtém ou define um valor que indica se o controle está habilitado.
    /// (Default = true)
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Obtém ou define um valor que indica se o controle é somente leitura
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Texto que especifica uma dica curta que descreve o valor esperado de um campo de entrada
    /// </summary>
    public string PlaceHolder { get; set; }

    /// <summary>
    /// Texto exibido quando o ponteiro do mouse passa sobre o controle
    /// </summary>
    public virtual string Tooltip { get; set; }
    
    public int MaxLength { get; set; }

    internal IFormValues FormValues { get; }
    
    /// <summary>
    /// Text content inside the input
    /// </summary>
    public string Text
    {
        get
        {
            if (_text == null && FormValues.ContainsFormValues())
            {
                _text = FormValues[Name];
            }
            return _text;
        }
        set => _text = value;
    }

    protected ControlBase(IFormValues formValues) 
    {
        FormValues = formValues;
    }

    public async Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var result = await GetResultAsync();

        if (result is RenderedComponentResult renderedResult)
        {
            return renderedResult.HtmlBuilder;
        }

        return new HtmlBuilder();
    }
}
