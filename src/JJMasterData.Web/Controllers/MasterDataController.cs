using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Controllers;

[Authorize(Policy = "MasterData")]
public class MasterDataController : Controller { }