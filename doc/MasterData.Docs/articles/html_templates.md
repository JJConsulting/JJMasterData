# HTML Templates

HTML templates are made using a Liquid template rendered by [Fluid](https://github.com/sebastienros/fluid).

Check [Liquid](https://shopify.github.io/liquid/) docs for more info.

## Available JJMasterData Functions

### 1. `localize`
Localizes a string based on a resource key and optional parameters.

```liquid
{{ localize("ResourceKey", "param1", "param2") }}
```
The function retrieves the localized string from the `MasterDataResources` resource file using the provided key. Optional parameters can be passed for string formatting.

### 2. `formatDate`
Formats a `DateTime` object into a string according to a specified format.

```liquid
{{ formatDate(dateObject, "MM/dd/yyyy") }}
```
If the first argument is a valid `DateTime`, it returns a formatted date string. Otherwise, it returns the default `ToString()` representation of the object.

### 3. `isNullOrEmpty`
Checks if the given string is `null` or empty.

```liquid
{{ isNullOrEmpty(myString) }}
```
Returns `true` if the string is `null` or empty, `false` otherwise.

### 4. `isNullOrWhiteSpace`
Checks if the given string is `null`, empty, or consists only of whitespace characters.

```liquid
{{ isNullOrWhiteSpace(myString) }}
```
Returns `true` if the string is `null`, empty, or whitespace only, `false` otherwise.

### 5. `substring`
Extracts a substring from a given string, starting at a specified index and with an optional length.

```liquid
{{ substring(myString, 0, 5) }} 
```
If only the start index is provided, the function returns the substring from that index to the end of the string. If both start index and length are provided, it extracts that range of characters.


### 6. `urlPath`
Returns the URL path of the application.

```liquid
<div class="avatar avatar-m">
    <img src="{{urlPath()}}Identity/Profile/AvatarByLegacyId?legacyId={{CodUser}}" style="width:30px" class="rounded-circle">
</div>
```

### 6. `dateAsText`
Returns the date as a string representation.

```liquid
{{ dateAsText(dateObject) }}
```

It will be rendered as "1 minute ago" for example.

### 6. `trim`
Removes leading and trailing whitespace from the string.

### 7. `trimStart`
Removes leading whitespace from the string.

### 8. `trimEnd`
Removes trailing whitespace from the string.  


