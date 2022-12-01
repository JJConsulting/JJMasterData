using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Http;
using System;
using System.Collections;
using System.Data;
using System.Linq;

namespace JJMasterData.Core.DataManager
{
    public static class DataHelper
    {
        public static string GetCurrentUserId(Hashtable userValues)
        {
            if (userValues != null && userValues.Contains("USERID"))
            {
                return userValues["USERID"].ToString();
            }

            var currentContext = JJHttpContext.GetInstance();
            if (currentContext.HasContext() &&
                currentContext.Session != null &&
                currentContext.Session["USERID"] != null)
            {
                return currentContext.Session["USERID"];
            }

            return null;
        }

        /// <summary>
        /// Retorna uma lista apenas com as chaves primarias da tabela,
        /// se não existir o valor da PK uma exceção será lançada
        /// </summary>
        /// <remarks>
        /// Se não existir o valor da PK uma DataDictionaryException será lançada
        /// </remarks>
        public static Hashtable GetPkValues(Element element, IDictionary values)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(FormElement));

            if (values == null)
                throw new ArgumentNullException(nameof(values));

            var primaryKeys = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            var elementPks = element.Fields.ToList().FindAll(x => x.IsPk);

            if (elementPks == null || elementPks.Count == 0)
                throw new DataDictionaryException(Translate.Key("Primary key not defined for dictionary {0}", element.Name));

            foreach (ElementField field in elementPks)
            {
                if (!values.Contains(field.Name))
                    throw new DataDictionaryException(Translate.Key("Primary key {0} not entered", field.Name));

                primaryKeys.Add(field.Name, values[field.Name]);
            }

            return primaryKeys;
        }

        public static Hashtable GetPkValues(Element element, string parsedValues, char separator)
        {
            var primaryKeys = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

            var values = parsedValues.Split(separator);
            if (values == null || values.Length == 0)
                throw new ArgumentException(Translate.Key("Invalid parameter or not found"), nameof(values));

            var elementPks = element.Fields.ToList().FindAll(x => x.IsPk);
            if (values.Length != elementPks.Count)
                throw new DataDictionaryException(Translate.Key("Invalid primary key"));

            for (int i = 0; i < values.Length; i++)
            {
                primaryKeys.Add(elementPks[i].Name, values[i]);
            }

            return primaryKeys;
        }

        /// <summary>
        /// Concat primary keys with separator caracteres
        /// </summary>
        public static string ParsePkValues(FormElement formElement, IDictionary formValues, char separator)
        {
            var pkFields = GetPkValues(formElement, formValues);
            string name = string.Empty;

            foreach (DictionaryEntry fieldKeyPar in pkFields)
            {
                if (name.Length > 0)
                    name += separator.ToString();

                string value = fieldKeyPar.Value.ToString();
                if (value.Contains(separator))
                    throw new Exception(Translate.Key("Primary key value {0} contains invalid characters.", value));

                name += value;
            }
            return name;
        }

        public static string ParsePkValues(FormElement formElement, DataRow row, char separator)
        {
            var formValues = row.Table.Columns
                .Cast<DataColumn>()
                .ToDictionary(col => col.ColumnName, col => row[col.ColumnName]);

            return ParsePkValues(formElement, formValues, separator);
        }

        /// <summary>
        /// Preserva o nome original do campo conforme cadastrado no dicionário 
        /// e valida se o campo existe
        /// </summary>
        public static Hashtable ParseOriginalName(FormElement formElement, Hashtable paramValues)
        {
            if (paramValues == null)
                return null;

            var filters = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            foreach (DictionaryEntry entry in paramValues)
            {
                var field = formElement.FormFields[entry.Key.ToString()];
                if (!filters.ContainsKey(entry.Key.ToString()))
                    filters.Add(field.Name, entry.Value);
            }

            return filters;
        }

        public static void CopyIntoHash(ref Hashtable newvalues, Hashtable valuesToBeCopied, bool replaceIfExistKey)
        {
            if (valuesToBeCopied == null || valuesToBeCopied.Count == 0)
                return;

            foreach (DictionaryEntry entry in valuesToBeCopied)
            {
                if (newvalues.ContainsKey(entry.Key))
                {
                    if (replaceIfExistKey)
                        newvalues[entry.Key] = entry.Value;
                }
                else
                {
                    newvalues.Add(entry.Key, entry.Value);
                }
            }
        }

    }
}
