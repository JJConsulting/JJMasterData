using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Resources;
using JJMasterData.Commons.Security;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Abstractions;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Web.Components;
using JJMasterData.Web.DataManager.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Reflection;

namespace JJMasterData.Web.Test.UI.Components;

public class ActionScriptsTests
{
    [Fact]
    public void AddUserAction_ShouldParseUrlRedirectModalTitleForGridTableAction()
    {
        var action = new UrlRedirectAction
        {
            Name = "treatment",
            UrlRedirect = "/SupplierTreatment/Index?requestId={int_rqt_id}",
            IsModal = true,
            ModalTitle = "Request {int_rqt_id}"
        };

        var formElement = new FormElement
        {
            Name = "requests",
            Fields =
            {
                new FormElementField
                {
                    Name = "int_rqt_id",
                    IsPk = true
                }
            }
        };

        var actionContext = new ActionContext
        {
            Action = action,
            FormElement = formElement,
            FormStateData = new FormStateData(
                new Dictionary<string, object?> { ["int_rqt_id"] = 42 },
                PageState.List),
            ParentComponentName = "requests-grid"
        };

        var actionScripts = CreateActionScripts();

        var script = GetUrlRedirectScript(actionScripts, action, actionContext, ActionSource.GridTable);

        Assert.Contains("/SupplierTreatment/Index?requestId=42", script);
        Assert.Contains(",'Request 42','", script);
    }

    private static ActionScripts CreateActionScripts()
    {
        var expressionsService = CreateExpressionsService();
        var urlRedirectService = new UrlRedirectService(
            CreateHttpContextAccessor(),
            Mock.Of<IEntityRepository>(),
            CreateStringLocalizer(),
            null!,
            expressionsService,
            new HmacHelper(Options.Create(new HmacOptions
            {
                SecretKey = "unit-test-secret"
            })));

        return new ActionScripts(
            expressionsService,
            urlRedirectService,
            Mock.Of<Microsoft.AspNetCore.Mvc.IUrlHelper>(),
            Mock.Of<IEncryptionService>(),
            CreateStringLocalizer());
    }

    private static ExpressionsService CreateExpressionsService()
    {
        return new ExpressionsService(
            Array.Empty<IExpressionProvider>(),
            new ExpressionParser(
                Mock.Of<IMasterDataRequestContext>(),
                Mock.Of<IMasterDataUser>(),
                Mock.Of<ILogger<ExpressionParser>>()),
            Mock.Of<IEncryptionService>(),
            Mock.Of<ILogger<ExpressionsService>>());
    }

    private static string GetUrlRedirectScript(
        ActionScripts actionScripts,
        UrlRedirectAction action,
        ActionContext actionContext,
        ActionSource actionSource)
    {
        var method = typeof(ActionScripts).GetMethod(
            "GetUrlRedirectScript",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(method);
        return Assert.IsType<string>(method.Invoke(actionScripts, [action, actionContext, actionSource]));
    }

    private static HttpContextAccessor CreateHttpContextAccessor()
    {
        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private static IStringLocalizer<MasterDataResources> CreateStringLocalizer()
    {
        var localizerMock = new Mock<IStringLocalizer<MasterDataResources>>();
        localizerMock
            .Setup(x => x[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        return localizerMock.Object;
    }
}
