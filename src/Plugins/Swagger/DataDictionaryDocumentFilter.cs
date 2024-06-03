using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Http.Description;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Swagger
{
    public class DataDictionaryDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            IDataDictionaryRepository dictionaryRepository = null; //Will be discountinued.
            var dictionaries = dictionaryRepository.GetFormElementListAsync().GetAwaiter().GetResult();

            foreach (var formElement in dictionaries)
            {
                var apiOptions = formElement.ApiOptions;
                string key;

                //Get All
                if (formElement.ApiOptions.EnableGetAll)
                {
                    key = $"/MasterApi/{formElement.Name}/";
                    swaggerDoc.paths.Add(key, GetAllPathItem(formElement, apiOptions));
                }

                //Get Detail
                if (formElement.ApiOptions.EnableGetDetail)
                {
                    key = $"/MasterApi/{formElement.Name}/{{id}}";
                    swaggerDoc.paths.Add(key, GetDetailPathItem(formElement, apiOptions));
                }

                //Add 
                if (formElement.ApiOptions.EnableAdd)
                {
                    key = $"/MasterApi/{formElement.Name}";
                    swaggerDoc.paths.Add(key, GetPostPathItem(formElement, apiOptions));
                }

                //Update 
                if (formElement.ApiOptions.EnableUpdate)
                {
                    key = $"/MasterApi/{formElement.Name}/ ";
                    swaggerDoc.paths.Add(key, GetPutPathItem(formElement, apiOptions));
                }

                //Parcial Update
                if (formElement.ApiOptions.EnableUpdatePart)
                {
                    key = $"/MasterApi/{formElement.Name} ";
                    swaggerDoc.paths.Add(key, GetPatchPathItem(formElement, apiOptions));
                }

                //Del
                if (formElement.ApiOptions.EnableDel)
                {
                    key = $"/MasterApi/{formElement.Name}/{{id}} ";
                    swaggerDoc.paths.Add(key, GetDelPathItem(formElement, apiOptions));
                }
            }

            //make operations alphabetic
            var paths = swaggerDoc.paths.OrderBy(e => e.Key.ToLower().Replace("api/", "").Replace("masterapi/", "")).ToList();
            swaggerDoc.paths = paths.ToDictionary(e => e.Key, e => e.Value);
        }

        private static PathItem GetPatchPathItem(FormElement formElement, FormElementApiOptions apiOptions)
        {
            string modelName = formElement.Name.ToLower().Replace("tb_", "").Replace("vw_", "");

            var description = new StringBuilder();
            description.Append(formElement.Title);
            description.Append("<br><br>Change some fields in a record.");
            description.Append("<br>We do not use transactions in this scope, ");
            description.Append("if sending a list of records the return can be 200(all right) or 207(error in some record) ");
            description.Append("in this case you can check the status of each record in the response return.");

            var filterSchema = new Schema
            {
                title = $"{modelName}List",
                type = "array",
                items = GetDictionarySchema(formElement, apiOptions, $"{modelName}List"),
                description = "List of records"
            };

            var oper = new Operation();
            oper.summary = "Update some especific fields";
            oper.description = description.ToString();
            oper.description += "<br><b>Accept-Encoding</b>: gzip, deflate ou utf8 (opcional)";
            oper.tags = new[] { formElement.Name };
            oper.operationId = $"{modelName}_Patch";
            oper.consumes = new[] { "application/json" };
            oper.produces = new[] { "application/json", "application/xml" };
            oper.parameters = new List<Parameter>
            {
                new Parameter
                {
                    name = apiOptions.GetFieldNameParsed("listParam"),
                    description = "Array with the objects<br>Primary key required",
                    @in = "body",
                    required = true,
                    schema = filterSchema
                }
            };
            oper.parameters.Add(GetAcceptLanguageHeaderAttr());

            var baseSchema = new Schema
            {
                title = $"{modelName}Status",
                type = "array",
                items = GetValidationLetterSchema(true),
                description = "List with status and validations"
            };

            var oPathItem = new PathItem();
            oPathItem.patch = oper;
            oPathItem.patch.responses = new Dictionary<string, Response>();
            oPathItem.patch.responses.Add("200", new Response { description = "OK", schema = baseSchema });
            oPathItem.patch.responses.Add("207", new Response { description = "Mult Status" });
            oPathItem.patch.responses.Add("400", new Response { description = "Bad Request" });
            oPathItem.patch.responses.Add("401", new Response { description = "Unauthorized" });
            oPathItem.patch.responses.Add("403", new Response { description = "Token Expired" });
            oPathItem.patch.responses.Add("404", new Response { description = "Not Found" });
            oPathItem.patch.responses.Add("500", new Response { description = "Internal Server Error" });

            return oPathItem;
        }

        private static PathItem GetPostPathItem(FormElement f, FormElementApiOptions metadataApiOptions)
        {
            string modelName = f.Name.ToLower().Replace("tb_", "").Replace("vw_", "");

            var sDescription = new StringBuilder();
            sDescription.Append(f.Title);
            sDescription.Append("<br><br>Insert a list of records.");
            sDescription.Append("<br>We do not use transactions in this scope, ");
            sDescription.Append("if sending a list of records the return can be 201(all right) or 207(error in some record) ");
            sDescription.Append("in this case you can check the status of each record in the response return.");

            var filterSchema = new Schema
            {
                title = $"{modelName}List",
                type = "array",
                items = GetDictionarySchema(f, metadataApiOptions, $"{modelName}List", true),
                description = "List of records"
            };

            var oper = new Operation();
            oper.summary = "Add new records";
            oper.description = sDescription.ToString();
            oper.description += "<br><b>Accept-Encoding</b>: gzip, deflate ou utf8 (opcional)";
            oper.tags = new[] { f.Name };
            oper.operationId = $"{modelName}_Post";
            oper.consumes = new[] { "application/json" };
            oper.produces = new[] { "application/json", "application/xml" };
            oper.parameters = new List<Parameter>
            {
                new Parameter
                {
                    name = metadataApiOptions.GetFieldNameParsed("listParam"),
                    description = "Array with the objects.",
                    @in = "body",
                    required = true,
                    schema = filterSchema
                },
                new Parameter
                {
                    name = metadataApiOptions.GetFieldNameParsed("replace"),
                    description = "If exist record, update otherwise insert. (default false)",
                    @in = "query",
                    required = false,
                    type = "boolean",
                    maxLength = 1
                }
            };
            oper.parameters.Add(GetAcceptLanguageHeaderAttr());

            var baseSchema = new Schema
            {
                title = $"{modelName}Status",
                type = "array",
                items = GetValidationLetterSchema(true),
                description = "List with status and validations"
            };

            var oPathItem = new PathItem();
            oPathItem.post = oper;
            oPathItem.post.responses = new Dictionary<string, Response>();
            oPathItem.post.responses.Add("201", new Response { description = "Created", schema = baseSchema });
            oPathItem.post.responses.Add("207", new Response { description = "Mult Status" });
            oPathItem.post.responses.Add("400", new Response { description = "Bad Request" });
            oPathItem.post.responses.Add("401", new Response { description = "Unauthorized" });
            oPathItem.post.responses.Add("403", new Response { description = "Token Expired" });
            oPathItem.post.responses.Add("404", new Response { description = "Not Found" });
            oPathItem.post.responses.Add("500", new Response { description = "Internal Server Error" });

            return oPathItem;
        }

        private static PathItem GetPutPathItem(FormElement f, FormElementApiOptions metadataApiOptions)
        {
            string modelName = f.Name.ToLower().Replace("tb_", "").Replace("vw_", "");

            var sDescription = new StringBuilder();
            sDescription.Append(f.Title);
            sDescription.Append("<br><br>Update a list of records in batch.");
            sDescription.Append("<br>We do not use transactions in this scope, ");
            sDescription.Append("if sending a list of records the return can be 200(all right) or 207(error in some record) ");
            sDescription.Append("in this case you can check the status of each record in the response return.");


            var filterSchema = new Schema
            {
                title = $"{modelName}List",
                type = "array",
                items = GetDictionarySchema(f, metadataApiOptions, $"{modelName}List"),
                description = "List of records"
            };

            var oper = new Operation();
            oper.summary = "Update records";
            oper.description = sDescription.ToString();
            oper.description += "<br><b>Accept-Encoding</b>: gzip, deflate ou utf8 (opcional)";
            oper.tags = new[] { f.Name };
            oper.operationId = $"{modelName}_Put";
            oper.consumes = new[] { "application/json" };
            oper.produces = new[] { "application/json", "application/xml" };
            oper.parameters = new List<Parameter>
            {
                new Parameter
                {
                    name = metadataApiOptions.GetFieldNameParsed("listParam"),
                    description = "Array with the objects",
                    @in = "body",
                    required = true,
                    schema = filterSchema
                }
            };
            oper.parameters.Add(GetAcceptLanguageHeaderAttr());

            var baseSchema = new Schema
            {
                title = $"{modelName}Status",
                type = "array",
                items = GetValidationLetterSchema(true),
                description = "List with status and validations"
            };

            var oPathItem = new PathItem();
            oPathItem.put = oper;
            oPathItem.put.responses = new Dictionary<string, Response>();
            oPathItem.put.responses.Add("200", new Response { description = "OK", schema = baseSchema });
            oPathItem.put.responses.Add("207", new Response { description = "Mult Status" });
            oPathItem.put.responses.Add("400", new Response { description = "Bad Request" });
            oPathItem.put.responses.Add("401", new Response { description = "Unauthorized" });
            oPathItem.put.responses.Add("403", new Response { description = "Token Expired" });
            oPathItem.put.responses.Add("404", new Response { description = "Not Found" });
            oPathItem.put.responses.Add("500", new Response { description = "Internal Server Error" });

            return oPathItem;
        }

        private static PathItem GetDelPathItem(FormElement f, FormElementApiOptions metadataApiOptions)
        {
            string modelName = f.Name.ToLower().Replace("tb_", "").Replace("vw_", "");
            var pkFields = f.Fields.ToList().FindAll(x => x.IsPk);
            var sDescription = new StringBuilder();
            sDescription.Append(f.Title);
            sDescription.Append("<br>Remove a record. ");
            if (pkFields.Count > 1)
                sDescription.Append("Please enter the values ​​of the PKs separated by commas in the order of the object");
            else
                sDescription.Append("Please enter the value of the primary key as a parameter.");


            string nameFields = string.Empty;
            foreach (var field in pkFields)
            {
                if (nameFields.Length > 0)
                    nameFields += ", ";

                nameFields += field.Name;
            }

            var oPathItem = new PathItem();
            var oper = new Operation();
            oper.summary = "Delete a record";
            oper.description = sDescription.ToString();
            oper.tags = new[] { f.Name };
            oper.operationId = $"{modelName}_Del";
            oper.consumes = null;
            oper.produces = new[] { "application/json", "application/xml" };
            oper.parameters = new List<Parameter>
            {
                new Parameter
                {
                    name = metadataApiOptions.GetFieldNameParsed("id"),
                    description = $"Primary Key Value.<br>{nameFields}",
                    @in = "path",
                    required = true,
                    type = "string"
                }
            };
            oper.parameters.Add(GetAcceptLanguageHeaderAttr());

            oPathItem.delete = oper;
            oPathItem.delete.responses = new Dictionary<string, Response>();

            var baseSchema = GetValidationLetterSchema();
            oPathItem.delete.responses.Add("204", new Response { description = "OK", schema = baseSchema });
            oPathItem.delete.responses.Add("400", new Response { description = "Bad Request" });
            oPathItem.delete.responses.Add("401", new Response { description = "Unauthorized" });
            oPathItem.delete.responses.Add("403", new Response { description = "Token Expired" });
            oPathItem.delete.responses.Add("404", new Response { description = "Not Found" });
            oPathItem.delete.responses.Add("500", new Response { description = "Internal Server Error" });

            return oPathItem;
        }

        private static PathItem GetDetailPathItem(FormElement f, FormElementApiOptions metadataApiOptions)
        {
            string modelName = f.Name.ToLower().Replace("tb_", "").Replace("vw_", "");
            var pkFields = f.Fields.ToList().FindAll(x => x.IsPk);
            var sDescription = new StringBuilder();
            sDescription.Append(f.Title);
            sDescription.Append("<br><b>Accept-Encoding</b>: gzip, deflate ou utf8 (opcional)<br>");
            if (pkFields.Count > 1)
                sDescription.Append("Please enter the values ​​of the PKs separated by commas in the order of the object");
            else
                sDescription.Append("Please enter the value of the primary key as a parameter.");


            string nameFields = string.Empty;
            foreach (var field in pkFields)
            {
                if (nameFields.Length > 0)
                    nameFields += ", ";

                nameFields += field.Name.ToLower();
            }

            var oPathItem = new PathItem();
            var oper = new Operation();
            oper.summary = "Get a record detail";
            oper.description = sDescription.ToString();
            oper.tags = new[] { f.Name };
            oper.operationId = $"{modelName}_Get";
            oper.consumes = null;
            oper.produces = new[] { "application/json", "application/xml" };
            oper.parameters = new List<Parameter>
            {
                new Parameter
                {
                    name = metadataApiOptions.GetFieldNameParsed("id"),
                    description = $"Primary Key Value.<br>{nameFields}",
                    @in = "path",
                    required = true,
                    type = "string"
                }
            };
            oper.parameters.Add(GetAcceptLanguageHeaderAttr());

            oPathItem.get = oper;
            oPathItem.get.responses = new Dictionary<string, Response>();

            var baseSchema = GetDictionarySchema(f, metadataApiOptions, modelName);
            oPathItem.get.responses.Add("200", new Response { description = "OK", schema = baseSchema });
            oPathItem.get.responses.Add("400", new Response { description = "Bad Request" });
            oPathItem.get.responses.Add("401", new Response { description = "Unauthorized" });
            oPathItem.get.responses.Add("403", new Response { description = "Token Expired" });
            oPathItem.get.responses.Add("404", new Response { description = "Not Found" });
            oPathItem.get.responses.Add("500", new Response { description = "Internal Server Error" });

            return oPathItem;
        }

        private static PathItem GetAllPathItem(FormElement f, FormElementApiOptions metadataApiOptions)
        {
            string modelName = f.Name.ToLower().Replace("tb_", "");
            var oPathItem = new PathItem();
            var oper = new Operation();
            oper.summary = "Get all records";
            oper.description = f.Title;
            oper.description += "<br><b>Accept-Encoding</b>: gzip, deflate ou utf8 (opcional)";
            oper.tags = new[] { f.Name };
            oper.operationId = $"{modelName}_GetAll";
            oper.consumes = null;
            oper.produces = new[] { "application/json", "application/xml", "text/csv" };
            oper.parameters = new List<Parameter>();
            oper.parameters.Add(new Parameter
            {
                name = metadataApiOptions.GetFieldNameParsed("pag"),
                description = "Current page",
                @in = "query",
                required = true,
                type = "integer",
                format = "int32",
                @default = 1
            });
            oper.parameters.Add(new Parameter
            {
                name = metadataApiOptions.GetFieldNameParsed("regporpag"),
                description = "Number of records per page",
                @in = "query",
                required = true,
                type = "integer",
                format = "int32",
                @default = 1000
            });
            oper.parameters.Add(new Parameter
            {
                name = metadataApiOptions.GetFieldNameParsed("orderby"),
                description = "Order of records (default is pk ASC). Attention this field is case sensitive.",
                @in = "query",
                required = false,
                type = "string",
                format = "text"
            });
            oper.parameters.Add(new Parameter
            {
                name = metadataApiOptions.GetFieldNameParsed("tot"),
                description = "If you pass the total, the count of records will not be executed saving processing. (optional)",
                @in = "query",
                required = false,
                type = "integer",
                format = "int32",
                @default = 0
            });
            
            var fields = f.Fields.ToList().FindAll(x => !x.IsPk & x.Filter.Type != FilterMode.None);
            foreach (FormElementField item in fields)
            {
                string fieldName = metadataApiOptions.GetFieldNameParsed(item.Name);
                string description = $"Filter available. ({item.Filter.Type.ToString().ToLower()})";
                if (!string.IsNullOrEmpty(item.Label))
                    description += $"<br>{item.Label}";

                if (!string.IsNullOrEmpty(item.HelpDescription))
                    description += $"<br>{item.HelpDescription}";

                var schema = GetFieldSchema(item);
                if (item.Filter.Type == FilterMode.Range)
                {
                    oper.parameters.Add(new Parameter
                    {
                        name = $"{fieldName}_from",
                        description = description,
                        @in = "query",
                        required = false,
                        type = schema.type,
                        format = schema.format
                    });

                    oper.parameters.Add(new Parameter
                    {
                        name = $"{fieldName}_to",
                        description = description,
                        @in = "query",
                        required = false,
                        type = schema.type,
                        format = schema.format
                    });
                }
                else
                {
                    var p = new Parameter
                    {
                        name = fieldName,
                        @in = "query",
                        required = false,
                        type = schema.type,
                        format = schema.format
                    };

                    if (item.Component == FormComponent.ComboBox
                        && item.DataItem != null
                        && item.DataItem.Items != null
                        && item.DataItem.Items.Count > 0)
                    {
                        p.@enum = new List<object>();
                        foreach (DataItemValue dataItem in item.DataItem.Items)
                        {
                            p.@enum.Add(dataItem.Id);
                            description += $"<br>{dataItem.Id} = {dataItem.Description}";
                        }
                    }

                    p.description = description;
                    oper.parameters.Add(p);
                }
            }
            oper.parameters.Add(GetAcceptLanguageHeaderAttr());

            oPathItem.get = oper;
            oPathItem.get.responses = new Dictionary<string, Response>();

            string totVarName = metadataApiOptions.GetFieldNameParsed("tot");
            string fieldsVarName = metadataApiOptions.GetFieldNameParsed("fields");

            var baseSchema = new Schema();
            baseSchema.title = $"{modelName}Response";
            baseSchema.type = "object";
            baseSchema.properties = new Dictionary<string, Schema>();
            baseSchema.properties.Add(totVarName, new Schema
            {
                type = "integer",
                format = "int32",
                description = "Total records"
            });

            baseSchema.properties.Add(fieldsVarName, new Schema
            {
                type = "array",
                description = "Properties",
                uniqueItems = true,
                items = GetDictionarySchema(f, metadataApiOptions, modelName)
            });

            baseSchema.required = new List<string> { totVarName, fieldsVarName };

            oPathItem.get.responses.Add("200", new Response { description = "OK", schema = baseSchema });
            oPathItem.get.responses.Add("400", new Response { description = "Bad Request" });
            oPathItem.get.responses.Add("401", new Response { description = "Unauthorized" });
            oPathItem.get.responses.Add("403", new Response { description = "Token Expired" });
            oPathItem.get.responses.Add("404", new Response { description = "Not Found" });
            oPathItem.get.responses.Add("500", new Response { description = "Internal Server Error" });

            return oPathItem;
        }

        private static Schema GetValidationLetterSchema(bool enableDataField = false)
        {
            var modelSchema = new Schema();
            modelSchema.title = "validationLetter";
            modelSchema.type = "object";
            modelSchema.required = new List<string>();
            modelSchema.properties = new Dictionary<string, Schema>();
            modelSchema.properties.Add("status", new Schema
            {
                description = "Http Response Code",
                type = "integer",
                format = "int32"
            });

            modelSchema.properties.Add("message", new Schema
            {
                description = "Error Message",
                type = "string"
            });

            modelSchema.properties.Add("validationList", new Schema
            {
                description = "Detailed error list",
                type = "array",
                items = new Schema
                {
                    description = "Field, Value",
                    type = "object"
                }
            });

            if (enableDataField)
            {
                modelSchema.properties.Add("data", new Schema
                {
                    description = "Return of fields, identity for example",
                    type = "array",
                    items = new Schema
                    {
                        description = "Field, Value",
                        type = "object"
                    }
                });
            }

            modelSchema.required.Add("status");
            modelSchema.required.Add("message");

            return modelSchema;
        }

        private static Schema GetDictionarySchema(FormElement f, FormElementApiOptions metadataApiOptions, string modelName, bool ignoreIdentity = false)
        {
            var modelSchema = new Schema();
            modelSchema.title = modelName;
            modelSchema.type = "object";
            modelSchema.properties = new Dictionary<string, Schema>();
            modelSchema.required = new List<string>();

            foreach (FormElementField item in f.Fields)
            {
                if (ignoreIdentity && item.IsPk && item.AutoNum)
                    continue;

                string fieldName = metadataApiOptions.GetFieldNameParsed(item.Name);
                var itemSchema = GetFieldSchema(item);
                modelSchema.readOnly = item.DataBehavior == FieldBehavior.ViewOnly;
                modelSchema.properties.Add(fieldName, itemSchema);

                if (item.IsRequired | item.IsPk)
                    modelSchema.required.Add(fieldName);
            }

            if (modelSchema.required.Count == 0)
                modelSchema.required = null;

            return modelSchema;
        }

        private static Schema GetFieldSchema(FormElementField item)
        {
            var itemSchema = new Schema();
            itemSchema.description = item.Label;
            if (item.IsPk)
                itemSchema.description += " (<span class='propType'>PK<span>)";

            if (!string.IsNullOrEmpty(item.HelpDescription))
                itemSchema.description += $"<br> {item.HelpDescription}";

            switch (item.DataType)
            {
                case FieldType.Int:
                    itemSchema.type = "integer";
                    itemSchema.format = "int32";
                    break;
                case FieldType.Float:
                    itemSchema.type = "number";
                    itemSchema.format = "float";
                    break;
                case FieldType.Date:
                    itemSchema.type = "string";
                    itemSchema.format = "date";
                    break;
                case FieldType.DateTime:
                case FieldType.DateTime2:
                    itemSchema.type = "string";
                    itemSchema.format = "date-time";
                    break;
                default:
                    itemSchema.type = "string";
                    if (item.Size > 0)
                        itemSchema.maxLength = item.Size;
                    break;
            }


            if (item.Component == FormComponent.ComboBox
                        && item.DataItem != null
                        && item.DataItem.Items != null
                        && item.DataItem.Items.Count > 0)
            {
                foreach (DataItemValue dataItem in item.DataItem.Items)
                {
                    itemSchema.description += $"<br>{dataItem.Id} = {dataItem.Description}";
                }
            }

            return itemSchema;
        }

        private static Parameter GetAcceptLanguageHeaderAttr()
        {
            return new Parameter
            {
                name = "Accept-Language",
                description = "Culture Code",
                @in = "header",
                required = false,
                type = "string",
                @default = Thread.CurrentThread.CurrentUICulture.Name
            };
        }

    }
}