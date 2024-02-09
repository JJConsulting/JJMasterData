# JJMasterData.NCalc

## Configuration
Add the following method to `MasterDataServiceBuilder`:
```cs
builder.AddJJMasterDataWeb().WithNCalcExpressionProvider();
```

## Options
- Check <xref:JJMasterData.NCalc.Configuration.NCalcExpressionProviderOptions>

## Usage
Expressions are pre-parsed just like the Value and DataTable providers. Using {}.

NCalc support more operators and alias than DataTable.Compute, like "if" statements or ternary operators.

```
if('{MyColumn}',1,0)
```

```
'{MyColumn}' ? 'true result' : 'false result'
```

```
'{MyColumn}' != 1 AND {'MyColumn'} <> 1
```

You can also create your own custom functions.

Check [NCalc wiki](https://github.com/ncalc/ncalc/wiki) for more information.

## Differences from DataTable.Compute
- NCalc is a mathematical expressions evaluator not a SQL-like syntax parser
- Better performance is expected

## Executing C# code at your expressions
You can execute C# code using the following example;

Program.cs:

```
builder.Services.AddJJMasterDataWeb()
.WithNCalcExpressionProvider(
    new NCalcExpressionProviderOptions
    {
        ReplaceDefaultExpressionProvider = true,
        AdditionalFunctions = [
            (name, args) =>
            {
                args.Result = name switch
                {
                    "now" => DateTime.Now,
                    "myawesomefunction" => MyCustomClass.Execute(args.Parameters[0], args.Parameters[1]),
                     _ => args.Result
                };
            }
        ]
    }
);


> [!WARNING] 
> NCalc send functions names in lower ase.
```
At your expression:
```
exp: myAwesomeFunction('{StringValue}',{IntValue})
```
