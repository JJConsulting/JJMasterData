using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using System;
using System.Collections;
using System.Linq;

namespace JJMasterData.Core.DataManager
{
    public static class DataHelper
    {
        /// <summary>
        /// Retorna uma lista apenas com as chaves primarias da tabela,
        /// se não existir o valor da PK uma exceção será lançada
        /// </summary>
        /// <remarks>
        /// Se não existir o valor da PK uma DataDictionaryException será lançada
        /// </remarks>
        public static Hashtable GetPkValues(FormElement formElement, Hashtable values)
        {
            var primaryKeys = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
            var elementPks = formElement.Fields.ToList().FindAll(x => x.IsPk);

            if (elementPks == null || elementPks.Count == 0)
                throw new DataDictionaryException(Translate.Key("Primary key not defined for dictionary {0}", formElement.Name));

            foreach (ElementField field in elementPks)
            {
                if (!values.ContainsKey(field.Name))
                    throw new DataDictionaryException(Translate.Key("Primary key {0} not entered", field.Name));

                primaryKeys.Add(field.Name, values[field.Name]);
            }

            return primaryKeys;
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
                var field = formElement.Fields[entry.Key.ToString()];
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
        public static Hashtable GetDiff(Hashtable original, Hashtable result, DicApiSettings api)
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

    }
}
