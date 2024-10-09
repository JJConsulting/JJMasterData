#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.DataDictionary.Models;

public sealed class Entity
{
    [Display(Name = "Schema")]
    public required string? Schema { get; set; }
    
    [Display(Name = "Name")]
    public required string Name { get; set; }
    
    [Display(Name = "Table Name")]
    public required string TableName { get; set; }
    
    [Display(Name = "Use Read Procedure")]
    public required bool UseReadProcedure { get; set; }
    
    [Display(Name = "Use Write Procedure")]
    public required bool UseWriteProcedure { get; set; }
    
    [Display(Name = "Read Procedure")]
    public required string? ReadProcedureName { get; set; }
    
    [Display(Name = "Write Procedure")]
    public required string? WriteProcedureName { get; set; }
    
    [Display(Name = "Title")]
    [SyncExpression]
    public required string? Title { get; set; }
    
    [Display(Name = "Size")]
    public required HeadingSize TitleSize { get; set; }
    
    [Display(Name = "SubTitle")]
    [SyncExpression]
    public required string? SubTitle { get; set; }
    
    [Display(Name = "Additional Info")]
    public required string? Info { get; set; }
    
    [Display(Name = "Icon")]
    public required IconType? Icon { get; set; }
    
    [Display(Name = "Connection String")]
    public required Guid? ConnectionId { get; set; }

    public static Entity FromFormElement(FormElement formElement)
    {
        return new Entity
        {
            Name = formElement.Name,
            TableName = formElement.TableName,
            Schema = formElement.Schema,
            UseReadProcedure = formElement.UseReadProcedure,
            UseWriteProcedure = formElement.UseWriteProcedure,
            ReadProcedureName = formElement.ReadProcedureName,
            WriteProcedureName = formElement.WriteProcedureName,
            Title = formElement.Title,
            TitleSize = formElement.TitleSize,
            SubTitle = formElement.SubTitle,
            Info = formElement.Info,
            Icon = formElement.Icon,
            ConnectionId = formElement.ConnectionId
        };
    }

    public void SetFormElement(FormElement formElement)
    {
        formElement.Name = Name;
        formElement.TableName = TableName;
        formElement.Schema = Schema;
        formElement.UseReadProcedure = UseReadProcedure;
        formElement.UseWriteProcedure = UseWriteProcedure;
        formElement.ReadProcedureName = ReadProcedureName;
        formElement.WriteProcedureName = WriteProcedureName;
        formElement.Title = Title;
        formElement.TitleSize = TitleSize;
        formElement.SubTitle = SubTitle;
        formElement.Info = Info;
        formElement.Icon = Icon;
        formElement.ConnectionId = ConnectionId;
    }
}