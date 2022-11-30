using System;
using System.Threading.Tasks;
using InfiniteForgeConstants.Forge_UI;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
using Memory;
using WindowsInput.Native;

namespace Halo_Forge_Bot.Utilities;

public static class NavigationHelper
{
    private static int _travelSleep = 10;

    /// <summary>
    /// Holds all data relating to the current UI navigation state
    /// </summary>
    private class NavigationState
    {
        public int CurrentTabSelection = -1;
        public _ObjectBrowserState ObjectBrowserState = new _ObjectBrowserState();
        public _ObjectPropertiesState ObjectPropertiesState = new _ObjectPropertiesState();

        /// <summary>
        /// Holds all data relating to the "Object Browser" UI navigation state
        /// </summary>
        public class _ObjectBrowserState
        {
            public ForgeUICategory? CurrentCategory = null;
            public ForgeUIFolder? CurrentFolder = null;
            public int CurrentVerticalSelection = 0;
            
            public enum ObjectBrowserDepth
            {
                None = 0,
                Category = 1,
                Folder = 2,
                Items = 3
            }

            public ObjectBrowserDepth GetObjectBrowserDepth()
            {
                if (CurrentFolder != null) return ObjectBrowserDepth.Items;
                if (CurrentCategory != null) return ObjectBrowserDepth.Folder;
                if (CurrentCategory == null && CurrentVerticalSelection != 0) return ObjectBrowserDepth.Category;
                return ObjectBrowserDepth.None;
            }
        }

        /// <summary>
        /// Holds all data relating to the "Object Properties" UI navigation state
        /// </summary>
        public class _ObjectPropertiesState
        {
            public int CurrentVerticalSelection = 0;
        }

        /// <summary>
        /// Updates the navigation state based on what tab is currently selected
        /// </summary>
        /// <param name="index"> The vertical position to set to </param>
        /// <exception cref="Exception"> Invalid tab </exception>
        public void UpdateVerticalState(int index)
        {
            switch (CurrentTabSelection)
            {
                case 0:
                    ObjectBrowserState.CurrentVerticalSelection = index;
                    break;
                case 1:
                    ObjectPropertiesState.CurrentVerticalSelection = index;
                    break;
                default:
                    throw new Exception("Trying to update state on a tab that hasn't been setup yet.");
            }
        }

        /// <summary>
        /// Grabs the current vertical state of the UI
        /// </summary>
        public int GetVerticalState()
        {
            return CurrentTabSelection switch
            {
                0 => ObjectBrowserState.CurrentVerticalSelection,
                1 => ObjectPropertiesState.CurrentVerticalSelection,
                _ => -1
            };
        }
    }

    private static NavigationState _navigationState = new NavigationState();

    private static int ContentBrowserTabsCount = Enum.GetNames(typeof(ContentBrowserTabs)).Length;
    public enum ContentBrowserTabs
    {
        ObjectBrowser = 0,
        ObjectProperties = 1,
        Folders = 2,
        MapOptions = 3,
        ToolSettings = 4
    }

    /// <summary>
    /// Reset the current state variables (dev tool)
    /// </summary>
    public static void ResetNavigationState()
    {
        _navigationState = new NavigationState();
    }

    /// <summary>
    /// Opens the UI onto any tab requested
    /// </summary>
    /// <param name="openToTab"> The tab to open up </param>
    public static async Task OpenUI(ContentBrowserTabs? openToTab = null)
    {
        //Ensure the UI menu is open
        while (MemoryHelper.GetMenusVisible() != 1)
        {
            await Input.KeyPress(VirtualKeyCode.VK_R, 250, 250);
        }

        await Task.Delay(100);

        if (openToTab != null) 
            await MoveToTab((ContentBrowserTabs)openToTab);
    }

    /// <summary>
    /// Tries to open up the edit menu on the requested UI selection
    /// </summary>
    /// <param name="index"> The UI element to try to open </param>
    public static async Task OpenEditUI(int index)
    {
        if (MemoryHelper.GetEditMenuState() == 1) throw new Exception("Trying to open edit UI when it is already open");
        
        await NavigateVertical(index);
        while (MemoryHelper.GetEditMenuState() == 0)
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(200);
        }
    }

    /// <summary>
    /// Tries to close the edit menu if it is open
    /// </summary>
    public static async Task CloseEditUI()
    {
        while (MemoryHelper.GetEditMenuState() != 0)
        {
            Input.Simulate.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            await Task.Delay(200);
        }
    }

    /// <summary>
    /// Navigates up and down the UI to specific indexes
    /// </summary>
    /// <param name="index"> Index to navigate to </param>
    public static async Task NavigateVertical(int index)
    {
        await OpenUI();
        if (MemoryHelper.GetGlobalHover() == index) return;

        while (MemoryHelper.GetGlobalHover() != index)
        {
            if (Math.Abs(MemoryHelper.GetGlobalHover() - index) > 1)
            {
                if (index == 0)
                {
                    MemoryHelper.SetGlobalHover(index + 1);
                    await Input.KeyPress(VirtualKeyCode.VK_W, _travelSleep, 50);
                }
                else { 
                    MemoryHelper.SetGlobalHover(index - 1);
                    await Input.KeyPress(VirtualKeyCode.VK_S, _travelSleep, 50);
                }
                    
                _navigationState.UpdateVerticalState(MemoryHelper.GetGlobalHover());
            }
            else
            {
                await Input.KeyPress(index > MemoryHelper.GetGlobalHover()
                    ? VirtualKeyCode.VK_S
                    : VirtualKeyCode.VK_W, _travelSleep);

                _navigationState.UpdateVerticalState(MemoryHelper.GetGlobalHover());
            }
        }
        
        _navigationState.UpdateVerticalState(MemoryHelper.GetGlobalHover());
    }

    /// <summary>
    /// Updates vertical state to be current hovered value, can be different depending on previous runs
    /// </summary>
    private static async Task EnsureVerticalState()
    {
        _navigationState.UpdateVerticalState(MemoryHelper.GetGlobalHover());
    }
    
    /// <summary>
    /// reset the cursor to the top of the current menu (in most cases the object browser)
    /// </summary>
    public static async Task ReturnToTop()
    {
        await NavigateVertical(0);
    }

    /// <summary>
    /// Navigate the UI top tabs until at the desired index
    /// </summary>
    /// <param name="tabIndex"> Tab to navigate to </param>
    public static async Task MoveToTab(ContentBrowserTabs tabIndex)
    {
        await OpenUI();
        
        while (MemoryHelper.GetTopBrowserHover() != (int)tabIndex)
        {
            var leftDistance = (int)tabIndex > MemoryHelper.GetTopBrowserHover()
                ? MemoryHelper.GetTopBrowserHover() + (ContentBrowserTabsCount - (int)tabIndex)
                : MemoryHelper.GetTopBrowserHover() - (int)tabIndex;

            var rightDistance = (int)tabIndex < MemoryHelper.GetTopBrowserHover()
                ? ContentBrowserTabsCount - MemoryHelper.GetTopBrowserHover() + (int)tabIndex
                : (int)tabIndex - MemoryHelper.GetTopBrowserHover();
            
            await Input.KeyPress(leftDistance < rightDistance ? VirtualKeyCode.VK_Q : VirtualKeyCode.VK_E, _travelSleep);
            _navigationState.CurrentTabSelection = MemoryHelper.GetTopBrowserHover();
        }
        
        _navigationState.CurrentTabSelection = MemoryHelper.GetTopBrowserHover();
    }
    
    /// <summary>
    /// Navigate to a specific category in the "Object Browser"
    /// </summary>
    /// <param name="category"> The ForgeUICategory to go to </param>
    public static async Task NavigateToCategory(ForgeUICategory category)
    {
        await MoveToTab(ContentBrowserTabs.ObjectBrowser);
        //Home to "Object Browser" category level if needed
        switch (_navigationState.ObjectBrowserState.GetObjectBrowserDepth())
        {
            case NavigationState._ObjectBrowserState.ObjectBrowserDepth.None:
            case NavigationState._ObjectBrowserState.ObjectBrowserDepth.Category:
                if (_navigationState.ObjectBrowserState.CurrentCategory == category) return;
                break;
            
            //Recursively call this function until at the correct category
            default:
                //Navigate to the folder level (arbitrary category open and hovered over a folder)
                await HomeObjectBrowserToCategoryLevel();
                
                //Recursively call NavigateToCategory with the correct initial state required
                await NavigateToCategory(category);
                return;
        }
        
        await NavigateVertical(category.CategoryOrder - 1);
        await Input.KeyPress(VirtualKeyCode.RETURN, 200, 200);
        
        //Update State
        _navigationState.ObjectBrowserState.CurrentCategory = category;
    }

    /// <summary>
    /// Navigate to a specific folder in the "Object Browser"
    /// </summary>
    /// <param name="folder"> The ForgeUIFolder to go to </param>
    public static async Task NavigateToFolder(ForgeUIFolder folder)
    {
        await MoveToTab(ContentBrowserTabs.ObjectBrowser);
        //Home to "Object Browser" folder level if needed
        switch (_navigationState.ObjectBrowserState.GetObjectBrowserDepth())
        {
            case NavigationState._ObjectBrowserState.ObjectBrowserDepth.None:
            case NavigationState._ObjectBrowserState.ObjectBrowserDepth.Category:
                await NavigateToCategory(folder.ParentCategory);
                break;
            
            case NavigationState._ObjectBrowserState.ObjectBrowserDepth.Folder:
                if (folder.ParentCategory != _navigationState.ObjectBrowserState.CurrentCategory)
                    await NavigateToCategory(folder.ParentCategory);
                break;
            
            case NavigationState._ObjectBrowserState.ObjectBrowserDepth.Items:
                if (folder == _navigationState.ObjectBrowserState.CurrentFolder) return;
                //Navigate to the folder level (arbitrary category open and hovered over a folder)
                await HomeObjectBrowserToFolderLevel();
                
                //Recursively call NavigateToFolder with the correct initial state required
                await NavigateToFolder(folder);
                return;
        }
        
        await NavigateVertical(folder.ParentCategory.CategoryOrder + folder.FolderOffset - 1);
        await Input.KeyPress(VirtualKeyCode.RETURN, 200, 200);
        
        //Update State
        _navigationState.ObjectBrowserState.CurrentFolder = folder;
        await EnsureVerticalState();
    }

    /// <summary>
    /// Navigate to a specific item in the "Object Browser"
    /// </summary>
    /// <param name="item"> The ForgeUIObject to go to </param>
    public static async Task NavigateToItem(ForgeUIObject item)
    {
        await MoveToTab(ContentBrowserTabs.ObjectBrowser);
        await NavigateToFolder(item.ParentFolder);
        await NavigateVertical(item.ObjectOrder - 1);
    }

    /// <summary>
    /// Spawn a forge item into the world
    /// </summary>
    /// <param name="item"></param>
    public static async Task SpawnItem(ForgeUIObject item)
    {
        await NavigateToItem(item);
        await Input.KeyPress(VirtualKeyCode.RETURN, 500, 500);

        //_navigationState.CurrentTabSelection = -1;
    }
    
    /// <summary>
    /// Home to the very top of the "Object Browser" ("recents" category)
    /// </summary>
    public static async Task HomeObjectBrowserToZero()
    {
        await MoveToTab(ContentBrowserTabs.ObjectBrowser);
        await HomeObjectBrowserToCategoryLevel();
        await NavigateVertical(0);
    }

    /// <summary>
    /// Home to the category level with the current category closed if inside one
    /// </summary>
    public static async Task HomeObjectBrowserToCategoryLevel()
    {
        await MoveToTab(ContentBrowserTabs.ObjectBrowser);
        await HomeObjectBrowserToFolderLevel();

        //If navigation state is within a category but not inside a folder, proceed as necessary 
        if (_navigationState.ObjectBrowserState.GetObjectBrowserDepth() ==
            NavigationState._ObjectBrowserState.ObjectBrowserDepth.Folder)
        {
            //Close current category
            await NavigateVertical(_navigationState.ObjectBrowserState.CurrentCategory.CategoryOrder - 1);
            await Input.KeyPress(VirtualKeyCode.RETURN, 200, 200);

            //Update navigation state
            _navigationState.ObjectBrowserState.CurrentCategory = null;
        }
    }
    
    /// <summary>
    /// Home to the folder level with category open if at a lower level
    /// </summary>
    public static async Task HomeObjectBrowserToFolderLevel()
    {
        await MoveToTab(ContentBrowserTabs.ObjectBrowser);
        //If inside a folder escape it
        if (_navigationState.ObjectBrowserState.GetObjectBrowserDepth() ==
            NavigationState._ObjectBrowserState.ObjectBrowserDepth.Items)
        {
            await Input.KeyPress(VirtualKeyCode.BACK, 100, 75);
            
            //Update navigation state
            await EnsureVerticalState();
            _navigationState.ObjectBrowserState.CurrentFolder = null;
        }
        
    }
}