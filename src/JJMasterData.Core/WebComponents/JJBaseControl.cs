using JJMasterData.Commons.Dao;

namespace JJMasterData.Core.WebComponents;

public abstract class JJBaseControl : JJBaseView
{
    private string _text;

    public JJBaseControl()
    {
    }

    public JJBaseControl(IDataAccess dataAccess) : base(dataAccess)
    {
        
    }

    /// <summary>
    /// Obtém ou define um valor que indica se o controle está habilitado.
    /// (Default = true)
    /// </summary>
    public bool Enable { get; set; }

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
    public string ToolTip { get; set; }

    /// <summary>
    /// Tamanho máximo de caracteres permitido
    /// </summary>
    public int MaxLength { get; set; }

    /// <summary>
    /// Conteudo da caixa de texto 
    /// </summary>
    public string Text
    {
        get
        {
            if (_text == null && IsPostBack)
            {
                _text = CurrentContext.Request[Name];
            }
            return _text;
        }
        set
        {
            _text = value;
        }
    }


}
