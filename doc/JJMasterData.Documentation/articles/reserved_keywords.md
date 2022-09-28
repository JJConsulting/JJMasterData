# Reserved Keywords
We have some reserved keywords, which can be used between {...} in expressions and elsewhere.

## USERID
The USERID is a global identifier to the current user in JJMasterData. It's used for  user identification the following services:

- Data Exportation / Importation
- Audit Log 

You can set the USERID in your application after authentication on both Session or Claims.

```cs
context.Session.SetString("USERID","YourUserIdentifier");
```
or
```cs
identity.AddClaim(new Claim("USERID", "YourUserIdentifier"))
```

## pagestate
It's the current state of the form:
```
"INSERT", "UPDATE", "VIEW", "LIST", "FILTER", "IMPORT"
```
## objname
It's the name of the field that triggered the autopostback event

## search_id
It's is a parameter from JJSearchBox component passed to server to filter a list

## search_text
Used in JJSearchBox, it's a typed text to release a search command. You can be use it to prepare a command in the query.