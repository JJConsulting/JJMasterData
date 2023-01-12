# What is a Data Dictionary
Data Dictionary is a metadata to represent the structure of a table and 
its visualization in a CRUD (registration screen where the user can Create, Read, Update and Delete records).

Here you can manage and configure the functionalities and characteristics of this CRUD at runtime. 
Metadata is parsed into JSON by default and stored in a table in the database, 
but it can be stored in files or anywhere else, 
for that see the system [configurations](../configurations.md).

# Creating a Data Dictionary 

You can start through the link /pt-br/DataDictionary.
The New icon will allow you to create your metadata and generate scripts for your tables.

With JJMasterData you will be able to import or create your table and organize it into dictionaries and later displayed in metadata.

- <b>Adding a new Data Dictionary<br></b>
  To create the data structure directly by JJMasterData it will be necessary to leave the field “import field” unchecked, 
  insert the name of the table to be created and click Next.


- <b>Importing a Data Dictionary from an existing table<br></b>
  To create a data structure with JJMasterData using an existing table in your database,
  it is necessary that the “Import field” field is checked and the database connection user has permission to read the table structure.
  The Table Name field will be filled in with the name of the existing table and click Next.

After you will access the Entity window and you will find the following items:

## Entity
- **Dictionary name**: It will be the name that will represent your metadata.
- **Table Name**: Name of the table where your records will be stored.
- **Get procedure** Name: Name of the procedure that will read the records. 
  You can view the procedure created by clicking on *more*, *Get Script* and then *Get Procedure*.
- **Set Procedure Name**: Name of the procedure that will write the records.
  You can view the procedure created by clicking on *more*, *Get Script* and then *Set Procedure*.
- **Title**: Here you will fill in the title that will be displayed inside the Form. 
  It will be possible to enable or disable the visualization of the title through the internal settings.
- **Subtitle**: Here you will fill in the subtitle that will be displayed inside the Form. 
  Just like the title, the subtitle can also be enabled or disabled.
- **Info**: Free text information for internal use that will be shown to the developer.

## Fields
The next step is to access the Fields area, 
where you can fill in and format the form fields.

> [!INFO]
> After completing all required items, you must perform another action before the table is definitely created.
> you will access the More option on the right side of the menu and click on Get Script,
> in this way the scripts will be converted to an SQL script and will be displayed to you.
> After displaying the scripts, you will have the option of executing the stored procedure, 
> if there are no procedures to be executed, just click Run All and save the page by clicking at the bottom. 
> Okay, now your table is created. 
> By clicking on the Exit button, at list of Data Dictionaries, when locating your table, click on the icon on the right next to the edit button,
> so you will see the Preview button, it will let you see what the final display will look like.

The Fields are separated in following areas:

### General
It's required to fill in the following fields: 
FieldName, Filter, DataBehavior, Data Type, Size, Required, Pk, Identify.

- **FieldName**: This name is used to refer a field and will be related within the database.
- **Label**: Label that will be displayed in a form
- **Default Value**: An expression that will return a default value if the value inside the database is null.
- **Filter**: Field that indicates a type of filter that will be executed in the get procedure
- **DataBehavior**: Data behavior
  - Real: Will be used in Get and Set;
  - Virtual: It will be used only in Set;
  - ViewOnly: Will be used only in Get.
- **DataType**: Type of data that will be received from the table.
- **Size**: Number of characters and when used as Varchar it will be the size to be reserved within the database and the max length in the form. 
- **Required**: It will define the required filling of the field.
- **PK**: Define whether or not it's the primary key, it's important that you fill in one of the items in your table as the primary key.
- **Identity**: If the field will be auto-incremented, for example, if it will automatically return a field from within the database.
- **Help Description**: Message to be displayed to the user in a tooltip to help him.

### Component 
- **Component**: Component used to render a specific field within the grid and form.
- **AutoPostBack**: true if an automatic postback occurs when the component control loses focus; otherwise, false. 
  The default is false.
- **Placeholder**: Will show a note for the component when there is no data included.

To upload see [Configuring Data Upload](data_file.md)

### Advanced
Advanced Settings in a component

#### Expressions

!include[expressions](../expressions.md)

#### CssClass
It's possible to insert several Css classes and create responsive layouts using Bootstrap's grid system. 
For example, two fields with class `col-sm-6` in the form will be on the same line.

#### Line Group
It's the field line within the grid system. Represents the bootstrap row class.

#### Export
You can define whether or not the field will be exported.

#### Validade Request
On .NET Framework 4.8 systems, the field will validate dangerous values, like Html tags and SQL commands.

## **Panels**
Allows you to separate the dictionary fields into panels. 
But only for add, edit and view actions.

### General
- **Layout**: It's the way the panel will be displayed.
- **Expanded By Default**: This option will define if the created panel will be started maximized by minimized by default.
- **Available Field**: Will be the items that will not be displayed as panels.
- **Selected Fields**: Will be the items that will be displayed as panels.

## **Indexes and Relationships**
Both items will be used to generate information from your metadata. 
You can add this information by clicking the New button.

## Options
Layout settings that can be performed on the form at runtime.<br>
!include[expressions](ui_options.md)

## Actions
Within the Actions tab you can configure the display of icons to edit your table. 
The Actions field is divided into two, Grid and Toolbar. 

### Grid
- View: This icon will be displayed next to the items in your table, you can find it when using the preview of your table. By clicking on the *View* button, the chosen line will be displayed in detail.
- Edit: This icon will be displayed next to the items in your table, you can find it when using the preview of your table. By clicking on the *Edit* button, it will be possible to change information previously registered in your data table.
- Delete: This icon will be displayed next to the items in your table, you can find it when using the preview of your table. This option will delete the desired line of information.

### Toolbar
- Insert: Allows you to add new data to your table.
- Config: Allows you to access layout settings for the user, for example, records per page and show table border.
- Export: Allows you to select the export of the data present in the table. You will be able to import only the data visible on the screen or all the data in your table, exporting to pdf, csv, excel and txt files is allowed. If the export is done in PDF, it is necessary to enable the pdf plugin (LINK PARA DOC PLUGIN PDF).
- Import: This item will be displayed as an upload for the user, allowing the import of data to your table through txt, csv and log files. You can click the Help button to view the file upload formatting.
- Refresh: The refresh option will update your data table, in case there is any change to be displayed.
- Legend: The legend is used to help describe the data within your table, for example, you can create the legend for a gender column, where you can assign the description Woman, Man and then associate with colors and icons . You can see a complete description of how to create your legend via the link (LINK TO DATA_ITEM_LEG).
- Sort: Allows the user to order the search by items, for example, in alphabetical order.
- Filter: Shows all filter options for searching items within the table.
- Log: Records and displays the actions performed within the table, including adding, editing and deleting.

## **API**
Within this tab it will be possible to edit each verb responsible for http permissions within the REST API.
- ApplyUseridOn: Name of the field where the user ID filter will be applied.
- JsonFormat: This option will define and modify the formatting of the Json file. When using the default option, the default chosen by the user will be defined, however the LowerCase option will format the file for lowercase letters.
- Sync: This option enables or disables the REST API dictionary.
