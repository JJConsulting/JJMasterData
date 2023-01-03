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
using JJMasterData.Commons.Logging;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using Microsoft.Extensions.Logging;

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
            var model = GetEntityGridView();
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
            case "preview":
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
        var gridView = GetFormView();
        var selectedRows = gridView.GetSelectedGridValues();

        if(selectedRows.Count == 1)
        {
            var jsonBytes = _elementService.ExportSingleRow(selectedRows[0]);
            return File(jsonBytes, "application/json", selectedRows[0]["name"] + ".json");
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
        _elementService.Import(new MemoryStream(e.File.Bytes));
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

    public JJGridView GetEntityGridView()
    {
        var gridView = GetFormView();

        var acTools = new UrlRedirectAction
        {
            Icon = IconType.Pencil,
            Name = "tools",
            ToolTip = Translate.Key("Field Maintenance"),
            EnableExpression = "exp:'T' <> {type}",
            IsDefaultOption = true
        };
        gridView.AddGridAction(acTools);

        var renderBtn = new ScriptAction
        {
            Icon = IconType.Eye,
            Name = "preview",
            Text = Translate.Key("Preview"),
            EnableExpression = "exp:'T' <> {type}",
            IsGroup = true
        };
        gridView.AddGridAction(renderBtn);

        var btnDuplicate = new UrlRedirectAction
        {
            Icon = IconType.FilesO,
            Name = "duplicate",
            Text = Translate.Key("Duplicate"),
            EnableExpression = "exp:'T' <> {type}",
            IsGroup = true
        };
        gridView.AddGridAction(btnDuplicate);

        var btnAdd = new UrlRedirectAction
        {
            Name = "btnadd",
            Text = Translate.Key("New"),
            Icon = IconType.Plus,
            ShowAsButton = true,
            UrlRedirect = Url.Action("Add")
        };
        gridView.AddToolBarAction(btnAdd);

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
        gridView.AddToolBarAction(btnImport);

        var btnExport = new ScriptAction
        {
            Name = "btnExport",
            ToolTip = Translate.Key("Export Selected"),
            Icon = IconType.Download,
            ShowAsButton = true,
            Order = 10,
            CssClass = BootstrapHelper.PullRight,
            OnClientClick =
                $"jjdictionary.exportElement('{gridView.Name}', '{Url.Action("Export")}', '{Translate.Key("Select one or more dictionaries")}');"
        };
        gridView.AddToolBarAction(btnExport);

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

        gridView.AddToolBarAction(btnTheme);

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

        gridView.AddToolBarAction(btnAbout);

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

        gridView.AddToolBarAction(btnLog);

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

        gridView.AddToolBarAction(btnSettings);

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

        gridView.AddToolBarAction(btnResources);

        gridView.AddToolBarAction(new SubmitAction()
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

        gridView.OnRenderAction += OnRenderAction;

        return gridView;
    }

    public IActionResult Theme()
    {
        var theme = _themeService.GetTheme();

        _themeService.SetTheme(theme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light);

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete()
    {
        var formView = GetEntityGridView();

        var selectedGridValues = formView.GetSelectedGridValues();

        selectedGridValues
            .Select(value => value["name"]!.ToString()!)
            .ToList()
            .ForEach(metadata => _elementService.DataDictionaryRepository.Delete(metadata));

        return RedirectToAction(nameof(Index));
    }

    private JJGridView GetFormView()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var gridView = _elementService.GetFormView();
        gridView.FormElement.Title =
            $"<img src=\"{baseUrl}/{_themeService.GetLogoPath()}\" style=\"width:8%;height:8%;\"/>";

        return gridView;
    }
}