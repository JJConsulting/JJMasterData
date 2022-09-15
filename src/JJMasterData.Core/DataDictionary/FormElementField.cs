using System;
using System.Collections;
using System.Runtime.Serialization;
using JJMasterData.Commons.Dao.Entity;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Informações específicas do campo no formulário, herda de ElementField
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
[DataContract]
public class FormElementField : ElementField
{
    public const string PlaceholderAttribute = "placeholder";
    public const string RowsAttribute = "rows";
    public const string PopUpSizeAttribute = "popupsize";
    public const string PopUpTitleAttribute = "popuptitle";

    private FormElementFieldActions _action;

    /// <summary>
    /// Tipo do componente
    /// </summary>
    [DataMember(Name = "component")] 
    public FormComponent Component { get; set; }

    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [DataMember(Name = "visibleExpression")]
    public string VisibleExpression { get; set; }

    /// <summary>
    /// Expression on runtime
    /// </summary>
    /// <remarks>
    /// [See expressions](../articles/expressions.md)
    /// </remarks>
    [DataMember(Name = "enableExpression")]
    public string EnableExpression { get; set; }

    /// <summary>
    /// Ordem do campo
    /// </summary>
    [DataMember(Name = "order")]
    public int Order { get; set; }
    

    /// <summary>
    /// Contador da linha, utilizado para quebrar a linha no form (classe row)
    /// </summary>
    /// <remarks>
    /// Utilizado para especificar manualmente o tamanho dos campos no formulário
    /// Exemplo:
    /// <code>
    ///     //Linha 1 com um campo
    ///     FormElementField f1 = FormElement.Fields["fieldname1"];
    ///     f1.LineGroup = 1;
    ///     f1.CssClass = "col-sm-12";
    ///     
    ///     //Linha 2 com dois campos
    ///     FormElementField f2 = FormElement.Fields["fieldname2"];
    ///     f2.LineGroup = 2;
    ///     f2.CssClass = "col-sm-6";
    ///     
    ///     FormElementField f3 = FormElement.Fields["fieldname3"];
    ///     f3.LineGroup = 2;
    ///     f3.CssClass = "col-sm-6";
    /// </code>
    /// </remarks>
    [DataMember(Name = "lineGroup")]
    public int LineGroup { get; set; }

    /// <summary>
    /// Nome da classe (CSS) a ser acrescentado na renderização do grupo do objeto
    /// </summary>
    [DataMember(Name = "cssClass")]
    public string CssClass { get; set; }

    /// <summary>
    /// Texto de ajuda, será exibido ao lado do label
    /// </summary>
    [DataMember(Name = "helpDescription")]
    public string HelpDescription { get; set; }

    /// <summary>
    /// Configurações especificas para lista
    /// </summary>
    [DataMember(Name = "dataItem")]
    public FormElementDataItem DataItem { get; set; }

    /// <summary>
    /// Configurações do arquivo
    /// </summary>
    [DataMember(Name = "dataFile")]
    public FormElementDataFile DataFile { get; set; }

    /// <summary>
    /// Coleção de atributos arbitrários (somente para renderização) que não correspondem às propriedades do controle
    /// </summary>
    [DataMember(Name = "attributes")]
    public Hashtable Attributes { get; set; }

    /// <summary>
    /// Permite exportar o campo (Default=true)
    /// </summary>
    [DataMember(Name = "export")]
    public bool Export { get; set; }

    /// <summary>
    /// Valida valores possívelmente perigosos no request (Default=true)
    /// </summary>
    /// <remarks>
    /// Importante para versões inferiores do .net habilitar o parametro: 
    /// httpRuntime requestValidationMode="4.5" ... 
    /// </remarks>
    [DataMember(Name = "validateRequest")]
    public bool ValidateRequest { get; set; }

    /// <summary>
    /// Ao alterar o conteúdo recarrega todos os campos do formulário
    /// (Default=false)
    /// </summary>
    /// <remarks>
    /// Normalmente utilizado para atualizar componente combobox ou searchbox que utilizam 
    /// um valor do formulário como referência na query.
    /// <para/>Exemplo:
    /// "SELECT ID, DESCR FROM TB_FOO WHERE TPVEND = {campo_tpvend}"
    /// </remarks>
    [DataMember(Name = "autoPostBack")]
    public bool AutoPostBack { get; set; }

    /// <summary>
    /// Refaz a expressão sempre que um campo disparar o AutoPostBack
    /// <para/> Expressão para um valor padrão
    /// <para/> Tipo [val:] retorna um valor;
    /// <para/> Tipo [exp:] retorna o resultado da expressão;
    /// <para/> Tipo [sql:] retorna o resultado de um comando sql;
    /// <para/> Tipo [protheus:] retorna o resultado de uma função do Protheus;
    /// </summary>
    /// <example>
    /// <para/> Exemplo utilizando [val:] + texto
    /// <para/> Exemplo1: val:a simple text;
    /// <para/> Exemplo2: val:10000;
    /// <code lang="c#">
    /// var field = new ElementField();
    /// field.DefaultValue = "val:test";
    /// </code>
    /// <para/> Exemplo utilizando [exp:] + expressão
    /// <para/> Exemplo1: exp:{field1};
    /// <para/> Exemplo2: exp:({field1} + 10) * {field2};
    /// <code lang="c#">
    /// var field = new ElementField();
    /// field.DefaultValue = "exp:{UserId}";
    /// </code>
    /// <para/> Exemplo utilizando [sql:] + query
    /// <para/> Exemplo1: sql:select 'foo';
    /// <para/> Exemplo2: sql:select count(*) from table1;
    /// <code lang="c#">
    /// var field = new ElementField();
    /// field.DefaultValue = "sql:select field2 from table1 where field1 = '{field1}'";
    /// </code>
    /// <para/> Exemplo utilizando [protheus:] + "UrlProtheus", "NomeFunção", "Parametros"
    /// <para/> Exemplo1: protheus:"http://localhost/jjmain.apw","u_test","";
    /// <para/> Exemplo2: protheus:"http://localhost/jjmain.apw","u_test","{field1};parm2";
    /// <code lang="c#">
    /// var field = new ElementField();
    /// field.DefaultValue = "protheus:'http://10.0.0.6:8181/websales/jjmain.apw', 'u_vldpan', '1;2'";
    /// </code>
    /// *Importante: Para chamadas do Protheus aplicar o patch JJxFun e configurar a conexão http no Protheus
    /// </example>
    /// <remarks>
    /// <para/> Como montar uma expressão para ocultar ou exibir um objeto:
    /// <para/>Campos do Formuário, UserValues ou Sessão = exp:{NOME_DO_CAMPO}
    /// <para/>*Importante: 
    /// O conteúdo entre {} (chaves) serão substituidos pelos valores atuais em tempo de execução.
    /// Seguindo a ordem:
    /// <para>1) UserValues (propriedade do objeto)</para>
    /// <para>2) Campos do Formulário (nome do campo)</para>
    /// <para>3) Palavras chaves (pagestate)</para>
    /// <para>4) Sessão do usuário</para>
    /// <para/>Exemplos de palavras chaves:
    /// <para/>{pagestate} = Estado da página: {pagestate} = "INSERT" | "UPDATE" | "VIEW" | "LIST" | "FILTER" | "IMPORT"
    /// <para/>{objname} = Nome do campo que disparou o evento de autopostback
    /// Se o valor da trigger retorna nulo o mesmo será desconsiderado.
    /// </remarks>
    [DataMember(Name = "triggerExpression")]
    public string TriggerExpression { get; set; }

    /// <summary>
    /// Numero de casas decimais
    /// Default(0)
    /// </summary>
    /// <remarks>
    /// Propriedade válida somente para tipos numéricos
    /// </remarks>
    [DataMember(Name = "numberOfDecimalPlaces")]
    public int NumberOfDecimalPlaces { get; set; }

    /// <summary>
    /// Id do painel agrupador de campos
    /// </summary>
    /// <remarks>
    /// Id referente a classe FormElementPanel
    /// </remarks>
    [DataMember(Name = "panelId")]
    public int PanelId { get; set; }

    /// <summary>
    /// Ações do campo
    /// </summary>
    [DataMember(Name = "actions")]
    public FormElementFieldActions Actions
    {
        get => _action ??= new FormElementFieldActions();
        set => _action = value;
    }

    /// <summary>
    /// Observação interna do desenvolvedor
    /// </summary>
    [DataMember(Name = "internalNotes")]
    public string InternalNotes { get; set; }
    
    /// <summary>
    /// Minimum value for number components
    /// </summary>
    [DataMember(Name = "minValue")]
    public float? MinValue { get; set; }
    
    /// <summary>
    /// Maximum value for number components
    /// </summary>
    [DataMember(Name = "maxValue")]
    public float? MaxValue { get; set; }


    public FormElementField()
    {
        Component = FormComponent.Text;
        DataItem = new FormElementDataItem();
        Export = true;
        ValidateRequest = true;
        VisibleExpression = "val:1";
        EnableExpression = "val:1";
        Attributes = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
    }

    public FormElementField(ElementField elementField)
    {
        Name = elementField.Name;
        Label = elementField.Label;
        DataType = elementField.DataType;
        Size = elementField.Size;
        DefaultValue = elementField.DefaultValue;
        IsRequired = elementField.IsRequired;
        IsPk = elementField.IsPk;
        AutoNum = elementField.AutoNum;
        Filter = elementField.Filter;
        DataBehavior = elementField.DataBehavior;

        switch (elementField.DataType)
        {
            case FieldType.Date:
            case FieldType.DateTime:
                Component = FormComponent.Date;
                break;
            case FieldType.Int:
                Component = FormComponent.Number;
                break;
            default:
            {
                Component = elementField.Size > 290 ? FormComponent.TextArea : FormComponent.Text;
                break;
            }
        }

        VisibleExpression = "val:1";
        EnableExpression = "val:1";
        if (elementField.IsPk)
        {
            if (elementField.AutoNum)
            {
                EnableExpression = "exp:{pagestate} = 'FILTER'";
                VisibleExpression = "exp:{pagestate} <> 'INSERT'";
            }
            else
            {
                EnableExpression = "exp:{pagestate} <> 'UPDATE'";
            }

        }
        DataItem = new FormElementDataItem();
        Export = true;
        ValidateRequest = true;
        Attributes = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
    }


    public string GetAttr(string key)
    {
        if (Attributes != null && Attributes.ContainsKey(key))
            return Attributes[key].ToString();
        return string.Empty;
    }

    public void SetAttr(string key, object value)
    {
        Attributes ??= new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        if (Attributes.ContainsKey(key))
            Attributes[key] = value;
        else
            Attributes.Add(key, value);

        if (value == null || string.IsNullOrEmpty(value.ToString()))
            Attributes.Remove(key);
    }

    public ElementField DeepCopyField()
    {
        var field = new ElementField
        {
            FieldId = FieldId,
            Name = Name,
            Label = Label,
            DataType = DataType,
            Filter = Filter,
            Size = Size,
            DefaultValue = DefaultValue,
            IsRequired = IsRequired,
            IsPk = IsPk,
            AutoNum = AutoNum,
            DataBehavior = DataBehavior
        };

        return field;
    }


}