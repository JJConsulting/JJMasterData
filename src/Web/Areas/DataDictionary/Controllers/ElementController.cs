using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Services;
using JJMasterData.Core.FormEvents.Args;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Areas.DataDictionary.Controllers;

public class ElementController : DataDictionaryController
{
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private readonly ElementService _elementService;
    private readonly ClassGenerationService _classGenerationService;
    private readonly ScriptsService _scriptsService;

    public ElementController(ElementService elementService, ClassGenerationService classGenerationService, ScriptsService scriptsService, IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        StringLocalizer = stringLocalizer;
        _elementService = elementService;
        _classGenerationService = classGenerationService;
        _scriptsService = scriptsService;
    }

    public async Task<ActionResult> Index()
    {
        try
        {
            _elementService.CreateStructureIfNotExists();
            var model = await GetEntityFormView();
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

    public async Task<IActionResult> Export()
    {
        var formView = await GetFormView();
        var selectedRows = formView.GridView.GetSelectedGridValues();

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
        upload.AddLabel = StringLocalizer["Select Dictionaries"];
        upload.AllowedTypes = "json";
        upload.AutoSubmitAfterUploadAll = false;
        upload.OnFileUploaded += FileUploaded;
    }

    private async void FileUploaded(object? sender, FormUploadFileEventArgs e)
    {
        await _elementService.Import(new MemoryStream(e.File.Bytes));
        if (ModelState.IsValid)
        {
            e.SuccessMessage = StringLocalizer["Dictionary imported successfully!"];
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

    public async Task<IActionResult> ClassSourceCode(string dictionaryName)
    {
        ViewBag.ClassSourceCode = await _classGenerationService.GetClassSourceCode(dictionaryName);
        ViewBag.DictionaryName = dictionaryName;

        return View("ClassSourceCode", "_MasterDataLayout.Popup");
    }

    public async Task<IActionResult> Scripts(string dictionaryName)
    {
        var scripts = await _scriptsService.GetScriptsListAsync(dictionaryName);

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
            await _scriptsService.ExecuteScriptsAsync(dictionaryName, scriptOption);
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            var error = new { success = false, message = ex.Message };
            return new JsonResult("error") { StatusCode = (int)HttpStatusCode.InternalServerError, Value = error };
        }
    }

    public async Task<JJFormView> GetEntityFormView()
    {
        var formView = await GetFormView();

        var acTools = new UrlRedirectAction
        {
            Icon = IconType.Pencil,
            Name = "tools",
            ToolTip = StringLocalizer["Field Maintenance"],
            EnableExpression = "exp:'T' <> {type}",
            IsDefaultOption = true
        };
        formView.GridView.AddGridAction(acTools);

        var renderBtn = new ScriptAction
        {
            Icon = IconType.Eye,
            Name = "preview",
            Text = StringLocalizer["Preview"],
            EnableExpression = "exp:'T' <> {type}",
            IsGroup = true
        };
        formView.GridView.AddGridAction(renderBtn);

        var btnDuplicate = new UrlRedirectAction
        {
            Icon = IconType.FilesO,
            Name = "duplicate",
            Text = StringLocalizer["Duplicate"],
            EnableExpression = "exp:'T' <> {type}",
            IsGroup = true
        };
        formView.GridView.AddGridAction(btnDuplicate);

        var btnAdd = new UrlRedirectAction
        {
            Name = "btnadd",
            Text = StringLocalizer["New"],
            Icon = IconType.Plus,
            ShowAsButton = true,
            UrlRedirect = Url.Action("Add")
        };
        formView.GridView.AddToolBarAction(btnAdd);

        var btnImport = new UrlRedirectAction
        {
            Name = "btnImport",
            ToolTip = StringLocalizer["Import"],
            Icon = IconType.Upload,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = "Import",
            UrlRedirect = Url.Action("Import"),
            Order = 11,
            CssClass = BootstrapHelper.PullRight
        };
        formView.GridView.AddToolBarAction(btnImport);

        var btnExport = new ScriptAction
        {
            Name = "btnExport",
            ToolTip = StringLocalizer["Export Selected"],
            Icon = IconType.Download,
            ShowAsButton = true,
            Order = 10,
            CssClass = BootstrapHelper.PullRight,
            OnClientClick =
                $"DataDictionaryUtils.exportElement('{formView.Name}', '{Url.Action("Export")}', '{StringLocalizer["Select one or more dictionaries"]}');"
        };
        formView.GridView.AddToolBarAction(btnExport);

        var btnAbout = new UrlRedirectAction
        {
            Name = "btnAbout",
            ToolTip = StringLocalizer["About"],
            Icon = IconType.InfoCircle,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = StringLocalizer["About"],
            UrlRedirect = Url.Action("Index", "About", new { Area = "DataDictionary" }),
            Order = 13,
            CssClass = BootstrapHelper.PullRight
        };

        formView.GridView.AddToolBarAction(btnAbout);

        var btnLog = new UrlRedirectAction
        {
            Name = "btnLog",
            ToolTip = StringLocalizer["Log"],
            Icon = IconType.FileTextO,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = StringLocalizer["Log"],
            UrlRedirect = Url.Action("Index", "Log", new { Area = "DataDictionary" }),
            Order = 11,
            CssClass = BootstrapHelper.PullRight
        };

        formView.GridView.AddToolBarAction(btnLog);

        var btnSettings = new UrlRedirectAction
        {
            Name = "btnAppSettings",
            ToolTip = StringLocalizer["Application Options"],
            Icon = IconType.Code,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = StringLocalizer["Application Options"],
            UrlRedirect = Url.Action("Index", "Options", new { Area = "DataDictionary" }),
            Order = 12,
            CssClass = BootstrapHelper.PullRight
        };

        formView.GridView.AddToolBarAction(btnSettings);

        var btnResources = new UrlRedirectAction
        {
            Name = "btnResources",
            ToolTip = StringLocalizer["Resources"],
            Icon = IconType.Globe,
            ShowAsButton = true,
            UrlAsPopUp = true,
            PopUpTitle = StringLocalizer["Resources"],
            UrlRedirect = Url.Action("Index", "Resources", new { Area = "DataDictionary" }),
            Order = 11,
            CssClass = BootstrapHelper.PullRight
        };

        formView.GridView.AddToolBarAction(btnResources);

        formView.GridView.AddToolBarAction(new SubmitAction()
        {
            Name = "btnDeleteMetadata",
            Order = 0,
            Icon = IconType.Trash,
            Text = StringLocalizer["Delete Selected"],
            IsGroup = false,
            ConfirmationMessage = StringLocalizer["Do you want to delete ALL selected records?"],
            ShowAsButton = true,
            FormAction = Url.Action("Delete", "Element")
        });

        formView.GridView.OnRenderAction += OnRenderAction;

        return formView;
    }
    
    public async Task<IActionResult> Delete()
    {
        var formView = await GetEntityFormView();

        var selectedGridValues = formView.GridView.GetSelectedGridValues();

        selectedGridValues
            .Select(value => value["name"]!.ToString()!)
            .ToList()
            .ForEach(metadata => _elementService.DataDictionaryRepository.Delete(metadata));

        return RedirectToAction(nameof(Index));
    }

    private async Task<JJFormView> GetFormView()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var formView = await _elementService.GetFormView();
        formView.FormElement.Title =
            $"<img src=\"{baseUrl}/_content/JJMasterData.Web/images/JJMasterData.png\" style=\"width:8%;height:8%;\"/>";

        return formView;
    }
}