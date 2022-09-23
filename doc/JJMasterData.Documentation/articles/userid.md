<h1>USERID<small> A global identifier for some processes in JJMasterData</small></h1>

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