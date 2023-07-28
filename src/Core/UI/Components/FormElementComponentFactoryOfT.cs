using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.UI.Components;

public class FormElementComponentFactory<T> : IFormElementComponentFactory<T> where T : JJBaseView
{
    private IEnumerable<IFormElementComponentFactory> Factories { get; }

    public FormElementComponentFactory(IEnumerable<IFormElementComponentFactory> factories)
    {
        Factories = factories;
    }
    
    private IFormElementComponentFactory<TComponent> GetFactory<TComponent>() where TComponent : JJBaseView
    {
        return (IFormElementComponentFactory<TComponent>)Factories.First(f=>f is IFormElementComponentFactory<TComponent>);
    }
    
    public T Create(FormElement formElement)
    {
        var factory = GetFactory<T>();
         return factory.Create(formElement);
    }

    public async Task<T> CreateAsync(string elementName)
    {
        var factory = GetFactory<T>();
        return await factory.CreateAsync(elementName);
    }
}