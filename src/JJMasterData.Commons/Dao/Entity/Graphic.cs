using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JJMasterData.Commons.Dao.Entity
{
    public class Graphic
    {
        public string DicName { get; set; }

        public string Name { get; set; }
        public TypeChart Type { get; set; }
        public List<DataSetAttr> DataSet { get; set; }

        public string QueryDataSet { get; set; }
        public Graphic()
        {
            DicName = "";
            Name = "";
            Type = TypeChart.bar;
            DataSet = new List<DataSetAttr>();
            QueryDataSet = "";

        }


        // preciso transofmar isso no novo jeito do Lucio/Barrinho
        public string CreateScriptAjax(string UrlDataSetGet)
        {
            StringBuilder html = new StringBuilder();
            html.Append(" $.ajax({ ");
            html.AppendLine(" type: \"POST\", ");
            html.AppendLine($" url: '{UrlDataSetGet}', ");
            html.AppendLine(" contentType: \"application/json; charset=utf-8\", ");
            html.AppendLine("  success: function (mems) { ");
            html.AppendLine("  var aData = mems; ");
            html.AppendLine(" var ctx = document.getElementById('myChart').getContext('2d'); ");
            html.AppendLine(" var myChart = new Chart(ctx, { ");
            html.AppendLine($" type: '{Type.ToString()}', ");
            html.AppendLine(" data: { ");
            html.AppendLine($" labels: mems[0][0], ");
            html.AppendLine("  datasets: [ ");

            for (int i = 0; i < DataSet.Count; i++)
            {

                html.AppendLine("{");
                html.AppendLine($" label: \"{DataSet[i].NameDataSet}\", ");
                html.AppendLine($"  data: mems[{i}][1], ");
                html.AppendLine($" backgroundColor: \"{DataSet[i].ColorBackground}\", ");// 
                html.AppendLine($" borderColor: \"{DataSet[i].ColorBorder}\", ");
                html.AppendLine(" borderWidth: 2 ");
                html.AppendLine(" }, ");

            }
            html.AppendLine(" ]}, ");
            html.AppendLine(" options: { ");
            html.AppendLine("  scales: { ");
            html.AppendLine("  y: { ");
            html.AppendLine("  beginAtZero: true, ");
            html.AppendLine(" } ");
            html.AppendLine(" } ");
            html.AppendLine(" } ");
            html.AppendLine(" }); ");
            html.AppendLine(" } ");
            html.AppendLine(" }); ");

            return html.ToString();
        }
    }



    public class DataSetAttr
    {
        public string NameDataSet { get; set; }
        public string ColorBackground { get; set; }
        public string ColorBorder { get; set; }
    }
}
