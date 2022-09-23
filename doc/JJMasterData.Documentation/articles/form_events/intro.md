# Form Events

## What is a FormEvent?
FormEvents are events defined by the IFormEvent interface. These can be Python scripts or .NET assemblies.

The purpose of a class implementing IFormEvent, is to padronize the customizations and programmaticaly customize your dictionary rules without a custom View.

> [!TIP] 
> To use them, just add WithFormEvents() method on AddJJMasterDataWeb(). It will automatically add any class implementing IFormEvent to the services container.

Learn more about how to use them in the links below.
 - [.NET Assemblies](/articles/form_events/net.md) 
 - [Python](/articles/form_events/python.md)
