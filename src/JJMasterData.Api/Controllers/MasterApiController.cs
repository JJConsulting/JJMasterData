using System.Collections;
using System.Net;
using JJMasterData.Api.Models;
using JJMasterData.Api.Services;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Api.Controllers;

[Authorize]
[ApiController]
[Route("masterApi/{elementName}")]
public class MasterApiController : ControllerBase
{
    private MasterApiService Service { get; }

    public MasterApiController(MasterApiService service)
    {
        Service = service;
    }

    [HttpGet]
    [Produces(typeof(MasterApiListResponse))]
    [Route("{pag?}/{regporpag?}/{orderby?}/{tot?}")]
    public ActionResult<MasterApiListResponse> GetAll(string elementName, [FromQuery]int pag = 1,
        [FromQuery]int regporpag = 1000, [FromQuery]string? orderby = null, [FromQuery]int? tot = 0)
    {
        if (Request?.Headers?.Accept.ToString().Contains("text/csv") ?? false)
        {
            string text = Service.GetListFieldAsText(elementName, pag, regporpag, orderby);

            return Content(text, "text/csv");
        }

        var response = Service.GetListFields(elementName, pag, regporpag, orderby, tot.Value);
        return Ok(response);
    }


    [HttpGet]
    [Produces(typeof(Dictionary<string,object>))]
    [Route("{id}")]
    public ActionResult<Dictionary<string,object>> Get(string elementName, string id)
    {
        return Ok(Service.GetFields(elementName, id));
    }


    [HttpPost]
    [Route("")]
    public ActionResult<ResponseLetter> Post([FromBody]Hashtable[] listParam, string elementName, bool replace = false)
    {
        return GetResponseMessage(Service.SetFields(listParam, elementName, replace));
    }


    [HttpPut]
    [Route("")]
    public ActionResult<ResponseLetter> Put([FromBody]Hashtable[] listParam, string elementName)
    {
        return GetResponseMessage(Service.UpdateFields(listParam, elementName));
    }


    [HttpPatch]
    [Route("")]
    public ActionResult<ResponseLetter> Patch([FromBody] Hashtable[] listParam, string elementName)
    {
        return GetResponseMessage(Service.UpdatePart(listParam, elementName));
    }


    [HttpDelete]
    [Route("{id}")]
    public ActionResult<ResponseLetter> Delete(string elementName, string id)
    {
        return Ok(Service.Delete(elementName, id));
    }


    [HttpPost]
    [Produces(typeof(FormValues[]))]
    [Route("trigger/{pageState?}/{objname?}")]
    public ActionResult<ResponseLetter> PostTrigger(string elementName, [FromBody]Hashtable paramValues, PageState pageState, string objname = "")
    {
        return Ok(Service.PostTrigger(elementName, paramValues, pageState, objname));
    }
    
    [HttpGet]
    [Route("{id}/file/{fieldName}/{fileName}")]
    public IActionResult GetFile(string elementName, string id, string fieldName, string fileName)
    {
        var dictionary = new DictionaryDao().GetDictionary(elementName);

        var formElement = dictionary.GetFormElement();

        var folderPath = formElement.Fields.First(f => f.Name == fieldName).DataFile.FolderPath;
        
        var path = Path.Combine(folderPath, id);
        
        string? file = Directory.GetFiles(path).FirstOrDefault(f => f.EndsWith(fileName));

        if (file == null)
            throw new KeyNotFoundException(Translate.Key("File not found"));

        var fileStream = new FileStream(Path.Combine(path, file), FileMode.Open, FileAccess.Read, FileShare.Read);
        
        return File(fileStream, "application/octet-stream", fileName);
    }
    
    [HttpPost]
    [Route("{id}/file/{fieldName}")]
    public IActionResult PostFile(string elementName, string fieldName, string id, IFormFile file)
    {
        var dictionary = new DictionaryDao().GetDictionary(elementName);
    
        var formElement = dictionary.GetFormElement();

        var field = formElement.Fields.First(f => f.Name == fieldName);
        var folderPath = field.DataFile.FolderPath;

        var path = Path.Combine(folderPath, id);

        if (!field.DataFile.MultipleFile)
        {
            foreach (var fileInfo in new DirectoryInfo(path).EnumerateFiles())
            {
                fileInfo.Delete(); 
            }
        }

        using var fileStream = new FileStream(Path.Combine(path, file.FileName), FileMode.OpenOrCreate, FileAccess.ReadWrite);

        file.CopyToAsync(fileStream);
        
        var factory = new Factory();

        var hash = new Hashtable();

        int i = 0;
        foreach (var pk in formElement.Fields.Where(f => f.IsPk))
        {
            hash[pk.Name] = id.Split(",")[i];
            i++;
        }
        
        var values = factory.GetFields(elementName, hash);
        
        if (field.DataFile.MultipleFile)
        {
            var currentFiles = values[field.Name]!.ToString()!.Split(",").ToList();
        
            if (!currentFiles.Contains(file.FileName))
            {
                currentFiles.Add(file.FileName);
                values[field.Name] = string.Join(",", currentFiles);
            }
        }
        else
        {
            values[field.Name] = file.FileName;
        }
        
        factory.SetValues(formElement, values);
        
        return Created($"masterApi/{elementName}/{id}/{fieldName}/{file.FileName}","File successfully created.");
    }
    
    [HttpDelete]
    [Route("{id}/file/{fieldName}/{fileName}")]
    public IActionResult DeleteFile(string elementName, string fieldName, string id, string fileName)
    {
        var dictionary = new DictionaryDao().GetDictionary(elementName);
    
        var formElement = dictionary.GetFormElement();

        var field = formElement.Fields.First(f => f.Name == fieldName);
        var folderPath = field.DataFile.FolderPath;

        var path = Path.Combine(folderPath, Path.Combine(id, fileName));
        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);
        else
            throw new KeyNotFoundException(Translate.Key("File not found"));
        
        var factory = new Factory();

        var hash = new Hashtable();

        int i = 0;
        foreach (var pk in formElement.Fields.Where(f => f.IsPk))
        {
            hash[pk.Name] = id.Split(",")[i];
            i++;
        }
        
        var values = factory.GetFields(elementName, hash);
        
        if (field.DataFile.MultipleFile)
        {
            var currentFiles = values[field.Name]!.ToString()!.Split(",").ToList();
        
            if (currentFiles.Contains(fileName))
            {
                currentFiles.Remove(fileName);
                values[field.Name] = string.Join(",", currentFiles);
            }
        }
        else
        {
            values[field.Name] = null;
        }
        
        factory.SetValues(formElement, values);
        
        return Ok("File successfully deleted.");
    }

    private ActionResult<ResponseLetter> GetResponseMessage(List<ResponseLetter> listRet)
    {
        if (listRet.Count == 1)
            return new ObjectResult(listRet) { StatusCode = listRet.First().Status } ;

        int qtdTot = listRet.Count;
        int qtdInsert = listRet.Count(x => x.Status == (int)HttpStatusCode.Created);
        if (qtdTot == qtdInsert)
            return Created(nameof(GetResponseMessage),listRet);

        int qtdUpdate = listRet.Count(x => x.Status == (int)HttpStatusCode.OK);
        if (qtdTot == qtdUpdate)
            return Ok(listRet);

        int qtdError = qtdTot - qtdInsert - qtdUpdate;
        
        return qtdTot == qtdError ? BadRequest(listRet) : new ObjectResult(listRet) { StatusCode = 207};
    }

}
