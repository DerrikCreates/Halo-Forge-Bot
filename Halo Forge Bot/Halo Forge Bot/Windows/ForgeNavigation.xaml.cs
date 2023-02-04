using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Halo_Forge_Bot.Core;
using Halo_Forge_Bot.Utilities;
using InfiniteForgeConstants.Forge_UI.Object_Browser;

namespace Halo_Forge_Bot.Windows;

public partial class ForgeNavigation : Window
{
    public ForgeNavigation()
    {
        InitializeComponent();

        Bot.BuildUiLayout();

        foreach (var category in ForgeObjectBrowser.Categories)
        {
            var catSelector = new ForgeSelectorControl(category.Key);

            Categories.ListStackPanel.Children.Add(catSelector);

            catSelector.OnClicked += UpdateSubCategory;
            foreach (var subCategory in category.Value.CategoryFolders)
            {
                var subCatSelector = new ForgeSelectorControl(subCategory.Key);
                catSelector.SubpanelItems.Add(subCatSelector);
                subCatSelector.OnClicked += UpdateItems;

                foreach (var forgeUiObject in ForgeObjectBrowser.Categories[(string)catSelector.TitleButton.Content]
                             .CategoryFolders[(string)subCatSelector.TitleButton.Content].FolderObjects)
                {
                    var itemCatSelector = new ForgeSelectorControl(forgeUiObject.Key);
                    itemCatSelector.OnClicked += OnItemClick;
                    subCatSelector.SubpanelItems.Add(itemCatSelector);
                }
            }
        }


        InvalidateVisual();

        Categories.InvalidateVisual();
    }

    private void AddItemsToSelector(Panel parent, List<ForgeSelectorControl> itemsToAdd)
    {
        parent.Children.Clear();
        foreach (var newSubpanelItem in itemsToAdd)
        {
            parent.Children.Add(newSubpanelItem);
        }
    }

    public void UpdateItems(object sender, EventArgs e)
    {
        var selector = sender as ForgeSelectorControl;
        if (selector == null)
        {
            throw new NullReferenceException();
        }


        AddItemsToSelector(Items.ListStackPanel, selector.SubpanelItems);
    }


    public async void OnItemClick(object sender, EventArgs e)
    {
        var selector = sender as ForgeSelectorControl;
        if (selector == null)
        {
            throw new NullReferenceException();
        }

        var item = selector.TitleButton.Content;
        //new Error($"Clicked on item: {selector.TitleButton.Content}").Show();
        ForgeObjectBrowser.FindItem((string)item, out var forgeUiObject);

        MemoryHelper.Memory.OpenProcess(ForgeUI.SetHaloProcess().Id);
            // await NavigationHelper.NavigateToItem(forgeUiObject);

        await NavigationHelper.SpawnItem(forgeUiObject);
    }

    public void UpdateSubCategory(object sender, EventArgs e)
    {
        var selector = sender as ForgeSelectorControl;
        if (selector == null)
        {
            throw new NullReferenceException();
        }

        AddItemsToSelector(SubCategories.ListStackPanel, selector.SubpanelItems);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
    }
}