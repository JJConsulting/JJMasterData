<h1>Expressions<small> Any fields and actions supports expressions</small></h1>

## What is a expression?
Expression is a simple way to return a dynamic value.


## How it works?

Expressions can return a boolean or a value at runtime<br>
 - Tipo [val] retorna um valor; 
 - Tipo [exp] retorna o resultado da expressão; 
Example: <br>

"val:1" Return true.
<br>
"val:0" Return false.
<br>
"exp:1=1" Return true.
<br>
"exp:{pagestate} = 'LIST'" If a list return true<br>
"exp:{pagestate} = 'UPDATE' AND {ID} = '1'" If update and a field value ID equals 1 return true<br>

## What is the types of expressions?
- Type [val:] returns a value; (1 or 0) (true or false) ("foo") etc..
- Type [exp:] returns the result of the expression;
- Type [sql:] returns the result of a sql command;
- Type [protheus:] returns the result of a TOTVS ERP Protheus function;

> [!TIP] 
> Check if your field supports all expressions


## How to do it?
Building an expression<br>
> [!WARNING] 
> Contents enclosed in {} (braces) will be replaced by current values ​​at runtime. Following the order:


**System keywords**<br>
{pagestate} = "INSERT" | "UPDATE" | "VIEW" | "LIST" | "FILTER" | "IMPORT">
<br>
Campos do Formuário, UserValues ou Sessão = {FieldName}
<br>
{objname} = Name of the field that triggered the autopostback event

1. UserValues (propriedade do objeto)
2. Form Fields (Field Name)
3. System keywords
4. User Session


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