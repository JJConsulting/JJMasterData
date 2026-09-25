# Components and file fields

`FormElementField` separates data from presentation. `DataType`, `Size`, key settings and requiredness describe the database contract; `Component` chooses the form editor and its behavior. Changing a component does not change the column type, and an editor should be selected only when it can accept and format the field's `DataType` correctly.

For example, use `Number`, `Slider` or `Currency` with `Int`, `Float` or `Decimal` data; use `CheckBox` with `Bit`; and use `Date`, `DateTime` or `Hour` for date/time data. The default component is `Text`. When a field is created from an entity field, the default is inferred: date values use `Date`, integers use `Number`, and text longer than 290 characters uses `TextArea`.

## Choosing an editor

| Category | Components | When to use them |
| --- | --- | --- |
| Text | `Text`, `TextArea`, `Password`, `Email` | Plain, multiline, protected or email text. `TextArea` is appropriate for longer input. |
| Numeric | `Number`, `Currency`, `Percentage`, `Slider` | Numeric values. `Currency` and `Percentage` provide localized visual formatting; `Slider` is useful for a bounded range. |
| Date and time | `Date`, `DateTime`, `Hour` | A calendar date, a date with time, or a time of day. |
| Boolean | `CheckBox` | Boolean fields (`Bit`). It can render as a checkbox, switch or button. |
| Selection | `ComboBox`, `Search`, `Lookup`, `RadioButtonGroup` | A value chosen from a list, query or related dictionary. These components require `DataItem`. |
| Brazilian formats | `Cnpj`, `Cpf`, `CnpjCpf`, `Cep`, `Tel`, `Phone` | Brazilian identifiers, postal codes and phone numbers. These provide input masks; validation is applied where supported. |
| Special | `Color`, `Icon`, `CodeEditor` | A color picker, icon picker or code editor. |
| Files | `File` | Upload and manage one or more files. Requires `DataFile`. |

`Size` continues to be a database concern. It is not a general-purpose width setting for the editor; use the layout settings described below to control its width on the form.

## Attributes and layout

`Attributes` contains rendering options supported by the selected component. Common options are:

| Component | Attributes |
| --- | --- |
| Text-based editors | `placeholder` |
| `TextArea` | `rows` |
| `Number`, `Slider` | `min`, `max`, `step` |
| `Currency` | `culture-info` |
| `CheckBox` | `is-switch` or `is-button` |
| `Date`, `DateTime`, `Hour` | `autocompletePicker` |
| `ComboBox`, `Search`, `Lookup` | `popupsize`, `popuptitle` |

The dictionary administration UI stores these attributes for the applicable component. Custom attributes are rendering-only, so do not use them as a substitute for server-side validation.

Place a field with `PanelId`, group fields into the same form row with `LineGroup`, and set responsive Bootstrap classes with `CssClass` (for example, `col-sm-6`). `RenderAsStatic` displays its value as static content, while `HelpDescription` adds help beside the label. `DataBehavior = ViewOnly` makes a field read-only outside filter pages.

## List and relationship components

`ComboBox`, `Search`, `Lookup` and `RadioButtonGroup` obtain their choices from `DataItem`. Select exactly one source type:

| `DataItemType` | Source | Configuration |
| --- | --- | --- |
| `Manual` | A fixed list | Populate `Items` with IDs and descriptions. |
| `SqlCommand` | A query | Configure `Command`; it must return an ID and a description. |
| `ElementMap` | Another data dictionary | Configure `ElementMap` to identify the target element and fields. |

`FirstOption` adds the configured leading choice, and `RadioLayout`, `EnableMultiSelect`, `ShowIcon` and `GridBehavior` refine how the list appears. Multi-select values need storage and downstream processing that support multiple values; it is generally best suited to write-only fields or a separate relationship table.

If one list depends on another field, reference the current form field in its query or mapping and enable `AutoPostBack` on the source field. A change then reloads the form and repopulates dependent editors. Use `TriggerExpression` for calculated values, and `VisibleExpression` or `EnableExpression` to change whether a field is available. See [expressions](expressions.md) for expression syntax and timing.

## File fields

Set `Component` to `File` and configure `DataFile`. The field itself cannot have `DataBehavior = Virtual`; its file settings are:

| Property | Meaning |
| --- | --- |
| `FolderPath` | Storage destination; required. |
| `AllowedTypes` | Comma-separated permitted extensions, such as `pdf,png,jpg`; use `*` for any type. |
| `MaxFileSize` | Maximum upload size in MB. |
| `MultipleFile` | Allows multiple uploads. |
| `DragDrop` / `AllowPasting` | Enables drag-and-drop or clipboard upload. |
| `ViewGallery` | Shows uploaded images as a gallery. |
| `ExportAsLink` | Exports a download link using the file name; cannot be combined with `MultipleFile`. |

Files are stored through the registered `IFileStorage` implementation. The built-in provider is disk-based; the application must be able to write to the configured destination. Upload extension checks complement, rather than replace, the host's request-size limit and any security controls required by the application. See [file storage](../developer-guide/file-storage.md) for provider registration and storage conventions.

## Field property reference

`FormElementField` extends `ElementField`. Database settings and editor settings serve different purposes:

| Property | Usage |
| --- | --- |
| `Name`, `DataType`, `Size` | Match the database column and its supported value. |
| `IsPk`, `AutoNum` | Identify record keys and generated values. |
| `IsRequired` | Reject missing required values. |
| `Component` | Select a `FormComponent`; default `Text`. |
| `DefaultValue`, `TriggerExpression` | Initial or recalculated values. |
| `VisibleExpression`, `EnableExpression` | Default `val:1`; return a boolean-compatible result. |
| `AutoPostBack` | Reload the form when the value changes; default `false`. |
| `PanelId`, `LineGroup`, `CssClass` | Panel, row and responsive field layout. |
| `DataItem`, `DataFile` | Lookup/list and file-specific configuration. |
| `Export` | Include the field in export; default `true`. |
| `EncodeHtml` | Encode displayed content; default `true`. |
| `GridWidth`, `GridAlignment`, `GridRenderingTemplate` | Grid cell presentation. |

Field validation skips editors whose visibility or enablement expression evaluates to false. Use a dictionary rule or before-event for constraints that must apply regardless of editor state.

Rendering attributes include `placeholder`, `rows`, `min`, `max`, `step`, `is-switch` and `culture-info`; support depends on the selected component.

`ReadOnlyExpression` is a C#-only property (`JsonIgnore`) documented to preserve the submitted value while disabling editing. Do not expect it to survive dictionary JSON export/import.
