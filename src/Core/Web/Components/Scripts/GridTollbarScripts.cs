using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JJMasterData.Core.Web.Components.Scripts
{
    internal static class GridTollbarScripts
    {
        public static string GetRefreshScript(this JJGridView gridView)
        {
            string name = gridView.Name;
            if (gridView.IsExternalRoute)
            {
                var url = ScriptHelper.GetUrl(gridView.FormElement.Name);
                return $"JJGridView.Refresh('{name}', '{url}')";
            }

            string enableAjax = gridView.EnableAjax ? "true" : "false";
            return $"jjview.doRefresh('{name}', {enableAjax})";
        }

        public static string GetFilterScript(this JJGridView gridView)
        {
            string name = gridView.Name;
            if (gridView.IsExternalRoute)
            {
                var url = ScriptHelper.GetUrl(gridView.FormElement.Name);
                return $"JJGridView.Filter('{name}', '{url}')";
            }

            string enableAjax = gridView.EnableAjax ? "true" : "false";
            return $"jjview.doFilter('{name}','{enableAjax}')";
        }
    }
}
