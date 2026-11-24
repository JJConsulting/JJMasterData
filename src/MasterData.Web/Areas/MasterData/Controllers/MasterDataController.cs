using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.Areas.MasterData.Controllers;

[Area("MasterData")]
[AutoValidateAntiforgeryToken]
public abstract class MasterDataController : Controller;
