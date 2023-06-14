using System.Text;

namespace JJMasterData.ConsoleApp.Utils;

public class ConsoleHelper
{
    public static void WriteJJConsultingLogo()
    {
        var sb = new StringBuilder();
        sb.AppendLine("          ######    ######           ");
        sb.AppendLine("            ######    ######         ");
        sb.AppendLine("        #     ######    ######       ");
        sb.AppendLine("       ####     ######    ######     ");
        sb.AppendLine("     ########     ######    ######   ");
        sb.AppendLine("   ############     ######    ###### ");
        sb.AppendLine(" ################   ######    ###### ");
        sb.AppendLine("   ############   ######    ######   ");
        sb.AppendLine("     ########    ######    ######    ");
        sb.AppendLine("       ####    ######    ######      ");
        sb.AppendLine("        #    ######    ######        ");
        Console.WriteLine(sb.ToString());
    }
}