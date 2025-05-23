# Localization

We use `Microsoft.Extensions.Localization` to localize the application. But you need something to translate your form labels, tooltips etc.

You have 3 ways to do this:

## Implement IStringLocalizer

See [ASP.NET Core Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization) for more information.

## Implement ILocalizationRepository
You can implement `ILocalizationRepository` to load your localization strings from a database for example. This is useful if you want to manage your localization strings in a centralized way.

## JJInfinity
JJInfinity is a powerful tool that allows you to create and manage reports and forms dynamically. It provides a centralized portal for your systems, enabling efficient user access management while ensuring security.
JJMasterData is installed at it by default. With JJInfinity you can easily manage your localization strings for MasterData with our `IStringLocalizer` implementation that recover strings from the database.
Learn more about JJInfinity at [our landing page](https://www.jjconsulting.com.br/en-us/produtos/portal-empresarial).
