# Reserved Keywords
We have some reserved keywords, which can be used between {...} in expressions and elsewhere.

## UserId
The USERID is a global identifier to the current user in JJMasterData. It's used for  user identification the following services:

- Data Exportation / Importation
- Audit Log 

You can set the USERID in your application after authentication on both Session or Claims.

```cs
context.Session.SetString("UserId","YourUserIdentifier");
```
or
```cs
identity.AddClaim(new Claim("UserId", "YourUserIdentifier"))
```

## PageState
It's the current state of the form:
```
"INSERT", "UPDATE", "VIEW", "LIST", "FILTER", "IMPORT"
```
## FieldName
It's the name of the field that triggered the autopostback event

## SearchId
It's is a parameter from JJSearchBox component passed to server to filter a list

## SearchText
Used in JJSearchBox, it's a typed text to release a search command. You can be use it to prepare a command in the query.