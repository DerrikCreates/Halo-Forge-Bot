using System;
using System.Threading.Tasks;
using InfiniteForgeConstants.Forge_UI;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
using Memory;
using Serilog;
using WindowsInput.Native;

namespace Halo_Forge_Bot.Utilities;

public static class NavigationHelper
{
    private static int _travelInitialSleep = 10;
    private static int _travelAfterSleep = 25;

    private static int _travelTabInitialSleep = 10;
    private static int _travelTabAfterSleep = 50;

    private static int _openUIInitialSleep = 50;
    private static int _openUIAfterSleep = 300;

    private static int _closeUIInitialSleep = 50;
    private static int _closeUIAfterSleep = 300;

    private static int _enterUIInitialSleep = 50;
    private static int _enterUIAfterSleep = 300;

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

    private static readonly int ContentBrowserTabsCount = Enum.GetNames(typeof(ContentBrowserTabs)).Length;

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
    /// Used to ensure cold open function doesn't get called during cold open setup
    /// </summary>
    private static bool _isInColdOpen = false;

    /// <summary>
    /// Sets up the menu from a cold start to ensure the bot doesn't break
    /// </summary>
    private static async Task ColdStart()
    {
        if (_navigationState.CurrentTabSelection == -1 && !_isInColdOpen)
        {
            _isInColdOpen = true;
            await MoveToTab(ContentBrowserTabs.ObjectBrowser);

            //Escape any folders
            while (MemoryHelper.GetMenusVisible() != 0)
            {
                await Input.KeyPress(VirtualKeyCode.ESCAPE, _closeUIAfterSleep, _closeUIInitialSleep);
                await Input.HandlePause();
            }

            //Open up the content browser and navigate to the bottom element
            await Input.KeyPress(VirtualKeyCode.VK_R, _openUIAfterSleep, _openUIInitialSleep);
            await ReturnToTop();
            await NavigateVerticalOneStep(true);

            var previousWasFolder = false;

            //Go up the menu closing all categories until you reach index 1 (prefabs)
            while (MemoryHelper.GetMenusVisible() != 0 && await MemoryHelper.GetGlobalHoverVerbose() != 1)
            {
                //Open folder or category then back out 1 level of the UI
                await Input.KeyPress(VirtualKeyCode.RETURN, _enterUIAfterSleep, _enterUIInitialSleep);
                await Input.KeyPress(VirtualKeyCode.ESCAPE, _closeUIAfterSleep, _closeUIInitialSleep);

                //If on category then the UI should close. open UI and close category
                if (MemoryHelper.GetMenusVisible() == 0)
                {
                    await Input.KeyPress(VirtualKeyCode.VK_R, _openUIAfterSleep, _openUIInitialSleep);

                    if (!previousWasFolder)
                        await Input.KeyPress(VirtualKeyCode.RETURN, _enterUIAfterSleep, _enterUIInitialSleep);

                    previousWasFolder = false;
                }
                else
                {
                    previousWasFolder = true;
                }

                await NavigateVerticalOneStep(true);
                await Input.HandlePause();
            }

            _isInColdOpen = false;
        }
    }

    /// <summary>
    /// Opens the UI onto any tab requested (can setup UI from cold start)
    /// </summary>
    /// <param name="openToTab"> The tab to open up </param>
    public static async Task OpenUI(ContentBrowserTabs? openToTab = null)
    {
        Log.Information("Ensuring the UI is open");
        await ColdStart();

        //Ensure the UI menu is open
        while (MemoryHelper.GetMenusVisible() != 1)
        {
            await Input.KeyPress(VirtualKeyCode.VK_R, _openUIAfterSleep, _openUIInitialSleep);
            await Input.HandlePause();
        }

        if (openToTab != null)
            await MoveToTab((ContentBrowserTabs)openToTab);
    }

    /// <summary>
    /// Close the UI
    /// </summary>
    public static async Task CloseUI()
    {
        //Ensure the UI menu is open
        while (MemoryHelper.GetMenusVisible() != 0)
        {
            await Input.KeyPress(VirtualKeyCode.VK_R, _closeUIAfterSleep, _closeUIInitialSleep);
            await Input.HandlePause();
        }
    }

    /// <summary>
    /// Tries to open up the edit menu on the requested UI selection
    /// </summary>
    /// <param name="index"> The UI element to try to open </param>
    public static async Task OpenEditUI(int index)
    {
        // if (MemoryHelper.GetEditMenuState() == 1) throw new Exception("Trying to open edit UI when it is already open");

        await NavigateVertical(index);
        while (MemoryHelper.GetEditMenuState() == 0)
        {
            await Input.KeyPress(VirtualKeyCode.RETURN, _enterUIAfterSleep, _enterUIInitialSleep);
            await Input.HandlePause();
        }
    }

    /// <summary>
    /// Tries to close the edit menu if it is open
    /// </summary>
    public static async Task CloseEditUI(bool escape = false)
    {
        while (MemoryHelper.GetEditMenuState() != 0)
        {
            if (escape)
            {
                await Input.KeyPress(VirtualKeyCode.ESCAPE, _closeUIAfterSleep, _closeUIInitialSleep);
            }
            else
            {
                await Input.KeyPress(VirtualKeyCode.RETURN, _closeUIAfterSleep, _closeUIInitialSleep);
            }

            await Input.HandlePause();
        }
    }

    /// <summary>
    /// Navigates up and down the UI to by one step
    /// </summary>
    public static async Task NavigateVerticalOneStep(bool up)
    {
        await OpenUI();
        var initialIndex = await MemoryHelper.GetGlobalHoverVerbose();
        var currentIndex = initialIndex;

        do
        {
            await OpenUI();
            await Input.KeyPress(up ? VirtualKeyCode.VK_W : VirtualKeyCode.VK_S, _travelAfterSleep,
                _travelInitialSleep);
            await Input.HandlePause();
        } while ((currentIndex = await MemoryHelper.GetGlobalHoverVerbose()) == initialIndex);

        _navigationState.UpdateVerticalState(await MemoryHelper.GetGlobalHoverVerbose());

        //Logic below seems flawed
        // //UI should move a lot with this logic
        // if ((initialIndex == 0 && up) || (currentIndex == 0 && initialIndex != 0 && !up))
        //     return;
        //
        // //If the UI moved too much, UI messages probably messed up the pointers
        // if (Math.Abs(await MemoryHelper.GetGlobalHoverVerbose() - initialIndex) > 1)
        // {
        //     await NavigateVertical(initialIndex);
        //     await NavigateVerticalOneStep(up);
        // }
    }

    /// <summary>
    /// Navigates up and down the UI to specific indexes
    /// </summary>
    /// <param name="index"> Index to navigate to </param>
    public static async Task NavigateVertical(int index)
    {
        await OpenUI();
        var currentIndex = await MemoryHelper.GetGlobalHoverVerbose();
        if (currentIndex == index) return;

        do
        {
            await OpenUI();

            //Check if it would be faster to set the pointer (jump) instead
            if (Math.Abs(currentIndex - index) > 1)
            {
                //Make sure pointer is valid before setting it
                await MemoryHelper.GetGlobalHoverVerbose();
                if (index == 0)
                {
                    MemoryHelper.SetGlobalHover(index + 1);
                    await Input.KeyPress(VirtualKeyCode.VK_W, _travelAfterSleep, _travelInitialSleep);
                }
                else
                {
                    MemoryHelper.SetGlobalHover(index - 1);
                    await Input.KeyPress(VirtualKeyCode.VK_S, _travelAfterSleep, _travelInitialSleep);
                }
            }
            else
            {
                await NavigateVerticalOneStep(currentIndex > index);
            }

            await Input.HandlePause();
        } while ((currentIndex = await MemoryHelper.GetGlobalHoverVerbose()) != index);

        _navigationState.UpdateVerticalState(await MemoryHelper.GetGlobalHoverVerbose());
    }

    /// <summary>
    /// Updates vertical state to be current hovered value, can be different depending on previous runs
    /// </summary>
    private static async Task EnsureVerticalState()
    {
        _navigationState.UpdateVerticalState(await MemoryHelper.GetGlobalHoverVerbose());
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
        Log.Information("Ensuring we are on the correct tab. Expected Tab: {ExpectedTab}", tabIndex.ToString());
        await OpenUI();

        while (MemoryHelper.GetTopBrowserHover() != (int)tabIndex)
        {
            await OpenUI();

            //Calculate the distance from the current index to the new index allowing for rap-around navigation
            var leftDistance = (int)tabIndex > MemoryHelper.GetTopBrowserHover()
                ? MemoryHelper.GetTopBrowserHover() + (ContentBrowserTabsCount - (int)tabIndex)
                : MemoryHelper.GetTopBrowserHover() - (int)tabIndex;

            var rightDistance = (int)tabIndex < MemoryHelper.GetTopBrowserHover()
                ? ContentBrowserTabsCount - MemoryHelper.GetTopBrowserHover() + (int)tabIndex
                : (int)tabIndex - MemoryHelper.GetTopBrowserHover();

            //Press buttons based on above logic
            await Input.KeyPress(leftDistance < rightDistance ? VirtualKeyCode.VK_Q : VirtualKeyCode.VK_E,
                _travelTabAfterSleep, _travelTabInitialSleep);
            _navigationState.CurrentTabSelection = MemoryHelper.GetTopBrowserHover();

            await Input.HandlePause();
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
                await ReturnToTop();
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
        await Input.KeyPress(VirtualKeyCode.RETURN, _enterUIAfterSleep, _enterUIInitialSleep);

        //Update State
        _navigationState.ObjectBrowserState.CurrentCategory = category;
    }

    /// <summary>
    /// Navigate to a specific folder in the "Object Browser"
    /// </summary>
    /// <param name="folder"> The ForgeUIFolder to go to </param>
    public static async Task NavigateToFolder(ForgeUIFolder folder)
    {
        Log.Information(
            "Navigating to Object Browser Folder! {Folder} , The folders offset is {FolderOffset}, inside of the parent Category {FolderParent}"
            , folder.FolderName, folder.FolderOffset,
            folder.ParentCategory == null ? "No parent folder" : folder.ParentCategory.CategoryName);
        
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
        await Input.KeyPress(VirtualKeyCode.RETURN, _enterUIAfterSleep, _enterUIInitialSleep);

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
        Log.Information("Navigating to item {Item}", item.ObjectId.ToString());
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
        await Input.KeyPress(VirtualKeyCode.RETURN, _enterUIAfterSleep, _enterUIInitialSleep);
    }

    /// <summary>
    /// Home to the very top of the "Object Browser" ("recents" category)
    /// </summary>
    public static async Task HomeObjectBrowserToZero()
    {
        await MoveToTab(ContentBrowserTabs.ObjectBrowser);
        await HomeObjectBrowserToCategoryLevel();
        await ReturnToTop();
    }

    /// <summary>
    /// Home to the category level with the current category closed if inside one
    /// </summary>
    public static async Task HomeObjectBrowserToCategoryLevel()
    {
        Log.Information("Homing Object Browser UI");
        await MoveToTab(ContentBrowserTabs.ObjectBrowser);
        await HomeObjectBrowserToFolderLevel();

        //If navigation state is within a category but not inside a folder, proceed as necessary 
        if (_navigationState.ObjectBrowserState.GetObjectBrowserDepth() ==
            NavigationState._ObjectBrowserState.ObjectBrowserDepth.Folder)
        {
            //Close current category
            await NavigateVertical(_navigationState.ObjectBrowserState.CurrentCategory.CategoryOrder - 1);
            await Input.KeyPress(VirtualKeyCode.RETURN, _enterUIAfterSleep, _enterUIInitialSleep);

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
            await Input.KeyPress(VirtualKeyCode.BACK, _closeUIAfterSleep, _closeUIInitialSleep);

            //Update navigation state
            await EnsureVerticalState();
            _navigationState.ObjectBrowserState.CurrentFolder = null;
        }
    }
}