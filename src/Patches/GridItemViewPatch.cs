using System.Reflection;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace ArmorClassIcon.Patches;

public class GridItemViewPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GridItemView), nameof(GridItemView.UpdateInfo));
    }

    [PatchPostfix]
    public static void Postfix(GridItemView __instance)
    {
        if (__instance.Item == null) return;

        IconUpdater.Apply(__instance, __instance.ItemViewStats, __instance.Item, __instance.Examined);
    }
}
