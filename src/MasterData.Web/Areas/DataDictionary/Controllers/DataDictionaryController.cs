using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

[Area("DataDictionary")]
[AutoValidateAntiforgeryToken]
public abstract class DataDictionaryController : Controller;
