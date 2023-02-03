using System.Windows;
using System.Windows.Controls;

namespace Halo_Forge_Bot.Windows.UserControls;

public partial class ListViewer : UserControl
{
    public static readonly DependencyProperty ViewerTitle = DependencyProperty.Register(nameof(Title), typeof(string),
        typeof(ListViewer), new PropertyMetadata(string.Empty));
    
    
    public string Title
    {
        get => (string)GetValue(ViewerTitle);
        set
        {
            TitleLabel.Content = value;
            SetValue(ViewerTitle,value);
        }
    }

    public ListViewer()
    {
        InitializeComponent();
    }
}