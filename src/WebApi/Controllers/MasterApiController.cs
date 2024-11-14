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
[ApiExplorerSettings(IgnoreApi = true)]
[Route("masterApi/{elementName}")]
public class MasterApiController(MasterApiService service) : ControllerBase
{
    [HttpGet]
    [Produces<MasterApiListResponse>]
    [Route("{pag:int?}/{regporpag:int?}/{orderby?}/{tot?}")]
    public async Task<ActionResult<MasterApiListResponse>> GetAll(string elementName, [FromQuery] int pag = 1,
        [FromQuery] int regporpag = 1000, [FromQuery] string? orderby = null, [FromQuery] int? tot = 0)
    {
        if (Request.Headers.Accept.ToString().Contains("text/csv"))
        {
            var text = await service.GetListFieldAsTextAsync(elementName, pag, regporpag, orderby);

            return Content(text, "text/csv");
        }

        var response = await service.GetListFieldsAsync(elementName, pag, regporpag, orderby, tot!.Value);
        return Ok(response);
    }
    
    [HttpGet]
    [Produces<Dictionary<string,object>>]
    [Route("{id}")]
    public async Task<ActionResult<Dictionary<string, object>>> Get(string elementName, string id)
    {
        return Ok(await service.GetFieldsAsync(elementName, id));
    }
    
    [HttpPost]
    public async Task<ActionResult<ResponseLetter>> Post([FromBody] Dictionary<string, object?>[] listParam, string elementName, bool replace = false)
    {
        return GetResponseMessage(await service.SetFieldsAsync(listParam, elementName, replace));
    }
    
    [HttpPut]
    public async Task<ActionResult<ResponseLetter>> Put([FromBody] Dictionary<string, object?>[] listParam, string elementName)
    {
        return GetResponseMessage(await service.UpdateFieldsAsync(listParam, elementName));
    }
    
    [HttpPatch]
    public async Task<ActionResult<ResponseLetter>> Patch([FromBody] Dictionary<string, object?>[] listParam, string elementName)
    {
        return GetResponseMessage(await service.UpdatePartAsync(listParam, elementName));
    }
    
    [HttpDelete]
    public async Task<ActionResult<ResponseLetter>> Delete(string elementName, string id)
    {
        return Ok(await service.DeleteAsync(elementName, id));
    }
    
    [HttpPost]
    [Produces<FormValues[]>]
    [Route("trigger/{pageState?}/{objname?}")]
    public async Task<ActionResult<ResponseLetter>> PostTrigger(string elementName, [FromBody] Dictionary<string, object>? paramValues,
        PageState pageState, string objname = "")
    {
        return Ok(await service.PostTriggerAsync(elementName, paramValues, pageState, objname));
    }

    private ActionResult<ResponseLetter> GetResponseMessage(IEnumerable<ResponseLetter> responseLetters)
    {
        var responseLetterList = responseLetters.ToList();

        if (responseLetterList.Count == 0)
            throw new ArgumentNullException(nameof(responseLetters), "Response not found");

        if (responseLetterList.Count == 1)
            return new ObjectResult(responseLetters) { StatusCode = responseLetterList[0].Status };

        var qtdTot = responseLetterList.Count;
        var qtdInsert = responseLetterList.Count(x => x.Status == (int)HttpStatusCode.Created);
        if (qtdTot == qtdInsert)
            return Created(nameof(GetResponseMessage), responseLetters);

        var qtdUpdate = responseLetterList.Count(x => x.Status == (int)HttpStatusCode.OK);
        if (qtdTot == qtdUpdate)
            return Ok(responseLetters);

        var qtdError = qtdTot - qtdInsert - qtdUpdate;
        if (qtdTot == qtdError)
            return BadRequest(responseLetters);

        return new ObjectResult(responseLetters) { StatusCode = (int)HttpStatusCode.MultiStatus };
    }
}