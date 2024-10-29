# HTML Templates

HTML templates are made using a Liquid template rendered by [Fluid](https://github.com/sebastienros/fluid).

## Objects
Objects are used to output dynamic content and are enclosed in `{{ }}`.

```liquid
{{ variable_name }}
```
If you are at Grid Rendering Template, you can use {{ MyField }}, if you are at [HTML Template Action, read more here](actions/html_template_action.md).

## Tags
Tags control the logic and flow. They are enclosed in `{% %}`.

### Control Flow:
```liquid
{% if age > 18 %}
  You are an adult
{% elsif age > 12 %}
  You are a teenager
{% else %}
  You are a child
{% endif %}
```

### Loops:
```liquid
{% for child in children %}
  * {{ child }}
{% else %}
  You have no kids. Enjoy all your money.
{% endfor %}
```

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
