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
public class DictionariesController(DictionariesService dictionariesService,
        IDataDictionaryRepository dataDictionaryRepository)
    : ControllerBase
{
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
        var dicList = await dataDictionaryRepository.GetFormElementListAsync(true);
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
    public Task<FormElement> Get(string id)
    {
        return dataDictionaryRepository.GetFormElementAsync(id);
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
        return await dictionariesService.GetSyncInfoAsync(param, Debugger.IsAttached);
    }
}
