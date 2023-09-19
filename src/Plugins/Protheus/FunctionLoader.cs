using System;
using System.Runtime.InteropServices;

namespace JJMasterData.Protheus;

public class FunctionLoader
{
    public const uint DontResolveDllReferences = 0x00000001;
    public const uint LoadIgnoreCodeAuthzLevel = 0x00000010;
    public const uint LoadLibraryAsDatafile = 0x00000002;
    public const uint LoadLibraryAsDatafileExclusive = 0x00000040;
    public const uint LoadLibraryAsImageResource = 0x00000020;
    public const uint LoadLibrarySearchApplicationDir = 0x00000200;
    public const uint LoadLibrarySearchDefaultDirs = 0x00001000;
    public const uint LoadLibrarySearchDllLoadDir = 0x00000100;
    public const uint LoadLibrarySearchSystem32 = 0x00000800;
    public const uint LoadLibrarySearchUserDirs = 0x00000400;
    public const uint LoadWithAlteredSearchPath = 0x00000008;



    /// <summary>
    /// Captura um ponteiro para uma função de uma dll carregada
    /// </summary>
    /// <param name="hModule">Ponteiro da Dll retornado pela função LoadLibraryEx</param>
    /// <param name="procName">Nome da função que será chamada</param>
    /// <returns>Ponteiro para função</returns>
    [DllImport("Kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);


    /// <summary>
    /// Carregar um ponteiro para dll
    /// </summary>
    /// <param name="dllFilePath">nome do arquivo com o caminho</param>
    /// <param name="hFile">use IntPtr.Zero</param>
    /// <param name="dwFlags">What will happend during loading dll
    /// <para>LOAD_LIBRARY_AS_DATAFILE</para>
    /// <para>DONT_RESOLVE_DLL_REFERENCES</para>
    /// <para>LOAD_WITH_ALTERED_SEARCH_PATH</para>
    /// <para>LOAD_IGNORE_CODE_AUTHZ_LEVEL</para>
    /// </param>
    /// <returns>Ponteiro para carregar a dll</returns>
    [DllImport("Kernel32.dll")]
    private static extern IntPtr LoadLibraryEx(string dllFilePath, IntPtr hFile, uint dwFlags);


    /// <summary>
    /// Descarregar uma dll
    /// </summary>
    /// <param name="dllPointer">Ponteiro da Dll retornado pela função LoadLibraryEx</param>
    /// <returns>Se descarregar corretamente a dll corretamente retorna true caso contrario retorna false</returns>
    [DllImport("Kernel32.dll")]
    public extern static bool FreeLibrary(IntPtr dllPointer);


    /// <summary>
    /// Carrega a dll com base no caminho
    /// </summary>
    /// <param name="dllFilePath">nome do arquivo com o caminho</param>
    /// <returns>Ponteiro para dll</returns>
    /// <exception cref="ApplicationException">
    /// Quando a carga falhar dispara ApplicationException
    /// </exception>
    public static IntPtr LoadWin32Library(string dllFilePath)
    {
        IntPtr moduleHandle = LoadLibraryEx(dllFilePath, IntPtr.Zero, LoadWithAlteredSearchPath);
        if (moduleHandle == IntPtr.Zero)
        {
            int errorCode = Marshal.GetLastWin32Error();
            throw new ApplicationException(
            string.Format("There was an error during dll loading : {0}, error - {1}", dllFilePath, errorCode)
            );
        }
        return moduleHandle;
    }


    /// <summary>
    /// Carrega uma função a partir de uma dll
    /// </summary>
    public static Delegate LoadFunction<T>(string dllPath, string functionName)
    {
        IntPtr hModule = LoadWin32Library(dllPath);
        IntPtr functionAddress = GetProcAddress(hModule, functionName);
        return Marshal.GetDelegateForFunctionPointer(functionAddress, typeof(T));
    }

    public static Delegate LoadFunction<T>(IntPtr hModule, string functionName)
    {
        IntPtr functionAddress = GetProcAddress(hModule, functionName);
        return Marshal.GetDelegateForFunctionPointer(functionAddress, typeof(T));
    }




}
