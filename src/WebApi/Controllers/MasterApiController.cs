using System.Collections;
using System.Net;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.WebApi.Models;
using JJMasterData.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("masterApi/{elementName}")]
public class MasterApiController : ControllerBase
{
    private readonly MasterApiService _service;
    public MasterApiController(MasterApiService service)
    {
        _service = service;
    }

    [HttpGet]
    [Produces(typeof(MasterApiListResponse))]
    [Route("{pag?}/{regporpag:int?}/{orderby?}/{tot?}")]
    public async Task<ActionResult<MasterApiListResponse>> GetAll(string elementName, [FromQuery] int pag = 1,
        [FromQuery] int regporpag = 1000, [FromQuery] string? orderby = null, [FromQuery] int? tot = 0)
    {
        if (Request.Headers.Accept.ToString().Contains("text/csv"))
        {
            string text = await _service.GetListFieldAsTextAsync(elementName, pag, regporpag, orderby);

            return Content(text, "text/csv");
        }

        var response = await _service.GetListFieldsAsync(elementName, pag, regporpag, orderby, tot!.Value);
        return Ok(response);
    }
    
    [HttpGet]
    [Produces(typeof(Dictionary<string, object>))]
    [Route("{id}")]
    public async Task<ActionResult<Dictionary<string, object>>> Get(string elementName, string id)
    {
        return Ok(await _service.GetFieldsAsync(elementName, id));
    }
    
    [HttpPost]
    public async Task<ActionResult<ResponseLetter>> Post([FromBody] Dictionary<string, object?>[] listParam, string elementName, bool replace = false)
    {
        return GetResponseMessage(await _service.SetFieldsAsync(listParam, elementName, replace).ToListAsync());
    }
    
    [HttpPut]
    public async Task<ActionResult<ResponseLetter>> Put([FromBody] Dictionary<string, object?>[] listParam, string elementName)
    {
        return GetResponseMessage(await _service.UpdateFieldsAsync(listParam, elementName).ToListAsync());
    }
    
    [HttpPatch]
    public async Task<ActionResult<ResponseLetter>> Patch([FromBody] Dictionary<string, object?>[] listParam, string elementName)
    {
        return GetResponseMessage(await _service.UpdatePartAsync(listParam, elementName).ToListAsync());
    }
    
    [HttpDelete]
    public async Task<ActionResult<ResponseLetter>> Delete(string elementName, string id)
    {
        return Ok(await _service.DeleteAsync(elementName, id));
    }
    
    [HttpPost]
    [Produces(typeof(FormValues[]))]
    [Route("trigger/{pageState?}/{objname?}")]
    public async Task<ActionResult<ResponseLetter>> PostTrigger(string elementName, [FromBody] IDictionary<string, object>? paramValues,
        PageState pageState, string objname = "")
    {
        return Ok(await _service.PostTriggerAsync(elementName, paramValues, pageState, objname));
    }
    
    [HttpPost]
    [Route("action/{actionName?}/{fieldName?}")]
    public HttpResponseMessage Action(string elementName, [FromBody] Hashtable paramValues, string actionName,
        string fieldName = "")
    {
        throw new NotImplementedException();
    }

    private ActionResult<ResponseLetter> GetResponseMessage(IEnumerable<ResponseLetter> responseLetters)
    {
        var responseLetterList = responseLetters.ToList();
        
        if (!responseLetterList.Any())
            throw new ArgumentNullException(nameof(responseLetters), @"Response not found");

        if (responseLetterList.Count == 1)
            return new ObjectResult(responseLetters) { StatusCode = responseLetterList.First().Status };

        int qtdTot = responseLetterList.Count;
        int qtdInsert = responseLetterList.Count(x => x.Status == (int)HttpStatusCode.Created);
        if (qtdTot == qtdInsert)
            return Created(nameof(GetResponseMessage), responseLetters);

        int qtdUpdate = responseLetterList.Count(x => x.Status == (int)HttpStatusCode.OK);
        if (qtdTot == qtdUpdate)
            return Ok(responseLetters);

        int qtdError = qtdTot - qtdInsert - qtdUpdate;
        if (qtdTot == qtdError)
            return BadRequest(responseLetters);

        return new ObjectResult(responseLetters) { StatusCode = (int)HttpStatusCode.MultiStatus };
    }
}