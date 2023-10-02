using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.WebApi.Models;
using JJMasterData.WebApi.Services;

namespace JJMasterData.WebApi.Controllers;

[Authorize]
[ApiController]
public class DictionariesController : ControllerBase
{
    private AccountService AccountService { get; }
    private readonly DictionariesService _dictionariesService;
    private readonly IDataDictionaryRepository _dataDictionaryRepository;

    public DictionariesController(
        AccountService accountService,
        DictionariesService dictionariesService, 
                                  IDataDictionaryRepository dataDictionaryRepository)
    {
        AccountService = accountService;
        _dictionariesService = dictionariesService;
        _dataDictionaryRepository = dataDictionaryRepository;
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
    [Produces(typeof(FormElement[]))]
    [Route("api/dictionaries/")]
    public async Task<ActionResult<FormElement[]>> GetAll()
    {
        var dicList = await _dataDictionaryRepository.GetFormElementListAsync(true);
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
    public async Task<FormElement> Get(string id)
    {
        return await _dataDictionaryRepository.GetFormElementAsync(id);
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
    public async Task<ActionResult<DicSyncInfo>> Count([FromBody]DicSyncParam[] param)
    {
        var userid = AccountService.GetTokenInfo(HttpContext.User.Claims.First().Value)?.UserId;

        if (userid == null)
            return Unauthorized();

        return await _dictionariesService.GetSyncInfoAsync(userid, param, Debugger.IsAttached);
    }
}
