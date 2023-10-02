using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.UI.Components;

public class IconFactory
{
    public JJIcon Create()
    {
        return new JJIcon();
    }

    public JJIcon Create(IconType icon)
    {
        return new JJIcon(icon);
    }

    public JJIcon Create(IconType icon, string color) 
    {
        return new JJIcon(icon, color);
    }

    public JJIcon Create(IconType icon, string color, string title) 
    {
        return new JJIcon(icon, color, title);
    }

    public JJIcon Create(string iconClass)
    {
        return new JJIcon(iconClass);
    }

    public JJIcon Create(string iconClass, string color)
    {
        return new JJIcon(iconClass, color);
    }

    public JJIcon Create(string iconClass, string color, string title)
    {
        return new JJIcon(iconClass, color, title);
    }
}