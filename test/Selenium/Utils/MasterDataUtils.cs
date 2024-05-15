using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests.Utils
{
    internal class MasterDataUtils
    {
        public static By AbrirPopUpExportar()
        {
            return By.CssSelector("a.btn.btn-secondary[onclick*='DataExportationHelper.openExportPopup']");

        }
        public static By ExecutarExportacao()
        {
            return By.CssSelector("a.btn.btn-secondary[onclick*='DataExportationHelper.startExportation']");
        }

        public static By GetLinkExportacao(string id)
        {
            return By.Id($"export_link_{id}");
        }

    }

}
