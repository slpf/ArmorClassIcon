using System;
using BepInEx.Configuration;
using ArmorClassIcon.Patches;

namespace ArmorClassIcon;

public static class Settings
{
    private const string CategoryToggles = "Toggles";

    public static ConfigEntry<bool> EnableBodyArmor;
    public static ConfigEntry<bool> EnableHeadwear;
    public static ConfigEntry<bool> EnableArmoredRigs;
    public static ConfigEntry<bool> EnableVisors;
    public static ConfigEntry<bool> EnableFaceCovers;

    public static ConfigEntry<IconDisplayMode> DisplayMode;

    public static void Init(ConfigFile config)
    {
        EnableBodyArmor = config.Bind(CategoryToggles, "Enable on body armor", true,
            "Show armor class icon on body armor.");

        EnableHeadwear = config.Bind(CategoryToggles, "Enable on headwear", true,
            "Show armor class icon on armored helmets.");

        EnableArmoredRigs = config.Bind(CategoryToggles, "Enable on armored rigs", true,
            "Show armor class icon on rigs with built-in armor.");

        EnableVisors = config.Bind(CategoryToggles, "Enable on visors", true,
            "Show armor class icon on armored visors.");

        EnableFaceCovers = config.Bind(CategoryToggles, "Enable on face covers", true,
            "Show armor class icon on armored face covers.");

        DisplayMode = config.Bind(CategoryToggles, "Icon display mode", IconDisplayMode.MaxClass,
            "MaxClass - single icon of the highest class, MinMax - min and max class icons stacked vertically, AllPlates - one icon per plate in slot order.");

        EnableBodyArmor.SettingChanged += OnSettingChanged;
        EnableHeadwear.SettingChanged += OnSettingChanged;
        EnableArmoredRigs.SettingChanged += OnSettingChanged;
        EnableVisors.SettingChanged += OnSettingChanged;
        EnableFaceCovers.SettingChanged += OnSettingChanged;
        DisplayMode.SettingChanged += OnSettingChanged;
    }

    private static void OnSettingChanged(object sender, EventArgs args)
    {
        ItemViewStatsPatch.RefreshAllViews();
    }
}

public enum IconDisplayMode
{
    MaxClass,
    MinMax,
    AllPlates
}
