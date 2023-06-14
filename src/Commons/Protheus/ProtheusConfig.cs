namespace JJMasterData.Commons.Protheus;

/// <summary>
/// Configurações para realizar um RPC no Protheus
/// </summary>
/// <remarks>Lucio Pelinson 19/05/2014</remarks> 
public class ProtheusConfig
{
    /// <summary>
    /// Permite conectar no protheus informando dinamicamente o caminho da variavel apapi.dll.
    /// Obs.: A DLL apconn.dll deve estar no mesmo diretório da apapi.dll.
    /// Por padrão o sistema tentará localizar a dll na raiz da aplicação.
    /// </summary>
    public string DllPath = "apapi.dll";

    /// <summary>
    /// IP do Servidor Protheus
    /// </summary>
    public string Server = null;

    /// <summary>
    /// Número da Porta do serviço do Protheus
    /// </summary>
    public int Port;

    /// <summary>
    /// Nome do Ambiente
    /// </summary>
    public string Environment = null;

    /// <summary>
    /// Usuário do Protheus - (Deve ser administrador)
    /// </summary>
    public string User = null;

    /// <summary>
    /// Senha
    /// </summary>
    public string PassWord = null;

    /// <summary>
    /// Código da Empresa
    /// </summary>
    public string Empresa = null;

    /// <summary>
    /// Código da Filial
    /// </summary>
    public string Filial = null;
}