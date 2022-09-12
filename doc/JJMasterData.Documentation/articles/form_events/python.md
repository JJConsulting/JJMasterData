# Python Form Events

## What are them?

They're Python Scripts that implements the IFormEvent interface. They run on CLR via IronPython.
You can use any CLR type in your scripts. Check more [here](../miscellaneous/python.html) how to use CLR in Python.

## Example

Create a python file with your data dictionary name and implement a class with the same name. Add jjmasterdata.py to enable autocomplete features.
To deploy, paste the file on the bin folder of your site or specify the path on the services setup.

```py
from JJMasterData.Core.FormEvents.Abstractions import BaseFormEvent

class DataDictionaryName(BaseFormEvent):
	def OnInstanceCreated(self, sender):
		sender.FormElement.Title = "Hello from Python!"
```

## How to inject them?

Install JJMasterData.Python and them simply inject in your application startup with the following code:

```csharp
	services.AddJJMasterDataWeb().WithPythonEngine()
```