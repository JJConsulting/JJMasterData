using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Web.Models;
using JJMasterData.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class ElementController : DataDictionaryController
{
    private readonly ElementService _elementService;

    public ElementController(ElementService elementService)
    {
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
            return RedirectToAction("Index", "Options", new { Area = "DataDictionary", isFullscreen = true });
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

    private async void OnPostFile(object? sender, FormUploadFileEventArgs e)
    {
        await _elementService.Import(new MemoryStream(e.File.Bytes));
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

    public async Task<IActionResult> Scripts(string dictionaryName)
    {
        var scripts = await _elementService.GetScriptsListAsync(dictionaryName);

        var model = new ElementScriptsViewModel
        {
            DictionaryName = dictionaryName,
            CreateTableScript = scripts[0]!,
            WriteProcedureScript = scripts[1]!,
            ReadProcedureScript = scripts[2]!,
            AlterTableScript = scripts[3]
        };

        return View(model);
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
    public async Task<IActionResult> Scripts(string dictionaryName, string scriptOption)
    {
        try
        {
            await _elementService.ExecuteScriptsAsync(dictionaryName, scriptOption);
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            var error = new { success = false, message = ex.Message };
            return new JsonResult("error") { StatusCode = (int)HttpStatusCode.InternalServerError, Value = error };
        }
    }

    public JJGridView GetEntityFormView()
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

        var btnAbout = new UrlRedirectAction
        {
            Name = "btnAbout",
            ToolTip = Translate.Key("About"),
            Icon = IconType.InfoCircle,
            ShowAsButton = true,
            UrlAsPopUp = true,
            TitlePopUp = Translate.Key("About"),
            UrlRedirect = Url.Action("Index", "About", new { Area = "DataDictionary" }),
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
            UrlRedirect = Url.Action("Index", "Log", new { Area = "DataDictionary" }),
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
            UrlRedirect = Url.Action("Index", "Options", new { Area = "DataDictionary" }),
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
            UrlRedirect = Url.Action("Index", "Resources", new { Area = "DataDictionary" }),
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
    
    public IActionResult Delete()
    {
        var formView = GetEntityFormView();

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
            $"<img src=\"{baseUrl}/_content/JJMasterData.Web/images/JJMasterData.png\" style=\"width:8%;height:8%;\"/>";

        return gridView;
    }
}