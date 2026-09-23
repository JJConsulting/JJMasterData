# TemplateAction

`HtmlTemplateAction` renders Liquid/HTML content as an action. Its `SqlCommand` runs with the current record's primary-key values, and the query result is exposed to the template as the `DataSource` collection.

## Quick Liquid reference

Print values with `{{ ... }}` and use `{% ... %}` for conditions or loops:

```liquid
<h5>Order history</h5>
<table class="table table-sm">
  <tbody>
  {% for order in DataSource %}
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

When the action is opened for a customer whose primary key is `Id`, `@Id` is populated from the current record and the resulting rows become `DataSource`.

Use standard Liquid filters with `|`, and call JJMasterData helpers as functions:

```liquid
{% for row in DataSource %}
  {% assign label = row.Status | capitalize %}
  <span>{{ row.OrderNumber }} — {{ label }}</span>
{% endfor %}
```

For optional values, provide a fallback explicitly:

```liquid
{% if DataSource == empty %}
  <span>No records found</span>
{% else %}
  <span>{{ DataSource.size }} records found</span>
{% endif %}
```

The available helpers and their purposes are listed in [Grid templates](grid-templates.md). The same template engine is used there, but a `TemplateAction` receives `DataSource` rows from its configured query rather than the fields of a single grid cell.

When the action depends on optional values, guard missing or empty values with `isNullOrEmpty` or `isNullOrWhiteSpace`. For links to files, `getFileUrl` requires the corresponding `DataFile` and record values in the rendering context.

The action is listed with the other action types in [Actions](../concepts/actions.md).
