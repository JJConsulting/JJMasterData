<h1>Internationalization<small> i18n</small></h1>

# How to do

You have 3 scenarios to do this:

1 - Fork JJMasterData, add a resource file for your language like [this one](https://github.com/JJConsulting/JJMasterData/blob/main/src/JJMasterData.Commons/Language/ResourceStrings_pt-br.resx), and send us a pull request. Other people from your country will have the values already out of the box.

2 - You will need to add the following line to your Program.cs, with cultures you will be using:
```cs
builder.Services.AddJJMasterDataWeb();
builder.Services.AddUrlRequestCultureProvider(
    new CultureInfo("zh-CN"),
    new CultureInfo("en-US")
);
```
Go to ```/en-us/MasterData/Resources``` and populate the database with your culture values, but only your system will reflect these changes

3 - Implement your own [ITranslator](articles/configurations.md#what-to-configure) with your custom logic.


<br>

# Why we implemented in the database

We put the values in the database because dictionaries are created dynamically, like labels, action names, and other fields in dictionaries. These strings are translated at runtime and can be added to the resource table or with the user interface.