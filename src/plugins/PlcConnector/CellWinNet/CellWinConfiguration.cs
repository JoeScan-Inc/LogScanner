using System.ComponentModel;
using System.Reflection;

namespace CellWinNet;

public static class CellWinConfiguration
{
    public static bool IsInitialized { get; private set; }
    public static void Initialize()
    {
        if (!IsInitialized)
        {
            try
            {
                var assembly = typeof(CellWinConfiguration).GetTypeInfo().Assembly;
                using Stream manifestResourceStream = assembly.GetManifestResourceStream("CellWinNet.native.cell-win.dll")!;
                byte[] ba = new byte[manifestResourceStream.Length];
                manifestResourceStream.Read(ba, 0, ba.Length);
                EmbeddedDllClass.ExtractEmbeddedDlls("cell-win.dll", ba);
                IsInitialized = true;
            }
            catch (Exception )
            {
                IsInitialized = false;
            }
        }
    }
}
