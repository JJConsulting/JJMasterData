using System.Collections;
using System.Collections.Generic;
using System.Net;
using JJMasterData.WebApi.Controllers;
using JJMasterData.WebApi.Models;
using JJMasterData.WebApi.Services;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit.Extensions.Ordering;

namespace JJMasterData.WebApi.Test.Controllers;


[Order(1)]
[CollectionDefinition("MasterApiController", DisableParallelization = true)]
public class MasterApiControllerTest
{
    private readonly MasterApiController _controller;
    public MasterApiControllerTest()
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                Connection =
                {
                    RemoteIpAddress = new IPAddress(0)
                },
                Session = new DefaultSessionFeature().Session
            }
        };

        IEntityRepository entityRepository = new Factory(); 
        IDataDictionaryRepository dataDictionaryRepository = new DatabaseDataDictionaryRepository(entityRepository);
        var masterApiService = new MasterApiService(accessor, entityRepository, dataDictionaryRepository, null);
        _controller = new MasterApiController(masterApiService);
    }
    
    [Order(4)]
    [Theory]
    [InlineData("ApiTestDictionary","1")]
    public void GetTest(string elementName, string id)
    {
        var result = _controller.Get(elementName, id).Result as OkObjectResult;

        var dictionary = result?.Value as Dictionary<string, object>;
        
        Assert.Equal(1, dictionary?["id"] );
    }
    [Order(3)]
    [Theory]
    [InlineData("ApiTestDictionary")]
    public void GetAllTest(string elementName)
    {
        var result = _controller.GetAll(elementName).Result as OkObjectResult;

        Assert.IsType<MasterApiListResponse>(result?.Value);
    }
    
    [Order(0)]
    [Theory]
    [InlineData("[{\"Id\":1, \"Name\":\"string\",\"Age\":0}]", "ApiTestDictionary", false)]
    [InlineData("[{\"Id\":1, \"Name\":\"new_string\",\"Age\":100}]", "ApiTestDictionary", true)]
    public void PostTest(string parametersString, string elementName, bool replace)
    {
        var parameterList = JsonConvert.DeserializeObject<Hashtable[]>(parametersString);

        if (parameterList == null) 
            return;
        
        var result = _controller.Post(parameterList, elementName, replace);

        var objectResult = result.Result as ObjectResult;

        Assert.Equal(replace ? 200 : 201, objectResult!.StatusCode);
    }
    
    [Order(1)]
    [Theory]
    [InlineData("[{\"Id\":1, \"Name\":\"put_string\",\"Age\":0}]", "ApiTestDictionary")]
    public void PutTest(string parametersString, string elementName)
    {
        var parameterList = JsonConvert.DeserializeObject<Hashtable[]>(parametersString);
        
        var result = _controller.Put(parameterList!, elementName);

        var objectResult = result.Result as ObjectResult;

        Assert.Equal(200, objectResult!.StatusCode);
    }
    
    [Order(2)]
    [Theory]
    [InlineData("[{\"Id\":1, \"Name\":\"patch_string\",\"Age\":0}]", "ApiTestDictionary")]
    public void PatchTest(string parametersString, string elementName)
    {
        var parameterList = JsonConvert.DeserializeObject<Hashtable[]>(parametersString);
        
        var result = _controller.Patch(parameterList!, elementName);

        var objectResult = result.Result as ObjectResult;

        Assert.Equal(200, objectResult!.StatusCode);
    }
    
    [Order(5)]
    [Theory]
    [InlineData("ApiTestDictionary", "1")]
    public void DeleteTest(string elementName, string id)
    {
        var result = _controller.Delete(elementName, id);

        var objectResult = result.Result as ObjectResult;

        Assert.Equal(200, objectResult!.StatusCode);
    }
}