using JJMasterData.Api.Models;
using JJMasterData.Api.Services;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JJMasterData.Api.Controllers;

[Authorize]
[ApiController]
public class DictionariesController : ControllerBase
{
    private readonly DictionariesService _dictionariesService;
    private readonly AccountService _accountService;
    private readonly IDictionaryRepository _dictionaryRepository;

    public DictionariesController(AccountService accountService, 
                                  DictionariesService dictionariesService, 
                                  IDictionaryRepository dictionaryRepository)
    {
        _accountService = accountService;
        _dictionariesService = dictionariesService;
        _dictionaryRepository = dictionaryRepository;
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
    [Produces(typeof(DataDictionary[]))]
    [Route("api/dictionaries/")]
    public ActionResult<DataDictionary[]> GetAll()
    {
        var dicList = _dictionaryRepository.GetListDictionary(true);
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
    public DataDictionary Get(string id)
    {
        return _dictionaryRepository.GetDictionary(id);
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
        var userid = _accountService.GetTokenInfo(HttpContext?.User?.Claims.First().Value)?.UserId;

        if (userid == null)
            return Unauthorized();

        return _dictionariesService.GetSyncInfo(userid, param, Debugger.IsAttached);
    }
}
