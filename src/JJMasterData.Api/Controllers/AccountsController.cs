using JJMasterData.Api.Models;
using JJMasterData.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Api.Controllers;

[ApiController]
public class AccountsController : ControllerBase
{

    private readonly AccountService Service;

    public AccountsController(AccountService service)
    {
        Service = service;
    }

    /// <summary>
    /// Realizes the login.
    /// </summary>
    /// <param name="form">
    /// user={login}<![CDATA[&]]>password={plain text}
    /// </param>
    /// <response code="200">OK</response>
    /// <response code="406">Not Acceptable</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost]
    [Produces(typeof(UserAccessInfo))]
    [Route("api/accounts/login")]
    public ActionResult<UserAccessInfo> Login(AccountLogin form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(form));

        UserAccessInfo info = Service.Login(form.User, form.Password, form.AppId);

        if (!info.IsValid)
            return Unauthorized();

        return Ok(info);
    }

    /// <summary>
    /// Change the user password.
    /// </summary>
    /// <param name="form">
    /// User data
    /// </param>
    /// <response code="200">OK</response>
    /// <response code="500">Internal Server Error</response>
    /// <response code="406">Not Acceptable</response>
    [HttpPost]
    [Produces(typeof(UserAccessInfo))]
    [Route("api/accounts/changepassword")]
    public ActionResult<UserAccessInfo> ChangePassword(AccountChange form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(form));

        UserAccessInfo info = Service.ChangePassword(form);

        if (!info.IsValid)
            return Unauthorized();

        return Ok(info);
    }


    /// <summary>
    /// Recover the user password.
    /// </summary>
    /// <param name="form">
    /// user={Login}<![CDATA[&]]>appId={App Id}
    /// </param>
    /// <response code="200">Ok</response>
    /// <response code="406">Not Acceptable</response>
    /// <response code="500">Internal Server Error</response>
    [HttpPost]
    [Produces(typeof(UserAccessInfo))]
    [Route("api/accounts/recoverpassword")]
    public ActionResult<UserAccessInfo> RecoverPassword(AccountRecover form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(form));

        var info = Service.RecoverPassword(form);

        if (!info.IsValid)
            Unauthorized();

        return Ok(info);
    }
}
