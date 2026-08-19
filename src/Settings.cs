using System;
using System.Collections.Generic;
using ArmorClassIcon.Patches;
using BepInEx.Configuration;

namespace ArmorClassIcon;

public static class Settings
{
    private const string CategoryDisplay = "1. Display";
    private const string CategoryItemTypes = "2. Item Types";
    private const string CategoryMode = "3. Display Mode";
    private const string CategoryCustomPreset = "4. Custom preset";

    public static ConfigEntry<bool> ShowInInventory;
    public static ConfigEntry<bool> ShowAtTraders;
    public static ConfigEntry<bool> ShowOnFlea;

    public static ConfigEntry<bool> EnableBodyArmor;
    public static ConfigEntry<bool> EnableHeadwear;
    public static ConfigEntry<bool> EnableArmoredRigs;
    public static ConfigEntry<bool> EnableVisors;
    public static ConfigEntry<bool> EnableFaceCovers;
    public static ConfigEntry<bool> EnableAccessories;

    public static ConfigEntry<string> FleaDisplayMode;

    public static ConfigEntry<string> DisplayMode;

    public static readonly Dictionary<string, ConfigEntry<bool>> SlotToggles = new();

    private static readonly (string Id, string Name)[] PlateSlots =
    {
        ("front_plate", "Front plate"),
        ("back_plate", "Back plate"),
        ("left_side_plate", "Left side plate"),
        ("right_side_plate", "Right side plate"),
        ("soft_armor_front", "Soft armor front"),
        ("soft_armor_back", "Soft armor back"),
        ("soft_armor_left", "Soft armor left"),
        ("soft_armor_right", "Soft armor right"),
        ("collar", "Collar"),
        ("groin", "Groin"),
        ("groin_back", "Groin back"),
        ("shoulder_l", "Left shoulder"),
        ("shoulder_r", "Right shoulder"),
        ("helmet_top", "Helmet top"),
        ("helmet_back", "Helmet back"),
        ("helmet_eyes", "Helmet eyes"),
        ("helmet_jaw", "Helmet jaw"),
        ("helmet_ears", "Helmet ears")
    };

    public static IconDisplayMode InvMode => ParseMode(DisplayMode.Value);

    public static IconDisplayMode FleaMode => ParseMode(FleaDisplayMode.Value);

    public static void Init(ConfigFile config)
    {
        ShowInInventory = config.Bind(CategoryDisplay, "Show in inventory", true,
            "Show armor class icons in inventory screens.");

        ShowAtTraders = config.Bind(CategoryDisplay, "Show at traders", true,
            "Show armor class icons in trader screens.");

        ShowOnFlea = config.Bind(CategoryDisplay, "Show on flea market", true,
            "Show armor class icons on flea market offers.");

        EnableBodyArmor = config.Bind(CategoryItemTypes, "Enable on body armor", true,
            "Show armor class icon on body armor.");

        EnableHeadwear = config.Bind(CategoryItemTypes, "Enable on headwear", true,
            "Show armor class icon on armored helmets.");

        EnableArmoredRigs = config.Bind(CategoryItemTypes, "Enable on armored rigs", true,
            "Show armor class icon on rigs with built-in armor.");

        EnableVisors = config.Bind(CategoryItemTypes, "Enable on visors", true,
            "Show armor class icon on armored visors.");

        EnableFaceCovers = config.Bind(CategoryItemTypes, "Enable on face covers", true,
            "Show armor class icon on armored face covers.");

        EnableAccessories = config.Bind(CategoryItemTypes, "Enable on accessories", true,
            "Show armor class icon on equipment components with armor (ArmoredEquipment items not covered by other types).");

        FleaDisplayMode = config.Bind(CategoryMode, "Flea market display mode", "Max Class",
            new ConfigDescription(
                "Armor class display type for the flea market.",
                new AcceptableValueList<string>("Highest Class", "Lowest & Highest")));

        DisplayMode = config.Bind(CategoryMode, "Inventory & traders display mode", "Max Class",
            new ConfigDescription(
                "Armor class display type for the inventory and trader screens.",
                new AcceptableValueList<string>("Highest Class", "Lowest & Highest", "All Plates", "Custom")));

        foreach (var (id, name) in PlateSlots)
            SlotToggles[id] = config.Bind(CategoryCustomPreset, name, true,
                "Show this plate slot in Custom display mode. Uncheck to hide the slot.");

        ShowInInventory.SettingChanged += OnSettingChanged;
        ShowAtTraders.SettingChanged += OnSettingChanged;
        ShowOnFlea.SettingChanged += OnSettingChanged;
        EnableBodyArmor.SettingChanged += OnSettingChanged;
        EnableHeadwear.SettingChanged += OnSettingChanged;
        EnableArmoredRigs.SettingChanged += OnSettingChanged;
        EnableVisors.SettingChanged += OnSettingChanged;
        EnableFaceCovers.SettingChanged += OnSettingChanged;
        EnableAccessories.SettingChanged += OnSettingChanged;
        FleaDisplayMode.SettingChanged += OnSettingChanged;
        DisplayMode.SettingChanged += OnSettingChanged;

        foreach (var toggle in SlotToggles.Values) toggle.SettingChanged += OnSettingChanged;
    }

    public static bool IsSlotShown(string slotId)
    {
        return SlotToggles.TryGetValue(slotId.ToLowerInvariant(), out var toggle) ? toggle.Value : true;
    }

    private static void OnSettingChanged(object sender, EventArgs args)
    {
        ItemViewStatsPatch.RefreshAllViews();
    }

    private static IconDisplayMode ParseMode(string value)
    {
        return value?.Trim() switch
        {
            "Lowest & Highest" => IconDisplayMode.MinMax,
            "All Plates" => IconDisplayMode.AllPlates,
            "Custom" => IconDisplayMode.Custom,
            _ => IconDisplayMode.MaxClass
        };
    }
}

public enum IconDisplayMode
{
    MaxClass,
    MinMax,
    AllPlates,
    Custom
}