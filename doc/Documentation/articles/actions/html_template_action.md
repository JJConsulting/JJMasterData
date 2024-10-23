# HTML Template Action
This type of action is used to render an HTML template based on SQL queries.

## Data Source
You can return multiple SQL queries in your Data Source.
```sql
SELECT 123 as OrderId, 'JJ Consulting' as CustomerName

SELECT 'Product 1' as Name, 5 As Price
UNION
SELECT 'Product 2' AS Name, 10 AS Price
```
MasterData values can be passed between curly braces `{USERID}` and are transformed into SQL parameters like `@USERID`.

## HTML Template
Here we use a Liquid template rendered by [Fluid](https://github.com/sebastienros/fluid). To access values, use the `{{ DataSource }}` variable.
The first index represents the query, the second index represents the row, and the property represents the equivalent column.

```html
<h1>HTML Template Example</h1>

<h2>Header</h2>

<p>Order Id is {{ DataSource[0][0].OrderId }}</p> 
<p>Customer name is {{ DataSource[0][0].CustomerName }}</p>

<h2>Items</h2>
<ul id="products">
  {% for product in DataSource[1] %}
    <li>
      <p>Name: {{product.Name}} </p>
	  <p>Price: {{product.Price}} <p>
    </li>
  {% endfor %}
</ul>
```

You can read more about the Liquid syntax [in this link](../html_templates.md).