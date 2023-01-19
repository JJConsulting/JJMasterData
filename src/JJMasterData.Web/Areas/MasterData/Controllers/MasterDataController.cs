using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Authorize(Policy = "MasterData")]
public abstract class MasterDataController : Controller { }