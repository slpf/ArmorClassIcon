using System.Reflection;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace ArmorClassIcon.Patches;

public class ItemViewStatsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ItemViewStats), nameof(ItemViewStats.SetStaticInfo));
    }

    [PatchPostfix]
    public static void Postfix(ItemViewStats __instance, Item item, bool examined)
    {
        IconUpdater.Apply(__instance, item, examined);
    }

    public static void RefreshAllViews()
    {
        foreach (var view in UnityEngine.Object.FindObjectsOfType<GridItemView>())
        {
            if (view.Item == null) continue;

            view.UpdateStaticInfo();
        }
    }
}
