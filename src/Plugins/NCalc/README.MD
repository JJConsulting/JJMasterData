# JJMasterData.NCalc

## Configuration
Add the following method to `MasterDataServiceBuilder`:
```cs
builder.AddJJMasterDataWeb().WithNCalcExpressionProvider();
```

## Usage
Expressions are parsed just like the Value and DataTable providers. Using {}.

NCalc support more operators than DataTable.Compute, like "if" statements.

```
if('{MyColumn}',1,0)
```

Check [NCalc wiki](https://github.com/ncalc/ncalc/wiki) for more information.

## Differences from DataTable.Compute
- NCalc is a mathematical expressions evaluator not a SQL-like syntax parser
- Better performance is expected