using System.Diagnostics;
using JJMasterData.Api.Services;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Api.Controllers;

[Authorize]
[ApiController]
public class DictionariesController : ControllerBase
{

    private AccountService AccountService { get; set; }
    public DictionariesController(AccountService accountService)
    {
        AccountService = accountService;
    }
    /// <summary>
    /// Get all dictionaries with sync enabled.
    /// </summary>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Token Expired</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet]
    [Produces(typeof(Dictionary[]))]
    [Route("api/dictionaries/")]
    public ActionResult<Dictionary[]> GetAll()
    {
        var dicList = new DictionaryDao().GetListDictionary(true);
        if (dicList == null)
            return NotFound();

        return dicList.ToArray();
    }

    /// <summary>
    /// Get a specific data dictionary.
    /// </summary>
    /// <param name="id">Element Name</param>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response> 
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Token Expired</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet]
    [Route("api/dictionaries/{id}")]
    public Dictionary Get(string id)
    {
        return new DictionaryDao().GetDictionary(id);
    }

    /// <summary>
    /// Returns the number of records modified from a specific date.
    /// </summary>
    /// <param name="param">List of dictionaries to be checked</param>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad Request</response> 
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Token Expired</response>
    /// <response code="404">Not Found</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost]
    [Route("api/dictionaries/count")]
    public ActionResult<DicSyncInfo> Count([FromBody]DicSyncParam[] param)
    {
        var userid = AccountService.GetTokenInfo(HttpContext?.User?.Claims.First().Value)?.UserId;

        if (userid == null)
            return Unauthorized();

        return new DictionaryDao().GetSyncInfo(userid, param, Debugger.IsAttached);
    }
}
