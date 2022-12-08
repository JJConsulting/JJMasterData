using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.WebComponents;
using JJMasterData.Web.Models;
using JJMasterData.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModel;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

[Area("DataDictionary")]
public class ElementController : DataDictionaryController
{
    private readonly ElementService _elementService;
    private readonly ThemeService _themeService;

    public ElementController(ElementService elementService, ThemeService themeService)
    {
        _themeService = themeService;
        _elementService = elementService;
    }

    public ActionResult Index()
    {
        try
        {
            _elementService.CreateStructureIfNotExists();
            var model = GetEntityFormView();
            return View(model);
        }
        catch (DataAccessException)
        {
            return RedirectToAction("Index", "Options", new { Area = "MasterData", isFullscreen = true });
        }
    }

    private void OnRenderAction(object? sender, ActionEventArgs e)
    {
        var formName = e.FieldValues["name"]?.ToString();
        switch (e.Action.Name)
        {
            case "render":
                e.LinkButton.OnClientClick =
                    $"window.open('{Url.Action("Render", "Form", new { dictionaryName = formName, Area = "MasterData" })}', '_blank').focus();";
                break;
            case "tools":
                e.LinkButton.UrlAction = Url.Action("Index", "Entity", new { dictionaryName = formName });
                e.LinkButton.OnClientClick = "";
                break;
            case "duplicate":
                e.LinkButton.UrlAction = Url.Action("Duplicate", "Element", new { dictionaryName = formName });
                e.LinkButton.OnClientClick = "";
                break;
        }
    }

    public IActionResult Add()
    {
        return View();
    }

    public IActionResult Export()
    {
        var formView = GetFormView();
        var selectedRows = formView.GetSelectedGridValues();

        if(selectedRows.Count == 1)
        {
            var jsonBytes = _elementService.ExportSingleRow(selectedRows[0]);
            return File(jsonBytes, "application/json", selectedRows[0]["name"]!.ToString() + ".json");
        }

        var zipBytes = _elementService.ExportMultipleRows(selectedRows);
        return File(zipBytes, "application/zip", "Dictionaries.zip");
    }

    public IActionResult Import()
    {
        return View(new ImportViewModel(ConfigureUploadArea));
    }

    private void ConfigureUploadArea(JJUploadArea upload)
    {
        upload.AddLabel = Translate.Key("Select Dictionaries");
        upload.AllowedTypes = "json";
        upload.AutoSubmitAfterUploadAll = false;
        upload.OnPostFile += OnPostFile;
    }

    private void OnPostFile(object? sender, FormUploadFileEventArgs e)
    {
        _elementService.Import(e.File.FileStream);
        if (ModelState.IsValid)
        {
            e.SuccessMessage = Translate.Key("Dictionary imported successfully!");
        }
        else
        {
            var jjSummary = _elementService.GetValidationSummary();
            foreach (var err in jjSummary.Errors)
                e.ErrorMessage += "<br>" + err;
        }
    }

    public IActionResult Duplicate(string dictionaryName)
    {
        return View(new { originName = dictionaryName });
    }

    public IActionResult ClassSourceCode(string dictionaryName)
    {
        ViewBag.ClassSourceCode = _elementService.GetClassSourceCode(dictionaryName);
        ViewBag.DictionaryName = dictionaryName;

        return View("ClassSourceCode", "_MasterDataLayout.Popup");
    }

    public IActionResult Scripts(string dictionaryName)
    {
        ViewBag.Scripts = _elementService.GetScriptsDictionary(dictionaryName);
        ViewBag.DictionaryName = dictionaryName;
        ViewBag.IsDefault = false;

        return View("Scripts", "_MasterDataLayout.Popup");
    }

    [HttpPost]
    public IActionResult Add(string tableName, bool importFields)
    {
        var element = _elementService.CreateEntity(tableName, importFields);
        if (element != null)
        {
            return RedirectToAction("Index", "Entity", new { dictionaryName = element.Name });
        }

        var jjValidationSummary = _elementService.GetValidationSummary();
        ViewBag.Error = jjValidationSummary.GetHtml();
        return View();
    }

    [HttpPost]
    public IActionResult Duplicate(string originName, string newName)
    {
        if (_elementService.DuplicateEntity(originName, newName))
        {
            return RedirectToAction("Index", new { dictionaryName = newName });
        }

        var jjValidationSummary = _elementService.GetValidationSummary();
        ViewBag.Error = jjValidationSummary.GetHtml();
        return View();
    }

    [HttpPost]
    public IActionResult Scripts(string dictionaryName, string scriptExec)
    {
        try
        {
            _elementService.ExecScripts(dictionaryName, scriptExec);
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            var error = new { success = false, message = ex.Message };
            return new JsonResult("error") { StatusCode = (int)HttpStatusCode.InternalServerError, Value = error };
        }
    }

    public JJFormView GetEntityFormView()
    {
        var formView = GetFormView();

        var acTools = new UrlRedirectAction
        {
            Icon = IconType.Pencil,
            Name = "tools",
            ToolTip = Translate.Key("Field Maintenance"),
            EnableExpression = "exp:'T' <> {type}",
            IsDefaultOption = true
        };
        formView.AddGridAction(acTools);

        var renderBtn = new ScriptAction
        {
            Icon = IconType.Eye,
            Name = "render",
            Text = Translate.Key("Render"),
            EnableExpression = "exp:'T' <> {type}",
            IsGroup = true
        };
        formView.AddGridAction(renderBtn);

        var acdup = new UrlRedirectAction
        {
            Icon = IconType.FilesO,
            Name = "duplicate",
            Text = Translate.Key("Duplicate"),
            EnableExpression = "exp:'T' <> {type}",
            IsGroup = true
        };
        formView.AddGridAction(acdup);

        var btnAdd = new UrlRedirectAction
        {
            Name = "btnadd",
            Text = Translate.Key("New"),
            Icon = IconType.Plus,
            ShowAsButton = true,
            UrlRedirect = Url.Action("Add")
        };
        formView.AddToolBarAction(btnAdd);

        var btnImport = new UrlRedirectAction
        {
            Name = "btnImport",
            ToolTip = Translate.Key("Import"),
            Icon = IconType.Upload,
            ShowAsButton = true,
            UrlAsPopUp = true,
            TitlePopUp = "Import",
            UrlRedirect = Url.Action("Import"),
            Order = 11,
            CssClass = BootstrapHelper.PullRight
        };
        formView.AddToolBarAction(btnImport);

        var btnExport = new ScriptAction
        {
            Name = "btnExport",
            ToolTip = Translate.Key("Export Selected"),
            Icon = IconType.Download,
            ShowAsButton = true,
            Order = 10,
            CssClass = BootstrapHelper.PullRight,
            OnClientClick =
                $"jjdictionary.exportElement('{formView.Name}', '{Url.Action("Export")}', '{Translate.Key("Select one or more dictionaries")}');"
        };
        formView.AddToolBarAction(btnExport);

        var themeMode = _themeService.GetTheme();
        var btnTheme = new UrlRedirectAction
        {
            Name = "btnTheme",
            ToolTip = themeMode == ThemeMode.Light ? Translate.Key("Dark Theme") : Translate.Key("Light Theme"),
            Icon = themeMode == ThemeMode.Light ? IconType.MoonO : IconType.SunO,
            ShowAsButton = true,
            UrlRedirect = Url.Action("Theme"),
            Order = 12,
            CssClass = BootstrapHelper.PullRight
        };

        formView.AddToolBarAction(btnTheme);

        var btnAbout = new UrlRedirectAction
        {
            Name = "btnAbout",
            ToolTip = Translate.Key("About"),
            Icon = IconType.InfoCircle,
            ShowAsButton = true,
            UrlAsPopUp = true,
            TitlePopUp = Translate.Key("About"),
            UrlRedirect = Url.Action("Index", "About", new { Area = "MasterData" }),
            Order = 13,
            CssClass = BootstrapHelper.PullRight
        };

        formView.AddToolBarAction(btnAbout);

        var btnLog = new UrlRedirectAction
        {
            Name = "btnLog",
            ToolTip = Translate.Key("Log"),
            Icon = IconType.FileTextO,
            ShowAsButton = true,
            UrlAsPopUp = true,
            TitlePopUp = Translate.Key("Log"),
            UrlRedirect = Url.Action("Index", "Log", new { Area = "MasterData" }),
            Order = 11,
            CssClass = BootstrapHelper.PullRight
        };

        formView.AddToolBarAction(btnLog);

        var btnSettings = new UrlRedirectAction
        {
            Name = "btnAppSettings",
            ToolTip = Translate.Key("Application Options"),
            Icon = IconType.Code,
            ShowAsButton = true,
            UrlAsPopUp = true,
            TitlePopUp = Translate.Key("Application Options"),
            UrlRedirect = Url.Action("Index", "Options", new { Area = "MasterData" }),
            Order = 12,
            CssClass = BootstrapHelper.PullRight
        };

        formView.AddToolBarAction(btnSettings);

        var btnResources = new UrlRedirectAction
        {
            Name = "btnResources",
            ToolTip = Translate.Key("Resources"),
            Icon = IconType.Globe,
            ShowAsButton = true,
            UrlAsPopUp = true,
            TitlePopUp = Translate.Key("Resources"),
            UrlRedirect = Url.Action("Index", "Resources", new { Area = "MasterData" }),
            Order = 11,
            CssClass = BootstrapHelper.PullRight
        };

        formView.AddToolBarAction(btnResources);

        formView.AddToolBarAction(new SubmitAction()
        {
            Name = "btnDeleteMetadata",
            Order = 0,
            Icon = IconType.Trash,
            Text = Translate.Key("Delete Selected"),
            IsGroup = false,
            ConfirmationMessage = Translate.Key("Do you want to delete ALL selected records?"),
            ShowAsButton = true,
            FormAction = Url.Action("Delete", "Element")
        });

        formView.OnRenderAction += OnRenderAction;

        return formView;
    }

    public IActionResult Theme()
    {
        var theme = _themeService.GetTheme();

        _themeService.SetTheme(theme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light);

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete()
    {
        var formView = GetEntityFormView();

        var selectedGridValues = formView.GetSelectedGridValues();

        selectedGridValues
            .Select(value => value["name"]!.ToString()!)
            .ToList()
            .ForEach(metadata => _elementService.DictionaryRepository.Delete(metadata));

        return RedirectToAction(nameof(Index));
    }

    private JJFormView GetFormView()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var formView = _elementService.GetFormView();
        formView.FormElement.Title =
            $"<img src=\"{baseUrl}/{_themeService.GetLogoPath()}\" style=\"width:8%;height:8%;\"/>";

        return formView;
    }
}