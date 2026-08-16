using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT.InventoryLogic;
using EFT.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace ArmorClassIcon;

public static class IconUpdater
{
    private const float BadgeGap = 2f;

    private static readonly ConditionalWeakTable<ItemViewStats, List<Image>> CloneCache = new();

    public static void Apply(ItemViewStats stats, Item item, bool examined)
    {
        var icon = stats != null ? stats._armorClassIcon : null;
        if (icon == null) return;

        HideClones(stats);

        if (item is ArmorPlate) return;

        if (!IsKnownType(item)) return;

        var classes = CollectClasses(item);

        if (classes.Count == 0)
        {
            icon.gameObject.SetActive(false);
            return;
        }

        if (!IsEnabled(item)) return;

        ShowBadges(stats, icon, classes, examined);
    }

    private static List<int> CollectClasses(Item item)
    {
        if (Settings.DisplayMode.Value == IconDisplayMode.AllPlates
            && item.TryGetItemComponent<ArmorHolderComponent>(out var holder))
        {
            var plates = holder.ArmorPlates.Select(p => p.Armor.ArmorClass).Where(c => c > 0).ToList();
            if (plates.Count > 0) return plates;
        }

        var classes = new List<int>();

        var own = item.GetItemComponent<ArmorComponent>();
        if (own != null && own.ArmorClass > 0)
        {
            classes.Add(own.ArmorClass);
        }

        foreach (var child in item.GetItemComponentsInChildren<CompositeArmorComponent>(true))
        {
            if (child.ArmorClass > 0)
            {
                classes.Add(child.ArmorClass);
            }
        }

        if (classes.Count == 0) return classes;

        var max = classes.Max();
        var min = classes.Min();

        if (Settings.DisplayMode.Value == IconDisplayMode.MinMax && min != max)
        {
            return new List<int> { max, min };
        }

        return new List<int> { max };
    }

    private static void ShowBadges(ItemViewStats stats, Image icon, List<int> classes, bool examined)
    {
        var vertical = Settings.DisplayMode.Value == IconDisplayMode.MinMax;

        ShowBadge(icon, icon, classes[0], 0, examined, vertical);

        if (!CloneCache.TryGetValue(stats, out var clones))
        {
            clones = new List<Image>();
            CloneCache.Add(stats, clones);
        }

        clones.RemoveAll(c => c == null);

        for (var i = 1; i < classes.Count; i++)
        {
            while (clones.Count < i)
            {
                clones.Add(CreateClone(icon));
            }

            ShowBadge(icon, clones[i - 1], classes[i], i, examined, vertical);
        }
    }

    private static void ShowBadge(Image original, Image badge, int armorClass, int index, bool examined, bool vertical)
    {
        var sprite = ResourcesCache.Pop<Sprite>("Mod Types/icon_type_mod_armor_plate_" + armorClass);
        if (sprite == null) return;

        var rect = original.rectTransform.rect;
        var width = rect.width > 0f ? rect.width : sprite.rect.width;
        var height = rect.height > 0f ? rect.height : sprite.rect.height;

        var offset = vertical
            ? new Vector2(0f, index * (height + BadgeGap))
            : new Vector2(index * (width + BadgeGap), 0f);

        badge.sprite = sprite;
        badge.rectTransform.anchoredPosition = original.rectTransform.anchoredPosition + offset;
        badge.gameObject.SetActive(examined);
    }

    private static Image CreateClone(Image original)
    {
        var clone = Object.Instantiate(original, original.transform.parent);
        clone.gameObject.SetActive(false);
        return clone;
    }

    private static void HideClones(ItemViewStats stats)
    {
        if (!CloneCache.TryGetValue(stats, out var clones)) return;

        foreach (var clone in clones)
        {
            if (clone != null) clone.gameObject.SetActive(false);
        }
    }

    private static bool IsKnownType(Item item)
    {
        return item is Armor or Headwear or Vest or Visors or FaceCover;
    }

    private static bool IsEnabled(Item item)
    {
        return item switch
        {
            Armor => Settings.EnableBodyArmor.Value,
            Headwear => Settings.EnableHeadwear.Value,
            Vest => Settings.EnableArmoredRigs.Value,
            Visors => Settings.EnableVisors.Value,
            FaceCover => Settings.EnableFaceCovers.Value,
            _ => false
        };
    }
}
