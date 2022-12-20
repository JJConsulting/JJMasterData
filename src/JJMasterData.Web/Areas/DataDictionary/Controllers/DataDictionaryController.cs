using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

[Authorize(Policy ="DataDictionary")]
public abstract class DataDictionaryController : Controller
{
}
