using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;

namespace Halo_Forge_Bot.Windows;

public partial class ForgeSelectorControl : UserControl
{
    public event EventHandler OnClicked;

    public List<ForgeSelectorControl> SubpanelItems = new();
    public ForgeSelectorControl(string name)
    {
        InitializeComponent();
        this.TitleButton.Content = name;
    }


    public void SortSubpanels()
    {
        SubpanelItems.Sort();
        
    }


    private void TitleButton_OnClick(object sender, RoutedEventArgs e)
    {
        OnClicked.Invoke(this, EventArgs.Empty);
    }
}