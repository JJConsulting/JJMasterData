# Authorization

The `DataDictionary` area manages metadata and should be restricted to administrators. The `MasterData` area renders forms for end users; authorize access to each dictionary according to the application permissions.

Authentication belongs to the host. Once its authentication scheme and policies are registered, protect both areas:

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.MapDataDictionary().RequireAuthorization("DictionaryAdmin");
app.MapMasterData().RequireAuthorization("MasterDataUser");
```

Define these policies in `AddAuthorization`. For per-dictionary permissions, inspect the `elementName` route value in your authorization handler. Apply policies separately to API groups if you map them.

`MasterDataCoreOptions.UserIdClaimType` defaults to `ClaimTypes.NameIdentifier`. `IMasterDataUser.Id` reads that claim and supplies user identity to features such as audit and import/export. Configure the claim type if the host uses another identifier.

The built-in render controller shows an editor shortcut for an identified user with the `Admin` role or a `DataDictionary` claim. This shortcut does not authorize the administration routes.

## Context-aware expressions and protected fields

A custom expression function can resolve the current user to decide action visibility. It must also handle evaluation without an HTTP request, and it must not capture one user's identity in shared options. See [Add application expression functions](../cookbook/custom-expression-functions.md).

`JJDataPanel.SecretValues` protects round-trip field values against editing, but does not establish access to the referenced record or prove that previously granted access is still valid. Recheck authorization when the host processes the submission and rebuild sensitive values from trusted context.
