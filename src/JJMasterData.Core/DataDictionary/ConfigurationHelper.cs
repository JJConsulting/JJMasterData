#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif
using System.Threading;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DI;

namespace JJMasterData.Core.DataDictionary
{
    public class ConfigurationHelper
    {
        public static string GetMasterDataAuditLog()
        {
            string prefix= JJServiceCore.Options.AuditLogTableName;
            if (string.IsNullOrEmpty(prefix))
                prefix = "tb_masterdata_auditlog";
            return prefix;
        }

        public static string GetUrlMasterData()
        {
            string value = JJServiceCore.Options.JJMasterDataUrl;

#if NETFRAMEWORK
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                    value = "/" + HttpContext.Current.Request.ApplicationPath.Replace("/", "");
            }
#else
            var context = new HttpContextAccessor().HttpContext;
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
                if (context != null && context.Request != null)
                    value = "/" + context.Request.PathBase.ToString().Replace("/", "");
            }
#endif

            if (!value.EndsWith("/"))
                value += "/";

            value += Thread.CurrentThread.CurrentUICulture.Name;
            value += "/MasterData/";

            return value;
        }


         

    }
}
