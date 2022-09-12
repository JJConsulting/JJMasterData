
# JJ DataFile

JJDataFile is the component responsible to manipulate files at your FormView.

## How to configure a property folder path?

**FolderPath** it is the place where the uploads performed by the user will be stored.  
Within this folder will be added several other folders containing the primary key of each record and inside, the uploads performed by users are stored.  
Then the file path will be Folder path\{PK}\{file name}  
For example:  
If your table's primary key is the id field (autonumeric) and you set the path:

    > C:/Files/My Form/

 When the user uploads the test.txt file in this field, it will be in:

    C:/Files/My Form/1/test.txt
    
If the form has a composite primary key, they will be separated by an underscore following a registration order.

### Important points

1) To use the upload, the dictionary must have a primary key  
2) The primary key cannot contain characters / \ < > | * : ? "  
3) The {app.path} is the path to the root of the site where the application is running and this variable will be replaced at runtime
4) Be careful not to configure a path that already exists in another dictionary

The paths below are valid, but consider using {app.path} to your dictionary be OS agnostic.

`{app.path}/App_Data/MyFolder/`

`/home/MyFolder/`

`C:/MyFolder/`

## What types are blocked by default ?
For security reasons we don't allow you to attach some file types by default. To keep up with the ever - changing malicious software, we frequently update file types without permission.  
  
These are the blocked file types:  

    .ade, .adp, .apk, .appx, .appxbundle, .bat, .cab, .chm, .cmd, .com, .cpl, .dll, .dmg, .ex, .ex_, .exe, .hta, .ins, .isp, .iso, .jar, .js, .jse, .lib, .lnk, .mde, .msc, .msi, .msix, .msixbundle, .msp, .mst, .nsh, .pif, .ps1, .scr, .sct, .shb, .sys, .vb, .vbe, .vbs, .vxd, .wsc, .wsf, .wsh .jar .bin .cs

### What can you do
If you are sure the file is safe you can specify file types separated by comma or compress the file before uploading.

## How do I further increase the file size in the MaxFileSize property ?

Where we configure the maximum size in bytes allowed in the upload. The maximum limit of this configuration is determined by the configuration performed on the application server.  

### What can you do

If you want to increase the size of this field, you will need to change the setting **MaxRequestLength**
The default size is 4 MB for .NET Framework and 30MB for .NET Core.

## Why when enabling a MultipleFile property I cannot enable ExportAsLink?

### ExportAsLink

When enabling this property when exporting files to excel or pdf a link will be added redirecting the user to the page where the file was generated, passing as a parameter an action to download the file, you will be presented with a different page informing the file name and size that is being downloaded.  
When we enable the option to upload multiple files, the filename is sent to comma - separated database.  
When exporting the field to excel it is not allowed to place more than one link in the same cell, for this reason it is not allowed to export multiple linked files.

### What can you do
Unfortunately in this scenario you will have to create a field for each file.
