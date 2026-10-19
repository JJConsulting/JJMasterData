# Show a status guide with HTML templates

Add a **Ticket guide** toolbar action that opens a read-only legend with ticket statuses and response targets. The example uses two SQL result sets and entirely fictional data; no business tables are required.

## Add the action

Create a form for an existing dictionary (`SupportTicket` here) and add the action before calling `GetResultAsync()`. Recreate this registration on every request, including the action's callback.

```csharp
using JJMasterData.Core.DataDictionary.Models.Actions;

// formFactory is IFormElementComponentFactory<JJFormView>.
var form = await formFactory.CreateAsync("SupportTicket");
var guide = new HtmlTemplateAction
{
    Name = "ticket-guide",
    Text = "Ticket guide",
    ShowAsButton = true,
    SqlCommand = """
        SELECT Code, Label, Description
        FROM (VALUES
            (1, 'OPEN', 'Open', 'Waiting for an agent.'),
            (2, 'IN_PROGRESS', 'In progress', 'An agent is working on the ticket.'),
            (3, 'CLOSED', 'Closed', 'The solution has been confirmed.')
        ) AS statuses(SortOrder, Code, Label, Description)
        ORDER BY SortOrder;

        SELECT Label, TargetHours
        FROM (VALUES
            (1, 'Urgent', 4),
            (2, 'Normal', 24),
            (3, 'Low', 72)
        ) AS priorities(SortOrder, Label, TargetHours)
        ORDER BY SortOrder;
        """,
    HtmlTemplate = """
        <!doctype html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <style>
            body { font: 15px/1.5 system-ui, sans-serif; margin: 24px; color: #243044; }
            h1 { font-size: 22px; margin: 0 0 16px; }
            h2 { font-size: 17px; margin-top: 24px; }
            ul { list-style: none; padding: 0; }
            li { padding: 10px 0; border-bottom: 1px solid #dde2e8; }
            .badge { display: inline-block; padding: 2px 8px; border-radius: 4px;
                     background: #e8edf3; font-weight: 600; }
            .open { background: #e4efff; color: #164782; }
            .in_progress { background: #fff0cc; color: #684700; }
            .closed { background: #ddf3e7; color: #165439; }
            p { margin: 4px 0; }
            table { width: 100%; border-collapse: collapse; }
            th, td { padding: 8px; border-bottom: 1px solid #dde2e8; text-align: left; }
          </style>
        </head>
        <body>
          <h1>Ticket guide</h1>
          <h2>Statuses</h2>
          {% if DataSource[0].size > 0 %}
            <ul>
            {% for status in DataSource[0] %}
              <li>
                <span class="badge {{ status.Code | downcase }}">{{ status.Label }}</span>
                <p>{{ status.Description }}</p>
              </li>
            {% endfor %}
            </ul>
          {% else %}
            <p>No statuses configured.</p>
          {% endif %}

          <h2>First response targets</h2>
          {% if DataSource[1].size > 0 %}
            <table>
              <thead><tr><th>Priority</th><th>Hours</th></tr></thead>
              <tbody>
              {% for priority in DataSource[1] %}
                <tr><td>{{ priority.Label }}</td><td>{{ priority.TargetHours }} h</td></tr>
              {% endfor %}
              </tbody>
            </table>
          {% else %}
            <p>No response targets configured.</p>
          {% endif %}
        </body>
        </html>
        """
};
form.GridView.ToolbarActions.Add(guide);
```

Use the controller/view result handling from [Routes and embedding](../developer-guide/routes-and-embedding.md). Return component `IActionResult` responses directly so opening the action returns its content. If configuring through the dictionary editor instead, create an `HtmlTemplateAction` in **Grid Toolbar Actions**, then paste the two string contents into **Data Source** and **HTML Template**.

## How the data reaches the template

| SQL result | Liquid value | Example |
| --- | --- | --- |
| First `SELECT` | `DataSource[0]` | `status.Label` is `Open`. |
| Second `SELECT` | `DataSource[1]` | `priority.TargetHours` is `4`. |

`DataSource` is an array of result sets, not a flat list of rows. Even one `SELECT` must be read through `DataSource[0]`. Keep the result-set order and column aliases consistent with the template.

The action renders inside an iframe and adds a print button. Its HTML does not inherit the host's Bootstrap stylesheet, which is why this example includes its own small stylesheet. Liquid output is HTML-encoded by the renderer; leave ordinary database text as `{{ value }}` rather than marking it as raw HTML.

## Use application data

Replace the literal SQL rows with your own read queries, retaining the same aliases. The command runs on the dictionary's `ConnectionId`.

For a **row action**, add the action to `form.GridView.TableActions` and filter with a primary-key placeholder such as `WHERE TicketId = {Id}` when the dictionary's key is named `Id`. The placeholder becomes a SQL parameter. A grid-toolbar action like this legend has no selected record key; it does not automatically pass filters, selected rows or all grid columns into the query.

The guide only displays information. If response targets affect business validation, enforce them in the save operation too.

Open **Ticket guide** to check the three status badges and the 4/24/72-hour targets. Then return zero rows from either query while preserving its columns: that section should show its empty message. See [Template actions](../developer-guide/template-action.md) for the reusable contract.
