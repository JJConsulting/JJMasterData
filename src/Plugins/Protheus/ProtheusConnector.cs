
using System.Text;
using JJMasterData.Commons.Util;

namespace JJMasterData.Protheus;

/// <summary>
/// Classe Responsável pela comunicação com o Protheus
/// Permite conectar no server do Protheus e executar funções nativas do clipper ou user function (rpc)
/// </summary>
/// <remarks>Lucio Pelinson 16/05/2014</remarks>
public class ProtheusConnector : IDisposable
{
    #region "Delegates"
    private delegate int DelegateApCreateConnControl(string cServer, int nPort, string cEnvironment, string cUser, string cPassWord);
    private delegate bool DelegateApDestroyConnControl(int objectId);
    private delegate bool DelegateApConnect(int objectId);
    private delegate void DelegateApDisconnect(int objectId);
    private delegate bool DelegateAddNumericParam(int objectId, double value);
    private delegate bool DelegateAddLogicalParam(int objectId, bool value);
    private delegate bool DelegateAddDateParamAsDouble(int objectId, double value);
    private delegate bool DelegateAddStringParam(int objectId, string value);
    private delegate bool DelegateAddArrayParam(int objectId, object value);
    private delegate bool DelegateCallProc(int objectId, string cFunction);
    private delegate IntPtr DelegateResultAsArray(int objectId);
    private delegate void DelegateResultAsString(int objectId, StringBuilder sStr, int nTam);
    private delegate double DelegateResultAsNumeric(int objectId);
    private delegate bool DelegateResultAsLogical(int objectId);
    #endregion

    #region "Variaveis"
    private readonly string _dllPath = "apapi.dll"; 
    private IntPtr _dllProtheus; 
    private DelegateApCreateConnControl? _callApCreateConnControl;
    private DelegateApDestroyConnControl? _callApDestroyConnControl;
    private DelegateApConnect? _callApConnect;
    private DelegateApDisconnect? _callApDisconnect;
    private DelegateAddNumericParam? _callAddNumericParam;
    private DelegateAddLogicalParam? _callAddLogicalParam;
    private DelegateAddDateParamAsDouble? _callAddDateParamAsDouble;
    private DelegateAddStringParam? _callAddStringParam;
    private DelegateAddArrayParam? _callAddArrayParam;
    private DelegateCallProc? _callCallProc;
    private DelegateResultAsArray? _callResultAsArray;
    private DelegateResultAsString? _callResultAsString;
    private DelegateResultAsNumeric? _callResultAsNumeric;
    private DelegateResultAsLogical? _callResultAsLogical;
    #endregion

    /// <summary>
    /// Permite conectar no protheus. Irá carregar os componentes apapi.dll e apconn.dll na raiz da aplicação.
    /// </summary>
    /// <remarks>Lucio Pelinson 16/05/2014</remarks>
    public ProtheusConnector()
    {
        _dllPath = FileIO.ResolveFilePath("apapi.dll");
        _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);
    }


    /// <summary>
    /// Permite conectar no protheus informando dinamicamente o caminho da variavel apapi.dll.
    /// Obs.: A DLL apconn.dll deve estar no mesmo diretório da apapi.dll
    /// </summary>
    /// <param name="dllpath">
    /// Caminho completo para apapi.dll com o nome da dll
    /// <para>"\Bin\apapi.dll"</para>
    /// <para>"C:\Protheus\Bin\apapi.dll"</para>
    /// </param>
    /// <remarks>Lucio Pelinson 16/05/2014</remarks>
    public ProtheusConnector(string dllpath)
    {
        _dllPath = FileIO.ResolveFilePath(dllpath);
        _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);
    }


    ~ProtheusConnector()
    {
        //Liberar Dll caso o metodo dispose não tenha sido acionado 
        Dispose(false);
    }


        
    /// <summary>
    /// Cria uma conexão com o Protheus
    /// </summary>
    /// <param name="cServer">IP do Servidor Protheus</param>
    /// <param name="nPort">Número da Porta do serviço do Protheus</param>
    /// <param name="cEnvironment">Nome do Ambiente</param>
    /// <param name="cUser">Usuário do Protheus - (Deve ser administrador)</param>
    /// <param name="cPassWord">Senha</param>
    /// <returns>Ponteiro da conexão</returns>
    public int ApCreateConnControl(string cServer, int nPort, string cEnvironment, string cUser, string cPassWord)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        if (_callApCreateConnControl == null)
            _callApCreateConnControl = (DelegateApCreateConnControl)FunctionLoader.LoadFunction<DelegateApCreateConnControl>(_dllProtheus, "APCreateConnControl");

        return _callApCreateConnControl(cServer, nPort, cEnvironment, cUser, cPassWord); 
    }


    /// <summary>
    /// Desconectar Conexão com o Protheus
    /// </summary>
    /// <param name="objectId">Ponteiro da conexão retornado pela função APCreateConnControl</param>
    /// <returns>Retorna verdadeiro caso a conexão foi fechada corretamente</returns>
    public bool ApDestroyConnControl(int objectId)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        _callApDestroyConnControl ??=
            (DelegateApDestroyConnControl)FunctionLoader.LoadFunction<DelegateApDestroyConnControl>(_dllProtheus,
                "APDestroyConnControl");

        return _callApDestroyConnControl(objectId);
    }

    public bool ApConnect(int objectId)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        _callApConnect ??= (DelegateApConnect)FunctionLoader.LoadFunction<DelegateApConnect>(_dllProtheus, "APConnect");

        return _callApConnect(objectId); 
    }

    public void ApDisconnect(int objectId)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        _callApDisconnect ??=
            (DelegateApDisconnect)FunctionLoader.LoadFunction<DelegateApDisconnect>(_dllProtheus, "APDisconnect");

        _callApDisconnect(objectId); 
    }

    public bool AddNumericParam(int objectId, double value)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        if (_callAddNumericParam == null)
            _callAddNumericParam = (DelegateAddNumericParam)FunctionLoader.LoadFunction<DelegateAddNumericParam>(_dllProtheus, "AddNumericParam");

        return _callAddNumericParam(objectId, value);
    }

    public bool AddLogicalParam(int objectId, bool value)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        _callAddLogicalParam ??=
            (DelegateAddLogicalParam)FunctionLoader.LoadFunction<DelegateAddLogicalParam>(_dllProtheus, "AddLogicalParam");

        return _callAddLogicalParam(objectId, value);
    }

    public bool AddDateParamAsDouble(int objectId, double value)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        if (_callAddDateParamAsDouble == null)
            _callAddDateParamAsDouble = (DelegateAddDateParamAsDouble)FunctionLoader.LoadFunction<DelegateAddDateParamAsDouble>(_dllProtheus, "AddDateParamAsDouble");

        return _callAddDateParamAsDouble(objectId, value);
    }

    public bool AddStringParam(int objectId, string value)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        _callAddStringParam ??=
            (DelegateAddStringParam)FunctionLoader.LoadFunction<DelegateAddStringParam>(_dllProtheus, "AddStringParam");

        return _callAddStringParam(objectId, value);
    }

    public bool AddArrayParam(int objectId, object value)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        if (_callAddArrayParam == null)
            _callAddArrayParam = (DelegateAddArrayParam)FunctionLoader.LoadFunction<DelegateAddArrayParam>(_dllProtheus, "AddArrayParam");

        return _callAddArrayParam(objectId, value); 
    }

    public bool CallProc(int objectId, string cFunction)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        _callCallProc ??= (DelegateCallProc)FunctionLoader.LoadFunction<DelegateCallProc>(_dllProtheus, "CallProc");

        bool lRet;
        try
        {
            lRet = _callCallProc(objectId, cFunction);
        }
        catch (AccessViolationException)
        {
            lRet = false;
        }
        catch
        {
            lRet = false;
        }
        return lRet;
    }

    public IntPtr ResultAsArray(int objectId)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        _callResultAsArray ??=
            (DelegateResultAsArray)FunctionLoader.LoadFunction<DelegateResultAsArray>(_dllProtheus, "ResultAsArray");

        return _callResultAsArray(objectId);
    }

    public void ResultAsString(int objectId, StringBuilder sStr, int nTam)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        _callResultAsString ??=
            (DelegateResultAsString)FunctionLoader.LoadFunction<DelegateResultAsString>(_dllProtheus, "ResultAsString");

        _callResultAsString(objectId, sStr, nTam);
    }

    public string ResultAsString(int objectId)
    {
        var sRet = new StringBuilder(500);
        ResultAsString(objectId, sRet, 500);
        return sRet.ToString();
    }

    public double ResultAsNumeric(int objectId)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        _callResultAsNumeric ??=
            (DelegateResultAsNumeric)FunctionLoader.LoadFunction<DelegateResultAsNumeric>(_dllProtheus, "ResultAsNumeric");

        return _callResultAsNumeric(objectId);
    }

    public bool ResultAsLogical(int objectId)
    {
        if (_dllProtheus == IntPtr.Zero)
            _dllProtheus = FunctionLoader.LoadWin32Library(_dllPath);

        _callResultAsLogical ??=
            (DelegateResultAsLogical)FunctionLoader.LoadFunction<DelegateResultAsLogical>(_dllProtheus, "ResultAsLogical");

        return _callResultAsLogical(objectId);
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    protected virtual void Dispose(bool disposing)
    {
        // free native resources if there are any.
        if (_dllProtheus != IntPtr.Zero && !disposing)
        {
            FunctionLoader.FreeLibrary(_dllProtheus);
            _dllProtheus = IntPtr.Zero;
        }

        _callApCreateConnControl = null;
        _callApDestroyConnControl = null;
        _callApConnect = null;
        _callApDisconnect = null;
        _callAddNumericParam = null;
        _callAddLogicalParam = null;
        _callAddDateParamAsDouble = null;
        _callAddStringParam = null;
        _callAddArrayParam = null;
        _callCallProc = null;
        _callResultAsArray = null;
        _callResultAsString = null;
        _callResultAsNumeric = null;
        _callResultAsLogical = null;

    }

}