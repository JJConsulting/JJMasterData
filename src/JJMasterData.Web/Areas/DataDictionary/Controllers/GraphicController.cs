using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Core.DataDictionary.DictionaryDAL;

using JJMasterData.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers
{
    [Area("DataDictionary")]
    public class GraphicController : DataDictionaryController
    {
        [HttpGet]
        public IActionResult Index(string dicName = "")
        {
            var DicDao = new DictionaryDao();
            var formElement = DicDao.GetFormElement(dicName);

            var model = formElement.Graphic;
            return View(model);
        }

        [HttpPost]
        public IActionResult Index(Graphic model)
        {

            var DicDao = new DictionaryDao();
            var formElement = DicDao.GetFormElement(model.DicName);
            formElement.Graphic = model;

            DicDao.SetFormElement(formElement);
            return View(model);
        }



        public IActionResult AddDataSet(string acao, string dicName)
        {
            DataSetActions(acao, dicName, "");

            return RedirectToAction("Index", new { dicName });
        }

        [HttpPost]
        public IActionResult RemoveDataSet(string acao, string dicName, string nameDataSet)
        {
            DataSetActions(acao, dicName, nameDataSet);

            return RedirectToAction("Index", new { dicName });
        }



        // criar um service p/ isso
        private void DataSetActions(string acao, string dicName, string nameDataSet)
        {
            var DicDao = new DictionaryDao();
            var formElement = DicDao.GetFormElement(dicName);

            switch (acao)
            {
                case "Create":
                    formElement.Graphic.DataSet.Add(new DataSetAttr());
                    break;
                case "Delete":
                    formElement.Graphic.DataSet.RemoveAll(r => r.NameDataSet.Equals(nameDataSet));
                    break;
                case "DeleteAll":
                    formElement.Graphic.DataSet = new List<DataSetAttr>();
                    break;
                default:
                    break;
            }

            DicDao.SetFormElement(formElement);
        }
    }
}
