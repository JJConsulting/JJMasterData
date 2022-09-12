using System;
using System.Text;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Protheus;

/// <summary>
/// Classe Responsável pela comunicação com o Protheus
/// Permite conectar no server do Protheus e executar funções nativas do clipper ou user function (rpc)
/// </summary>
/// <remarks>Lucio Pelinson 16/05/2014</remarks>
public class ProtheusConnect : IDisposable
{
    #region "Delegates"
    private delegate int DelegateAPCreateConnControl(string cServer, int nPort, string cEnvironment, string cUser, string cPassWord);
    private delegate bool DelegateAPDestroyConnControl(int ObjectID);
    private delegate bool DelegateAPConnect(int ObjectID);
    private delegate void DelegateAPDisconnect(int ObjectID);
    private delegate bool DelegateAddNumericParam(int ObjectID, double value);
    private delegate bool DelegateAddLogicalParam(int ObjectID, bool value);
    private delegate bool DelegateAddDateParamAsDouble(int ObjectID, double value);
    private delegate bool DelegateAddStringParam(int ObjectID, string value);
    private delegate bool DelegateAddArrayParam(int ObjectID, object value);
    private delegate bool DelegateCallProc(int ObjectID, string cFunction);
    private delegate IntPtr DelegateResultAsArray(int ObjectID);
    private delegate void DelegateResultAsString(int ObjectID, StringBuilder sStr, int nTam);
    private delegate double DelegateResultAsNumeric(int ObjectID);
    private delegate bool DelegateResultAsLogical(int ObjectID);
    #endregion

    #region "Variaveis"
    private string DllPath = "apapi.dll"; 
    private IntPtr DllProtheus = IntPtr.Zero; 
    private DelegateAPCreateConnControl CallAPCreateConnControl;
    private DelegateAPDestroyConnControl CallAPDestroyConnControl;
    private DelegateAPConnect CallAPConnect;
    private DelegateAPDisconnect CallAPDisconnect;
    private DelegateAddNumericParam CallAddNumericParam;
    private DelegateAddLogicalParam CallAddLogicalParam;
    private DelegateAddDateParamAsDouble CallAddDateParamAsDouble;
    private DelegateAddStringParam CallAddStringParam;
    private DelegateAddArrayParam CallAddArrayParam;
    private DelegateCallProc CallCallProc;
    private DelegateResultAsArray CallResultAsArray;
    private DelegateResultAsString CallResultAsString;
    private DelegateResultAsNumeric CallResultAsNumeric;
    private DelegateResultAsLogical CallResultAsLogical;
    #endregion

    /// <summary>
    /// Permite conectar no protheus. Irá carregar os componentes apapi.dll e apconn.dll na raiz da aplicação.
    /// </summary>
    /// <remarks>Lucio Pelinson 16/05/2014</remarks>
    public ProtheusConnect()
    {
        DllPath = FileIO.ResolveFilePath("apapi.dll");
        DllProtheus = FunctionLoader.LoadWin32Library(DllPath);
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
    public ProtheusConnect(string dllpath)
    {
        DllPath = FileIO.ResolveFilePath(dllpath);
        DllProtheus = FunctionLoader.LoadWin32Library(DllPath);
    }


    ~ProtheusConnect()
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
    public int APCreateConnControl(string cServer, int nPort, string cEnvironment, string cUser, string cPassWord)
    {
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallAPCreateConnControl == null)
            CallAPCreateConnControl = (DelegateAPCreateConnControl)FunctionLoader.LoadFunction<DelegateAPCreateConnControl>(DllProtheus, "APCreateConnControl");

        return CallAPCreateConnControl(cServer, nPort, cEnvironment, cUser, cPassWord); 
    }


    /// <summary>
    /// Desconectar Conexão com o Protheus
    /// </summary>
    /// <param name="objectID">Ponteiro da conexão retornado pela função APCreateConnControl</param>
    /// <returns>Retorna verdadeiro caso a conexão foi fechada corretamente</returns>
    public bool APDestroyConnControl(int objectID)
    {
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallAPDestroyConnControl == null)
            CallAPDestroyConnControl = (DelegateAPDestroyConnControl)FunctionLoader.LoadFunction<DelegateAPDestroyConnControl>(DllProtheus, "APDestroyConnControl");

        return CallAPDestroyConnControl(objectID);
    }

    public bool APConnect(int objectID)
    {
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallAPConnect == null)
            CallAPConnect = (DelegateAPConnect)FunctionLoader.LoadFunction<DelegateAPConnect>(DllProtheus, "APConnect");

        return CallAPConnect(objectID); 
    }

    public void APDisconnect(int objectID)
    {
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallAPDisconnect == null)
            CallAPDisconnect = (DelegateAPDisconnect)FunctionLoader.LoadFunction<DelegateAPDisconnect>(DllProtheus, "APDisconnect");

        CallAPDisconnect(objectID); 
    }

    public bool AddNumericParam(int objectID, double value)
    {
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallAddNumericParam == null)
            CallAddNumericParam = (DelegateAddNumericParam)FunctionLoader.LoadFunction<DelegateAddNumericParam>(DllProtheus, "AddNumericParam");

        return CallAddNumericParam(objectID, value);
    }

    public bool AddLogicalParam(int objectID, bool value)
    {
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallAddLogicalParam == null)
            CallAddLogicalParam = (DelegateAddLogicalParam)FunctionLoader.LoadFunction<DelegateAddLogicalParam>(DllProtheus, "AddLogicalParam");

        return CallAddLogicalParam(objectID, value);
    }

    public bool AddDateParamAsDouble(int objectID, double value)
    {
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallAddDateParamAsDouble == null)
            CallAddDateParamAsDouble = (DelegateAddDateParamAsDouble)FunctionLoader.LoadFunction<DelegateAddDateParamAsDouble>(DllProtheus, "AddDateParamAsDouble");

        return CallAddDateParamAsDouble(objectID, value);
    }

    public bool AddStringParam(int objectID, string value)
    {
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallAddStringParam == null)
            CallAddStringParam = (DelegateAddStringParam)FunctionLoader.LoadFunction<DelegateAddStringParam>(DllProtheus, "AddStringParam");

        return CallAddStringParam(objectID, value);
    }

    public bool AddArrayParam(int objectID, object value)
    {
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallAddArrayParam == null)
            CallAddArrayParam = (DelegateAddArrayParam)FunctionLoader.LoadFunction<DelegateAddArrayParam>(DllProtheus, "AddArrayParam");

        return CallAddArrayParam(objectID, value); 
    }

    public bool CallProc(int objectID, string cFunction)
    {
        Log.AddError("inicio teste");  
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallCallProc == null)
            CallCallProc = (DelegateCallProc)FunctionLoader.LoadFunction<DelegateCallProc>(DllProtheus, "CallProc");

        bool lRet;
        try
        {
            lRet = CallCallProc(objectID, cFunction);
        }
        catch (AccessViolationException ex)
        {
            Log.AddError(ex.ToString());
            lRet = false;
        }
        catch
        {
            lRet = false;
        }
        return lRet;
    }

    public IntPtr ResultAsArray(int objectID)
    {
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallResultAsArray == null)
            CallResultAsArray = (DelegateResultAsArray)FunctionLoader.LoadFunction<DelegateResultAsArray>(DllProtheus, "ResultAsArray");

        return CallResultAsArray(objectID);
    }

    public void ResultAsString(int objectID, StringBuilder sStr, int nTam)
    {
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallResultAsString == null)
            CallResultAsString = (DelegateResultAsString)FunctionLoader.LoadFunction<DelegateResultAsString>(DllProtheus, "ResultAsString");

        CallResultAsString(objectID, sStr, nTam);
    }

    public string ResultAsString(int objectID)
    {
        StringBuilder sRet = new StringBuilder(500);
        ResultAsString(objectID, sRet, 500);
        return sRet.ToString();
    }

    public double ResultAsNumeric(int objectID)
    {
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallResultAsNumeric == null)
            CallResultAsNumeric = (DelegateResultAsNumeric)FunctionLoader.LoadFunction<DelegateResultAsNumeric>(DllProtheus, "ResultAsNumeric");

        return CallResultAsNumeric(objectID);
    }

    public bool ResultAsLogical(int objectID)
    {
        if (DllProtheus == IntPtr.Zero)
            DllProtheus = FunctionLoader.LoadWin32Library(DllPath);

        if (CallResultAsLogical == null)
            CallResultAsLogical = (DelegateResultAsLogical)FunctionLoader.LoadFunction<DelegateResultAsLogical>(DllProtheus, "ResultAsLogical");

        return CallResultAsLogical(objectID);
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    protected virtual void Dispose(bool disposing)
    {
        // free native resources if there are any.
        if (DllProtheus != IntPtr.Zero && !disposing)
        {
            FunctionLoader.FreeLibrary(DllProtheus);
            DllProtheus = IntPtr.Zero;
        }

        CallAPCreateConnControl = null;
        CallAPDestroyConnControl = null;
        CallAPConnect = null;
        CallAPDisconnect = null;
        CallAddNumericParam = null;
        CallAddLogicalParam = null;
        CallAddDateParamAsDouble = null;
        CallAddStringParam = null;
        CallAddArrayParam = null;
        CallCallProc = null;
        CallResultAsArray = null;
        CallResultAsString = null;
        CallResultAsNumeric = null;
        CallResultAsLogical = null;

    }

}