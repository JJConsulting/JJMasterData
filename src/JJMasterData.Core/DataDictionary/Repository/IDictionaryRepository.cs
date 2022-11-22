using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace JJMasterData.Core.DataDictionary.Repository;

public interface IDictionaryRepository
{
    /// <summary>
    /// Cria Estrutura do dicionário de dados
    /// </summary>
    void ExecInitialSetup();

    /// <summary>
    /// Retorna metadados armazenados no banco de dados
    /// </summary>
    /// <param name="elementName"></param>
    /// <returns>
    /// Retorna Objeto armazenado no banco. 
    /// Responsável por montar o Element, FormElement 
    /// e outras configurações de layout
    /// </returns>
    Dictionary GetDictionary(string elementName);

    /// <summary>
    /// Recupera uma lista de metadados armazenados no banco de dados
    /// </summary>
    /// <param name="sync">
    /// true=Somente itens que serão sincronizados. 
    /// false=Somente itens sem sincronismo
    /// null=Todos
    /// </param>
    /// <remarks>
    /// Metodo normalmente utilizado para sincronismo do dicionários entre sistemas.
    /// Permitindo remondar a herança original no sistema legado.
    /// </remarks>
    List<Dictionary> GetListDictionary(bool? sync);

    /// <summary>
    /// Recupera a lista com os nomes do dicionario
    /// </summary>
    string[] GetListDictionaryName();

    
    DataTable GetDataTable(Hashtable filters, string orderby, int regperpage, int pag, ref int tot);

    /// <summary>
    /// Verifica se o dicionário existe
    /// </summary>
    bool HasDictionary(string elementName);

    /// <summary>
    /// Persiste o dicionário no banco de dados
    /// </summary>
    void SetDictionary(Dictionary dictionary);

    /// <summary>
    /// Exclui o elemento no banco de dados
    /// </summary>
    /// <param name="id">Nome do dicionário</param>
    void DelDictionary(string id);
}