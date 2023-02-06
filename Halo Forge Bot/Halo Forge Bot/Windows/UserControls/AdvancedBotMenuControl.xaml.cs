using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Halo_Forge_Bot.Core;
using Halo_Forge_Bot.Utilities;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
using WindowsInput.Native;

namespace Halo_Forge_Bot.Windows.UserControls;

public partial class AdvancedBotMenuControl : UserControl
{
    public AdvancedBotMenuControl()
    {
        InitializeComponent();
    }

    private async void CollectItemData_OnClick(object sender, RoutedEventArgs e)
    {
        MemoryHelper.Memory.OpenProcess(ForgeUI.SetHaloProcess().Id);
        foreach (var category in ForgeObjectBrowser.Categories)
        {
            await NavigationHelper.NavigateToCategory(category.Value);
            foreach (var folder in category.Value.CategoryFolders)
            {
                await NavigationHelper.NavigateToFolder(folder.Value);

                // Loop downwards using global hover to find each items name and xpositon value

                while (MemoryHelper.GetMenusVisible() != 0)
                {
                    Input.PressKey(VirtualKeyCode.RETURN);
                    await Task.Delay(25);
                }

                //Item is spawned

                //Get the Item name from the transform UI. Need to find a pointer.

                //save the name to the a item db

                await NavigationHelper.MoveToTab(NavigationHelper.ContentBrowserTabs.ObjectProperties);

                await NavigationHelper.NavigateVertical(1);

                int expectedHover = 1;
                bool loop = true;
                while (loop)
                {
                   // var forward = MemoryHelper.geti
                    Input.PressKey(VirtualKeyCode.VK_A);
                    
                }
                
                
            }
        }
    }
}