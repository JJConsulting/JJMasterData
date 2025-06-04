using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace JJMasterData.Core.UI.Components;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class BreadcrumbFactory : IComponentFactory<JJBreadcrumb>
{
    public JJBreadcrumb Create()
    {
        return new JJBreadcrumb();
    }
    
    public JJBreadcrumb Create(IEnumerable<BreadcrumbItem> items)
    {
        var breadcrumb = new JJBreadcrumb();
        breadcrumb.Items.AddRange(items); 
        
        return breadcrumb;
    }
}