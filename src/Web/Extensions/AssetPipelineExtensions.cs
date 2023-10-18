using Microsoft.Extensions.DependencyInjection;
using WebOptimizer;

namespace JJMasterData.Web.Extensions;

public static class AssetPipelineExtensions
{
    public static void AddBundles(this IAssetPipeline options)
    {
        var cssFiles = new[]
        {
            "_content/JJMasterData.Web/css/bootstrap/bootstrap-select.css",
            "_content/JJMasterData.Web/css/bootstrap/bootstrap-tagsinput.css",
            "_content/JJMasterData.Web/css/flatpickr/flatpickr.min.css",
            "_content/JJMasterData.Web/css/flatpickr/airbnb.css",
            "_content/JJMasterData.Web/css/highlightjs/ssms.min.css",
            "_content/JJMasterData.Web/css/dropzone/dropzone.min.css",
            "_content/JJMasterData.Web/css/dropzone/dropzone.min.css",
            "_content/JJMasterData.Web/css/codemirror/codemirror.min.css",
            "_content/JJMasterData.Web/css/codemirror/show-hint.css"
        };

        options.AddCssBundle(
            "/css/jjmasterdata-bundle.min.css",
            cssFiles
        );

        options.AddCssBundle(
            "/css/jjmasterdata-bundle-with-bootstrap.min.css",
            cssFiles.Prepend("_content/JJMasterData.Web/css/bootstrap/bootstrap.min.css").ToArray()
        );
        
        var commonJsFiles = new[]
        {
            "_content/JJMasterData.Web/js/jquery/jquery.js",
            "_content/JJMasterData.Web/js/jquery/jquery-form/jquery.form.js",
            "_content/JJMasterData.Web/js/jquery/jquery-number/jquery.number.min.js",
            "_content/JJMasterData.Web/js/jquery/jquery-ui/jquery-ui.min.js",
            "_content/JJMasterData.Web/js/jquery/jquery-validation/jquery.validate.min.js",
            "_content/JJMasterData.Web/js/jquery/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js",
            "_content/JJMasterData.Web/js/flatpickr/flatpickr.min.js",
            "_content/JJMasterData.Web/js/flatpickr/l10n/pt.min.js",
            "_content/JJMasterData.Web/js/flatpickr/l10n/en.min.js",
            "_content/JJMasterData.Web/js/popperjs/popper.min.js",
            "_content/JJMasterData.Web/js/spin/spin.min.js",
            "_content/JJMasterData.Web/js/inputmask/dist/inputmask.min.js",
            "_content/JJMasterData.Web/js/inputmask/dist/bindings/inputmask.binding.js",
            "_content/JJMasterData.Web/js/bootstrap/bootstrap-select/bootstrap-select.min.js",
            "_content/JJMasterData.Web/js/bootstrap/bootstrap-typeahead/bootstrap-typeahead.min.js",
            "_content/JJMasterData.Web/js/bootstrap/bootstrap-tagsinput/bootstrap-tagsinput.min.js",
            "_content/JJMasterData.Web/js/dropzone/dropzone.min.js",
            "_content/JJMasterData.Web/js/codemirror/lib/codemirror.js",
            "_content/JJMasterData.Web/js/codemirror/mode/sql.js",
            "_content/JJMasterData.Web/js/codemirror/addon/hint/show-hint.js",
            "_content/JJMasterData.Web/js/codemirror/addon/hint/sql-hint.js",
            "_content/JJMasterData.Web/js/jjmasterdata/jjmasterdata.js"
        };

        var bootstrap5JsFiles = commonJsFiles.ToList();
        bootstrap5JsFiles.Insert(13, "_content/JJMasterData.Web/js/bootstrap/bootstrap5/bootstrap.bundle.min.js");

        options.AddJavaScriptBundle(
            "/js/jjmasterdata-bundle-bootstrap-5.min.js",
            bootstrap5JsFiles.ToArray()
        ).MinifyJavaScript();

        var bootstrap3JsFiles = commonJsFiles.ToList();
        bootstrap3JsFiles.Insert(13, "_content/JJMasterData.Web/js/bootstrap3/bootstrap.min.js");
        bootstrap3JsFiles.Insert(14, "_content/JJMasterData.Web/js/bootstrap-toggle/bootstrap-toggle.min.js");

        options.AddJavaScriptBundle(
            "/js/jjmasterdata-bundle-bootstrap-3.min.js",
            bootstrap3JsFiles.ToArray()
        );
    }
}