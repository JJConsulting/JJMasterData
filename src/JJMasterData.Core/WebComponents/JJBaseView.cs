using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http;
using System;
using System.Collections;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Base class of every component that renders to HTML.
/// Every public component inherits from this class.
/// </summary>
public abstract class JJBaseView
{
    #region "Properties"

    private IDataAccess _dataAccess;
    private Factory _factory;
    private Hashtable _userValues;
    private Hashtable _attributes;
    private string _userId;

    internal bool IsPostBack => CurrentContext.Request.HttpMethod.Equals("POST");

    /// <summary>
    /// Object responsible for all database communcations.
    /// </summary>
    public IDataAccess DataAccess
    {
        get => _dataAccess ??= JJService.DataAccess;
        set => _dataAccess = value;
    }

    /// <summary>
    /// Object responsible for translating the base element into commands for the database
    /// </summary>
    public Factory Factory
    {
        get => _factory ??= new Factory(DataAccess);
        set => _factory = value;
    }

    /// <summary>
    /// Values specified by the user.
    /// Used to replace values who support expression during runtime .
    /// </summary>
    public Hashtable UserValues
    {
        get => _userValues ??= new Hashtable();
        set => _userValues = value;
    }

    internal JJHttpContext CurrentContext => JJHttpContext.GetInstance();

    public bool Visible { get; set; } = true;

    /// <summary>
    /// Represents the component unique identifier
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// HTML attributes represented by key/value pairs
    /// </summary>
    public Hashtable Attributes
    {
        get => _attributes ??= new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        set => _attributes = value;
    }

    public string CssClass { get; set; }


    /// <summary>
    /// Id do usuário Atual
    /// </summary>
    /// <remarks>
    /// Se a variavel não for atribuida diretamente,
    /// o sistema tenta recuperar em UserValues ou nas variaveis de Sessão
    /// </remarks>
    internal string UserId
    {
        get
        {
            if (_userId == null)
                _userId = DataHelper.GetCurrentUserId(UserValues);

            return _userId;
        }
    }

    #endregion

    #region "Constructors"

    public JJBaseView()
    {
    }

    public JJBaseView(IDataAccess dataAccess)
    {
        DataAccess = dataAccess;
    }

    #endregion

    /// <summary>
    /// Returns the object representation of the HTML
    /// </summary>
    internal abstract HtmlBuilder RenderHtml();

    public HtmlBuilder GetHtmlBuilder()
    {
        return Visible ? RenderHtml() : null;
    }

    /// <summary>
    /// Renders the content in HTML.
    /// </summary>
    /// <returns>
    /// The HTML string.
    /// </returns>
    public string GetHtml()
    {
        return Visible ? RenderHtml()?.ToString(true) : string.Empty;
    }

    protected DicParser GetDictionary(string elementName) => new DictionaryDao(DataAccess).GetDictionary(elementName);

    public string GetAttr(string key)
    {
        return Attributes.ContainsKey(key) ? Attributes[key].ToString() : string.Empty;
    }

    public void SetAttr(string key, object value)
    {
        if (Attributes.ContainsKey(key))
            Attributes[key] = value;
        else
            Attributes.Add(key, value);

        if (value == null || string.IsNullOrEmpty(value.ToString()))
            Attributes.Remove(key);
    }

    public void SetAttr(Hashtable values)
    {
        if (values == null)
            return;

        foreach (DictionaryEntry v in values)
        {
            SetAttr(v.Key.ToString(), v.Value);
        }
    }


    /// <summary>
    /// Add or update a value in UserValues.<br></br>
    /// If exists, insert it, else, update it.
    /// </summary>
    /// <param name="field">Name of the field</param>
    /// <param name="value">Name of the field</param>
    public void SetUserValues(string field, object value)
    {
        if (UserValues.ContainsKey(field))
            UserValues[field] = value;
        else
            UserValues.Add(field, value);
    }
}
