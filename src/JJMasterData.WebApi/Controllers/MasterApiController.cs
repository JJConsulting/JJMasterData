using System.Collections;
using System.Net;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Core.DataDictionary;
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
    public ActionResult<MasterApiListResponse> GetAll(string elementName, [FromQuery] int pag = 1,
        [FromQuery] int regporpag = 1000, [FromQuery] string? orderby = null, [FromQuery] int? tot = 0)
    {
        if (Request.Headers.Accept.ToString().Contains("text/csv"))
        {
            string text = _service.GetListFieldAsText(elementName, pag, regporpag, orderby);

            return Content(text, "text/csv");
        }

        var response = _service.GetListFields(elementName, pag, regporpag, orderby, tot!.Value);
        return Ok(response);
    }
    
    [HttpGet]
    [Produces(typeof(Dictionary<string, object>))]
    [Route("{id}")]
    public ActionResult<Dictionary<string, object>> Get(string elementName, string id)
    {
        return Ok(_service.GetFields(elementName, id));
    }
    
    [HttpPost]
    public ActionResult<ResponseLetter> Post([FromBody] Hashtable[] listParam, string elementName, bool replace = false)
    {
        return GetResponseMessage(_service.SetFields(listParam, elementName, replace));
    }
    
    [HttpPut]
    public ActionResult<ResponseLetter> Put([FromBody] Hashtable[] listParam, string elementName)
    {
        return GetResponseMessage(_service.UpdateFields(listParam, elementName));
    }
    
    [HttpPatch]
    public ActionResult<ResponseLetter> Patch([FromBody] Hashtable[] listParam, string elementName)
    {
        return GetResponseMessage(_service.UpdatePart(listParam, elementName));
    }
    
    [HttpDelete]
    public ActionResult<ResponseLetter> Delete(string elementName, string id)
    {
        return Ok(_service.Delete(elementName, id));
    }
    
    [HttpPost]
    [Produces(typeof(FormValues[]))]
    [Route("trigger/{pageState?}/{objname?}")]
    public ActionResult<ResponseLetter> PostTrigger(string elementName, [FromBody] Hashtable? paramValues,
        PageState pageState, string objname = "")
    {
        return Ok(_service.PostTrigger(elementName, paramValues, pageState, objname));
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
            throw new ArgumentNullException(nameof(responseLetters), "Response not found");

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