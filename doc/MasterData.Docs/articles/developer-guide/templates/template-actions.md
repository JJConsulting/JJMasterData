# TemplateAction

`HtmlTemplateAction` renders Liquid/HTML content as an action. Its `SqlCommand` runs with the current record's primary-key values, and the query result is exposed to the template as the `DataSource` array of result sets. Use `DataSource[0]` for rows from the first SQL result, `DataSource[1]` for the second, and so on.

## Quick Liquid reference

Print values with `{{ ... }}` and use `{% ... %}` for conditions or loops:

```liquid
<h5>Order history</h5>
<table class="table table-sm">
  <tbody>
  {% for order in DataSource[0] %}
    <tr>
      <td>{{ order.OrderNumber }}</td>
      <td>{{ formatDate(order.OrderDate, "yyyy-MM-dd") }}</td>
      <td>{{ order.Status }}</td>
    </tr>
  {% endfor %}
  </tbody>
</table>
```

For this example, configure the action's `SqlCommand` with fields matching the template:

```sql
SELECT OrderNumber, OrderDate, Status
FROM Orders
WHERE CustomerId = @Id
ORDER BY OrderDate DESC
```

When the action is opened for a customer whose primary key is `Id`, `@Id` is populated from the current record and the resulting rows become `DataSource[0]`.

Use standard Liquid filters with `|`, and call JJMasterData helpers as functions:

```liquid
{% for row in DataSource[0] %}
  {% assign label = row.Status | capitalize %}
  <span>{{ row.OrderNumber }} — {{ label }}</span>
{% endfor %}
```

For optional values, provide a fallback explicitly:

```liquid
{% if DataSource[0].size == 0 %}
  <span>No records found</span>
{% else %}
  <span>{{ DataSource[0].size }} records found</span>
{% endif %}
```

The available helpers and their purposes are listed in [Grid templates](grid-templates.md). The same template engine is used there, but a `TemplateAction` receives `DataSource` rows from its configured query rather than the fields of a single grid cell.

When the action depends on optional values, guard missing or empty values with `isNullOrEmpty` or `isNullOrWhiteSpace`. For links to files, `getFileUrl` requires the corresponding `DataFile` and record values in the rendering context.

The action is listed with the other action types in [Actions](../../concepts/actions.md).

## Complete example and rendering context

[Show a status guide with HTML templates](../../cookbook/html-templates.md) contains a complete action with two SQL results, fictional data, empty-state handling and a self-contained HTML document.

The query runs on the dictionary's connection. Record primary-key values are available as parameters (and through `{Id}`-style placeholders); a toolbar action without a record does not supply them. The template context contains the query results under `DataSource`, rather than the grid row's fields at the top level.

The rendered document is placed in an iframe with a print button. Include any required CSS in the template: it does not inherit the parent page's styles. Dynamic text is HTML-encoded by the renderer. File helpers such as `getFileUrl` return an empty value here because this action does not supply the field-specific `DataFile` context.
