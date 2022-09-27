using System;
using System.Collections;
using System.Collections.Generic;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Classe de apoio aos objetos que renderizam em html.
/// Todos os controles herdam dessa classe.
/// </summary>
public abstract class JJBaseView
{
    #region "Properties"

    private IDataAccess _dataAccess;
    private Factory _factory;
    private Hashtable _userValues;
    private Hashtable _attributes;
    private string _userId;

    /// <summary>
    /// Indica se a pagina esta sendo renderizada pela primeira vez
    /// </summary>
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
    /// Objeto responsável por traduzir o elemento base em comandos para o banco de dados
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

    /// <summary>
    /// Informações sobre o request HTTP
    /// </summary>
    internal JJHttpContext CurrentContext => JJHttpContext.GetInstance();

    /// <summary>
    /// Obtém ou define um valor que indica se o controle será ou não renderizado.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Id e nome do componente
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Coleção de atributos arbitrários (somente para renderização) que não correspondem às propriedades do controle
    /// </summary>
    public Hashtable Attributes
    {
        get => _attributes ??= new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        set => _attributes = value;
    }

    /// <summary>
    /// Classe CSS (Folha de Estilos em Cascata) aplicado no controle
    /// </summary>
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
            if (_userId != null) return _userId;
            
            const string userid = "USERID";
            
            if (UserValues.Contains(userid))
            {
                //Valor customizado pelo usuário
                _userId = UserValues[userid].ToString();
            }
            else if (CurrentContext.HasContext() &&
                     CurrentContext.Session != null &&
                     CurrentContext.Session[userid] != null)
            {
                //Valor da Sessão
                _userId = CurrentContext.Session[userid];
            }

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

    protected abstract string RenderHtml();

    /// <summary>
    /// Renders the content in HTML.
    /// </summary>
    /// <returns>
    /// The HTML string.
    /// </returns>
    public string GetHtml()
    {
        if (!Visible)
            return "";
        try
        {
            return RenderHtml();
        }
        catch (JJBaseException)
        {
            return null;
        }
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
