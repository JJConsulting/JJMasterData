using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.DictionaryDAL;

namespace JJMasterData.Core.DataManager;

public class FormManager
{
    private ExpressionManager _expression;
    private Hashtable _userValues;
    private Factory _factory;
    private IDataAccess _dataAccess;

    /// <summary>
    /// Valores espeçificos do usuário.
    /// Utilizado para substituir os valores em tempo de execução nos métodos que suportam expression.
    /// </summary>
    public Hashtable UserValues
    {
        get => _userValues ??= new Hashtable();
        set
        {
            _expression = null;
            _userValues = value;
        }
    }

    /// <summary>
    /// Objeto responsável por fazer toda a comunicação com o banco de dados
    /// </summary>
    public IDataAccess DataAccess
    {
        get => _dataAccess ??= JJService.DataAccess;
        set => _dataAccess = value;
    }

    /// <summary>
    /// Objeto responsável por parsear expressoões
    /// </summary>
    public ExpressionManager Expression => _expression ??= new(UserValues, DataAccess);

    /// <summary>
    /// Configurações pré-definidas do formulário
    /// </summary>
    public FormElement FormElement { get; set; }

    /// <summary>
    /// Objeto responsável por traduzir o elemento base em comandos para o banco de dados
    /// </summary>
    public Factory Factory
    {
        get => _factory ??= new Factory(DataAccess);
        set => _factory = value;
    }

    public FormManager(FormElement formElement)
    {
        FormElement = formElement;
    }

    public FormManager(FormElement formElement, Hashtable userValues, IDataAccess dataAccess) : this(formElement)
    {
        UserValues = userValues;
        DataAccess = dataAccess;
    }


    /// <summary>
    /// Valida os campos do formulário e 
    /// retorna uma lista com erros encontrados
    /// </summary>
    /// <param name="values">Dados do Formulário digitado pelo usuário</param>
    /// <param name="pageState">Estado atual da pagina</param>
    /// <param name="enableErrorLink">Inclui link em html nos campos de errro</param>
    /// <returns>
    /// Chave = Nome do Campo
    /// Valor = Mensagem de erro
    /// </returns>
    public Hashtable ValidateFields(Hashtable values, PageState pageState, bool enableErrorLink)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        var errors = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        foreach (var field in FormElement.Fields)
        {
            bool isVisible = Expression.GetBoolValue(field.VisibleExpression, field.Name, pageState, values);
            bool isEnable = Expression.GetBoolValue(field.EnableExpression, field.Name, pageState, values);
            
            if (!isVisible || !isEnable) continue;
            
            string value;
                
            if (values.Contains(field.Name) && values[field.Name] != null)
                value = values[field.Name].ToString();
            else
                value = "";

            var error = FieldValidator.ValidateField(field, field.Name, value, enableErrorLink);
            if (!string.IsNullOrEmpty(error))
                errors.Add(field.Name, error);
        }
        return errors;
    }

    /// <summary>
    /// Recupera os dados do Form, aplicando o valor padrão e as triggers
    /// </summary> 
    /// <param name="values">Dados do Formulário digitado pelo usuário</param>
    /// <param name="pageState">Estado atual da pagina</param>
    /// <param name="applyDefaultValues">Altera o valor padrão do campo se o mesmo for vazio</param>
    /// <returns>
    /// Retorna um novo hashtable com os valores atualizados
    /// </returns>
    public Hashtable GetTriggerValues(Hashtable values, PageState pageState, bool applyDefaultValues)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        Hashtable newvalues = new Hashtable();
        foreach (var f in FormElement.Fields)
        {
            if (values.Contains(f.Name))
            {
                object val = values[f.Name];
                if (val != null)
                {
                    if (f.Component == FormComponent.Cnpj ||
                        f.Component == FormComponent.Cnpj ||
                        f.Component == FormComponent.CnpjCpf)
                    {
                        val = StringManager.ClearCpfCnpjChars(val.ToString());
                    }
                    else if (f.Component == FormComponent.Tel)
                    {
                        val = StringManager.ClearTelChars(val.ToString());
                    }
                    else if (f.Component == FormComponent.Cep)
                    {
                        val = val.ToString().Replace("-", "");
                    }
                }

                if (newvalues.Contains(f.Name))
                    newvalues[f.Name] = val;
                else
                    newvalues.Add(f.Name, val);
            }
        }

        //Aplica o valor padrão do campos
        var defaultValues = GetDefaultValues(values, pageState);
        if (defaultValues != null)
        {
            foreach (DictionaryEntry d in defaultValues)
            {
                if (!newvalues.Contains(d.Key))
                {
                    newvalues.Add(d.Key, d.Value);
                }
                else
                {
                    if ((newvalues[d.Key] == null || string.IsNullOrEmpty(newvalues[d.Key].ToString()))
                        && applyDefaultValues)
                    {
                        newvalues[d.Key] = d.Value;
                    }
                }
            }
        }


        //Aplica expressão de gatilho nos campos
        var listFields = FormElement.Fields
            .ToList()
            .FindAll(x => !string.IsNullOrEmpty(x.TriggerExpression));
        foreach (var e in listFields)
        {
            string val = Expression.GetTriggerValue(e, pageState, newvalues);
            if (val != null)
            {
                if (newvalues.Contains(e.Name))
                    newvalues[e.Name] = val;
                else
                    newvalues.Add(e.Name, val);
            }
        }


        return newvalues;
    }

    public Hashtable GetDefaultValues(Hashtable formValues, PageState state)
    {
        Hashtable filters = new Hashtable();
        if (FormElement != null)
        {
            var list = FormElement.Fields
                .ToList()
                .FindAll(x => !string.IsNullOrEmpty(x.DefaultValue));
            foreach (var e in list)
            {
                string val = Expression.GetDefaultValue(e, state, formValues);
                if (!string.IsNullOrEmpty(val))
                {
                    filters.Add(e.Name, val);
                }
            }
        }

        return filters;
    }

    public Hashtable ApplyDefaultValues(Hashtable formValues, PageState state)
    {
        Hashtable values = new Hashtable();
        if (formValues != null)
        {
            foreach (DictionaryEntry v in formValues)
                values.Add(v.Key, v.Value);
        }

        if (FormElement != null)
        {
            var list = FormElement.Fields
                .ToList()
                .FindAll(x => !string.IsNullOrEmpty(x.DefaultValue));
            foreach (var e in list)
            {
                string val = Expression.GetDefaultValue(e, state, values);
                if (!string.IsNullOrEmpty(val))
                {
                    if (!values.Contains(e.Name))
                        values.Add(e.Name, val);
                }
            }
        }

        return values;
    }


    /// <summary>
    /// Retorna uma lista apenas com as chaves primarias da tabela,
    /// se não existir o valor da PK uma exceção será lançada
    /// </summary>
    /// <remarks>
    /// Se não existir o valor da PK uma DataDictionaryException será lançada
    /// </remarks>
    public Hashtable GetPkValues(Hashtable paramValues)
    {
        var primaryKeys = new Hashtable();
        var elementPks = FormElement.Fields.ToList().FindAll(x => x.IsPk);

        if (elementPks == null || elementPks.Count == 0)
            throw new DataDictionaryException(Translate.Key("Primary key not defined for dictionary {0}", FormElement.Name));

        foreach (ElementField field in elementPks)
        {
            if (!paramValues.ContainsKey(field.Name))
                throw new DataDictionaryException(Translate.Key("Primary key {0} not entered", field.Name));

            primaryKeys.Add(field.Name, paramValues[field.Name]);
        }

        return primaryKeys;
    }

    /// <summary>
    /// Preserva o nome original do campo conforme cadastrado no dicionário 
    /// e valida se o campo existe
    /// </summary>
    public Hashtable ParseOriginalName(Hashtable paramValues)
    {
        if (paramValues == null)
            return null;

        var filters = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        foreach (DictionaryEntry entry in paramValues)
        {
            var field = FormElement.Fields[entry.Key.ToString()];
            if (!filters.ContainsKey(entry.Key.ToString()))
                filters.Add(field.Name, entry.Value);
        }

        return filters;
    }

    /// <summary>
    /// Compara os valores dos campos recebidos com os enviados para banco,
    /// retornando os registros diferentes
    /// </summary>
    /// <remarks>
    /// Isso acontece devido as triggers ou os valores 
    /// retornados nos metodos de set (id autoNum) por exemplo
    /// </remarks>
    public Hashtable GetDiff(Hashtable original, Hashtable result, DicApiSettings api)
    {
        var newValues = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        foreach (DictionaryEntry entry in result)
        {
            if (entry.Value == null)
                continue;

            string fieldName = api.GetFieldNameParsed(entry.Key.ToString());
            if (original.ContainsKey(entry.Key))
            {
                if (original[entry.Key] == null && entry.Value != null ||
                    !original[entry.Key].Equals(entry.Value))
                    newValues.Add(fieldName, entry.Value);
            }
            else
            {
                newValues.Add(fieldName, entry.Value);
            }
        }

        return newValues.Count > 0 ? newValues : null;
    }


    public List<DataItemValue> GetDataItemValues(FormElementDataItem DataItem, Hashtable formValues, PageState pageState)
    {
        if (DataItem == null)
            return null;

        var values = new List<DataItemValue>();
        if (DataItem.Command != null && !string.IsNullOrEmpty(DataItem.Command.Sql))
        {

            string sql = DataItem.Command.Sql;
            if (sql.Contains("{"))
            {
                var exp = new ExpressionManager(UserValues, DataAccess);
                sql = exp.ParseExpression(sql, pageState, false, formValues);
            }


            DataTable dt = DataAccess.GetDataTable(sql);
            foreach (DataRow row in dt.Rows)
            {
                var item = new DataItemValue();
                item.Id = row[0].ToString();
                item.Description = row[1].ToString().Trim();
                if (DataItem.ShowImageLegend)
                {
                    item.Icon = (IconType)int.Parse(row[2].ToString());
                    item.ImageColor = row[3].ToString();
                }
                values.Add(item);
            }
        }
        else
        {
            values = DataItem.Items;
        }

        return values;
    }

}