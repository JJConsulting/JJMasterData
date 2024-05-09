using System.Collections.Generic;

namespace JJMasterData.Core.UI.Components;

public class BreadcrumbFactory : IComponentFactory<JJBreadcrumb>
{
    public JJBreadcrumb Create()
    {
        return new JJBreadcrumb();
    }
    
    public JJBreadcrumb Create(List<BreadcrumbItem> items)
    {
        var breadcrumb = new JJBreadcrumb();
        breadcrumb.Items = items;
        
        return breadcrumb;
    }
    
}