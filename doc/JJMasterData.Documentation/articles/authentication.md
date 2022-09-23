<h1>Authentication<small> How to secure your routes on JJ MasterData</small></h1>

JJMasterData uses 3 controller attributes with Authorize and a policy by default. If you are not familiarized with the concept of policies, please check this [link](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-6.0).

- DataDictionary
- MasterData
- Log

DataDictionary and MasterData attributes are used in all Controllers with the same area name. Keep in mind that the end-user only access the MasterData Area. Log attribute is used just on the Log Controller if you are using JJMasterData default log. 

You can also inject your custom attributes for all routes using:

```cs
app.UseJJMasterData(options=>{
    options.RouteAttributes = //Your array with custom attributes.
})
```

You will need in your application, implement a **authentication**  service, before implementing a **authorization** one. Please check [ASP.NET Core docs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-6.0) for more information.

In the example bellow, I'm using a basic cookie authentication to simplify the process. You can use any supported authentication service.

```cs
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "SharedCookie";
    }).AddCookie("SharedCookie");
```

To apply your authorization rules on these policies, use the example bellow in your .NET 6 application Program.cs.

```cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DataDictionary", policy => policy RequireClaim("DataDictionary"));
});
```

If you want to protect specific actions or fields in your DataDictionary, you will need to implement your own [JJFormView](/articles/components/form_view.md) in your custom View or use the [IFormEvent](/articles/form_events/intro.md) interface in the method OnInstanceCreated, customizing your JJFormView object. 

Don't forget to secure every policy in **production**, this is just a example for learning purposes.
