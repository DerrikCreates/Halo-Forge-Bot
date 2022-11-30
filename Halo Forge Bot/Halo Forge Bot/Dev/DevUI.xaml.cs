using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Halo_Forge_Bot.GameUI;
using Halo_Forge_Bot.Utilities;

namespace Halo_Forge_Bot.Windows;

public partial class DevUI : Window
{
    public DevUI()
    {
        InitializeComponent();
    }


    private async void DownloadMvarButton_OnClick(object sender, RoutedEventArgs e)
    {
        await Utils.DownloadMvar(FileShareLinkTextBox.Text, "");
    }


    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
       // var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
       Error errorWindow = new Error();
       errorWindow.ErrorTextBox.Text = "TESTSETSTSTESTT THIS IS A TEST";
       errorWindow.Show();

       // await Task.Run(Bot.DevTesting);
    }


    private void UpdateMem_OnClick(object sender, RoutedEventArgs e)
    {
        MemoryTestUI.Text = MemoryHelper.GetEditBoxText();
    }

    bool keepUpdating = true;


    private Task UpdateMemoryUI(string address, string length)
    {
        if (int.TryParse(length, out int result))
        {
            while (keepUpdating)
            {
                var data = MemoryHelper.Memory.ReadBytes(address, result);
                Dispatcher.Invoke(() => { return DebugMemoryLabel.Text = Convert.ToHexString(data); });

                Thread.Sleep(10);
            }
        }

        return Task.CompletedTask;
    }

    private void OnupdateMemoryUI(string s)
    {
        DebugMemoryLabel.Text = s;
    }

    private void ReadAddressButton_OnClick(object sender, RoutedEventArgs e)
    {
        var address = DebugMemoryAddressTextBox.Text;
        var length = DebugMemoryAddressLengthTextBox.Text;
        Task.Run(() => UpdateMemoryUI(address, length));
    }

    private void AttachToHalo_OnClick(object sender, RoutedEventArgs e)
    {
        MemoryHelper.Memory.OpenProcess(ForgeUI.HaloProcess.Id);
    }
    
    private void GetHaloObjects_OnClick(object sender, RoutedEventArgs e)
    {
        Dev.GetAllObjectTypeData();
    }
    
    private void GetHaloObjects2_OnClick(object sender, RoutedEventArgs e)
    {
        Dev.GetAllObjectTypeData2();
    }
    
    private void ResetState_OnClick(object sender, RoutedEventArgs e)
    {
        NavigationHelper.ResetNavigationState();
    }
}