using System.Collections;
using System.Net;
using JJMasterData.Api.Models;
using JJMasterData.Api.Services;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Core.DataDictionary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Api.Controllers;

[Authorize]
[ApiController]
public class MasterApiController : ControllerBase
{
    private readonly MasterApiService _service;

    public MasterApiController(MasterApiService service)
    {
        _service = service;
    }

    [HttpGet]
    [Produces(typeof(MasterApiListResponse))]
    [Route("masterApi/{elementName}/{pag?}/{regporpag?}/{orderby?}/{tot?}")]
    public ActionResult<MasterApiListResponse> GetAll(string elementName, [FromQuery]int pag = 1,
        [FromQuery]int regporpag = 1000, [FromQuery]string? orderby = null, [FromQuery]int? tot = 0)
    {
        if (Request?.Headers?.Accept.ToString().Contains("text/csv") ?? false)
        {
            string text = _service.GetListFieldAsText(elementName, pag, regporpag, orderby);

            return Content(text, "text/csv");
        }

        var response = _service.GetListFields(elementName, pag, regporpag, orderby, tot.Value);
        return Ok(response);
    }


    [HttpGet]
    [Produces(typeof(Dictionary<string,object>))]
    [Route("masterApi/{elementName}/{id}")]
    public ActionResult<Dictionary<string,object>> Get(string elementName, string id)
    {
        return Ok(_service.GetFields(elementName, id));
    }


    [HttpPost]
    [Route("masterApi/{elementName}")]
    public ActionResult<ResponseLetter> Post([FromBody]Hashtable[] listParam, string elementName, bool replace = false)
    {
        return GetResponseMessage(_service.SetFields(listParam, elementName, replace));
    }


    [HttpPut]
    [Route("masterApi/{elementName}")]
    public ActionResult<ResponseLetter> Put([FromBody]Hashtable[] listParam, string elementName)
    {
        return GetResponseMessage(_service.UpdateFields(listParam, elementName));
    }


    [HttpPatch]
    [Route("masterApi/{elementName}")]
    public ActionResult<ResponseLetter> Patch([FromBody] Hashtable[] listParam, string elementName)
    {
        return GetResponseMessage(_service.UpdatePart(listParam, elementName));
    }


    [HttpDelete]
    [Route("masterApi/{elementName}/{id}")]
    public ActionResult<ResponseLetter> Delete(string elementName, string id)
    {
        return Ok(_service.Delete(elementName, id));
    }


    [HttpPost]
    [Produces(typeof(FormValues[]))]
    [Route("masterApi/{elementName}/trigger/{pageState?}/{objname?}")]
    public ActionResult<ResponseLetter> PostTrigger(string elementName, [FromBody]Hashtable? paramValues, PageState pageState, string objname = "")
    {
        return Ok(_service.PostTrigger(elementName, paramValues, pageState, objname));
    }


    [HttpPost]
    [Route("masterApi/{elementName}/action/{actionName?}/{fieldName?}")]
    public HttpResponseMessage Action(string elementName, [FromBody]Hashtable paramValues, string actionName, string fieldName = "")
    {
        throw new NotImplementedException();
    }

    private ActionResult<ResponseLetter> GetResponseMessage(List<ResponseLetter> listRet)
    {
        if (listRet == null || listRet.Count == 0)
            throw new ArgumentNullException(nameof(listRet), "Response not found");

        if (listRet.Count == 1)
            return new ObjectResult(listRet) { StatusCode = listRet.First().Status };

        int qtdTot = listRet.Count;
        int qtdInsert = listRet.ToList().Count(x => x.Status == (int)HttpStatusCode.Created);
        if (qtdTot == qtdInsert)
            return Created(nameof(GetResponseMessage), listRet);

        int qtdUpdate = listRet.ToList().Count(x => x.Status == (int)HttpStatusCode.OK);
        if (qtdTot == qtdUpdate)
            return Ok(listRet);

        int qtdError = qtdTot - qtdInsert - qtdUpdate;
        if (qtdTot == qtdError)
            return BadRequest(listRet);

        return new ObjectResult(listRet) { StatusCode = (int)HttpStatusCode.MultiStatus };
    }
}
