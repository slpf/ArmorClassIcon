using System.Reflection;
using System.Threading;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace ArmorClassIcon.Patches;

public class ItemViewStatsPatch : ModulePatch
{
    private static int _refreshRequested;

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ItemViewStats), nameof(ItemViewStats.SetStaticInfo));
    }

    [PatchPostfix]
    public static void Postfix(ItemViewStats __instance, Item item, bool examined)
    {
        IconUpdater.Apply(FindOwnerView(__instance), __instance, item, examined);
    }

    private static GridItemView FindOwnerView(Component component)
    {
        for (var transform = component.transform; transform != null; transform = transform.parent)
        {
            var view = transform.GetComponent<GridItemView>();
            if (view != null) return view;
        }

        return null;
    }

    public static void RequestRefresh()
    {
        Interlocked.Exchange(ref _refreshRequested, 1);
    }

    public static void RefreshIfRequested()
    {
        if (Interlocked.Exchange(ref _refreshRequested, 0) == 0) return;

        RefreshAllViews();
    }

    private static void RefreshAllViews()
    {
        foreach (GridItemView view in Object.FindObjectsOfType<GridItemView>())
        {
            if (view.Item == null) continue;

            IconUpdater.Apply(view, view.BottomPanel?.ItemViewStats, view.Item, view.Examined);
        }
    }
}
