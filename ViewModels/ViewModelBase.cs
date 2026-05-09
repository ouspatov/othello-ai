using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OthelloAI.ViewModels;

// This is all needed to not render again and again whole window, instead only the concrete changed value
public class ViewModelBase : INotifyPropertyChanged 
{
    public event PropertyChangedEventHandler? PropertyChanged; // Event controller

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));// Invoke(sender, name of the changed variable)
    }  
}