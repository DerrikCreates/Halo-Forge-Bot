using System;
using System.Windows;

namespace Halo_Forge_Bot;

public partial class Error : Window
{
    public Error(string errorMessage)
    {
        
        InitializeComponent();
        ErrorTextBox.Text = errorMessage;
    }

    public Error(Exception exception)
    {
        
        InitializeComponent();
        ErrorTextBox.Text = exception.Message + Environment.NewLine + exception.StackTrace;
    }
}