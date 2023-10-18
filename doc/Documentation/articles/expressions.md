<h1>Expressions<small> Any fields and actions supports expressions</small></h1>

## What is a expression?
Expression is a simple way to return a dynamic value.

## How it works?

Expressions can return a boolean or a value at runtime<br>
Example: <br>

"val:1" Return true.
<br>
"val:0" Return false.
<br>
"exp:1=1" Return true.
<br>
"exp:{pagestate} = 'LIST'" If a list return true<br>
"exp:{pagestate} = 'UPDATE' AND {ID} = '1'" If update and a field value ID equals 1 return true<br>

## What are the default expression providers?
- Type [val:] returns a value; (1 or 0) (true or false) ("foo") etc..
- Type [exp:] returns the result of the expression from `DataTable.Compute`;
- Type [sql:] returns the result of a sql command;

> [!TIP] 
> Check if your field supports all expressions


## How to do it?
Building an expression<br>
> [!WARNING] 
> Contents enclosed in {} (braces) will be replaced by current values ​​at runtime. Following the order:


**System keywords**<br>
- {PageState} = "INSERT" | "UPDATE" | "VIEW" | "LIST" | "FILTER" | "IMPORT"
- {ComponentName} = Name of the component that triggered the AutoPostBack event
- {UserId} = Identifier of the authenticated user

Dynamic values will be recovered in the following order:
1. UserValues
2. FormValues
3. System keywords
4. UserSession


## Examples

Example using [val:] + text<br>
1. val:a simple text;
2. val:10000;
```cs
var field = new ElementField();
field.DefaultValue = "val:test";
```

Example using [exp:] + expression<br>
1. exp:{field1};
2. exp:({field1} + 10) * {field2};
```cs
var field = new ElementField();
field.DefaultValue = "exp:{UserId}";
```

Example using [sql:] + query<br>
1. sql:select 'foo';
2. sql:select count(*) from table1;
```cs
var field = new ElementField();
field.DefaultValue = "sql:select field2 from table1 where field1 = '{field1}'";
```

## Implementing your own expression provider

Implement the [IExpressionProvider](https://portal.jjconsulting.com.br/jjdoc/lib/JJMasterData.Core.Expressions.Abstractions.IExpressionProvider.html) interface and add to your services your custom provider.

```cs
builder.Services.AddJJMasterDataWeb().WithExpressionProvider<TMyCustomProvider>();
//or

```

## Protheus Plugin

Add to your project:
```cs
    builder.Services.AddJJMasterDataWeb().WithProtheusServices();
```
Example using [protheus:] + "UrlProtheus", "NameFunction", "Parameters" <br>
1. protheus:"http://localhost/jjmain.apw","u_test","";
2. protheus:"http://localhost/jjmain.apw","u_test","{field1};parm2";
```cs
var field = new ElementField();
field.DefaultValue = "protheus:'http://10.0.0.6:8181/websales/jjmain.apw', 'u_vldpan', '1;2'";
```

> [!IMPORTANT] 
> A invalid expression will throw a exception in the application

> [!WARNING] 
> For Protheus calls apply JJxFun patch and configure http connection in Protheus
> 


## 