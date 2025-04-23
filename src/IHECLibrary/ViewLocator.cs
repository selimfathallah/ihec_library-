using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using IHECLibrary.ViewModels;

namespace IHECLibrary;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? param)
    {
        if (param is null)
            return null;
        
        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        // If Type.GetType fails, try using the assembly-qualified approach
        if (type == null)
        {
            // Get the assembly where views are defined (same as the executing assembly)
            var assembly = typeof(ViewLocator).Assembly;
            type = assembly.GetType(name);
        }

        if (type != null)
        {
            try
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            catch (Exception ex)
            {
                return new TextBlock { Text = $"Error creating view: {ex.Message}" };
            }
        }
        
        return new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
