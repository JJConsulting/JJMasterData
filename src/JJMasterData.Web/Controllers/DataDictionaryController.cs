

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Controllers;

[Authorize(Policy ="DataDictionary")]
public class DataDictionaryController : Controller
{
}
