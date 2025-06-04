# What is the Relationship?
The Relationship will be done between two created tables.

# How does it work?
The Relationship can be created from 1xN.
When one item of a table could be related to several items from another table.
For example, in the making of two tables, one of them called Orders and another one called Products,
you can relate many ID’s from the table called Products to just one ID from the table called Orders.

You can also create 1x1 relationships.

# How to create a Relationship with JJMasterData?
After accessing the table through the edit button, the field “Relationship” will be visible on the title tab. Inside the field “Elements” there will be a button called “New”, which will be responsible for creating a Relationship between the tables, the item “ChildElement” must be selected according to the secondary table. The field “Layout”, beside “Elements”, allows the editing and setting of the tables as well as their display.

# Primary Key Column and Foreign Key Column:
These checkboxes will be used to reference which columns will be related, the space Columns shows what is the Relationship created. It is important to remember  that Foreign Key Column should be a filter in the ChildElement. The picture below will show how it will be displayed.

![](/media/RelationshipsExample.png)

The picture below will show how it will be inside the table created.

![](/media/ExampleRelationshipTable.png)
