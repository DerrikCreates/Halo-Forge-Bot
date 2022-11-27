using System;
using System.Collections.Generic;
using System.IO;
using InfiniteForgeConstants.Forge_UI.Object_Browser;
using InfiniteForgeConstants.ObjectSettings;
using Newtonsoft.Json;

namespace Halo_Forge_Bot.config;

//Rewrite of:
// This is the most cursed thing ive ever wrote. I just wanted finished
public class ConformForgeObjects
{
    public static void BuildUiLayout()
    {
        string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
        var data = File.ReadAllLines(strWorkPath + "/config/ForgeObjects.txt");
        
        foreach (var line in data)
        {
            AddItemLine(line);
        }
        
        JsonSerializerSettings s = new JsonSerializerSettings();
        s.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        var cp = ForgeObjectBrowser.Categories["Accents"].CategoryFolders["City_Props"];
        var cpMP = ForgeObjectBrowser.Categories["Accents"].CategoryFolders["City_Props_MP"];

        ForgeObjectBrowser.Categories["Accents"].CategoryFolders["City_Props"] = cpMP;
        ForgeObjectBrowser.Categories["Accents"].CategoryFolders["City_Props_MP"] = cp;
        var a = JsonConvert.SerializeObject(ForgeObjectBrowser.Categories, s);
        // File.WriteAllText("z:/josh/ItemData.json", a);
    }

    public static void AddItemLine(string line)
    {
        var sections = line.Split(new char[] { '>', ':' });
        AddItem(FixCapital(sections[0].Trim()), FixCapital(sections[1].Trim()), sections[2].Trim(), sections[3].Trim());
    }
    
    public static void AddItem(string categoryName, string folderName, string itemName, string itemID)
    {
        var itemEnumName = Enum.GetName((ObjectId)int.Parse(itemID));
        categoryName = categoryName.Replace("Fx", "FX").Replace(" ", "_");

        folderName = folderName.Replace("Mp", "MP").Replace(" ", "_").Replace("Bazzar", "Bazaar")
            .Replace("Missles", "Missiles").Replace("Unsc", "UNSC").Replace("-", "").Replace("__", "_")
            .Replace("_/", "");

        if (categoryName == "Halo_Design_Set")
        {
            folderName = folderName.Replace("Crate", "Crates");
        }
        // Handle the first letter in the string.

        if (categoryName == "Z_null")
        {
            return;
        }

        if (itemEnumName == null)
        {
        }

        ForgeObjectBrowser.Categories[categoryName].CategoryFolders[folderName]
            .AddItem(itemEnumName ??= itemName, Enum.Parse<ObjectId>(itemID));
    }

    public static string FixCapital(string text)
    {
        var array = text.ToCharArray();
        if (array.Length >= 1)
        {
            if (char.IsLower(array[0]))
            {
                array[0] = char.ToUpper(array[0]);
            }
        }

        for (var x = 1; x < array.Length; x++)
        {
            if (array[x - 1] == ' ' || array[x - 1] == '_')
            {
                if (char.IsLower(array[x]))
                {
                    array[x] = char.ToUpper(array[x]);
                }
            }
        }

        return new string(array).Replace("Fx", "FX").Replace("Mp", "MP").Replace("Unsc", "UNSC");
    }
}