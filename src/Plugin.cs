using System.Reflection;
using ArmorClassIcon;
using ArmorClassIcon.Patches;
using BepInEx;

[assembly: AssemblyProduct(ModInfo.Name)]
[assembly: AssemblyTitle(ModInfo.Name)]
[assembly: AssemblyDescription(ModInfo.Description)]
[assembly: AssemblyCopyright(ModInfo.Copyright)]
[assembly: AssemblyVersion(ModInfo.Version)]
[assembly: AssemblyFileVersion(ModInfo.Version)]
[assembly: AssemblyInformationalVersion(ModInfo.Version)]

namespace ArmorClassIcon;

[BepInPlugin(ModInfo.Guid, ModInfo.ClientName, ModInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Settings.Init(Config);

        new ItemViewStatsPatch().Enable();
        new GridItemViewPatch().Enable();
    }

    private void LateUpdate()
    {
        ItemViewStatsPatch.RefreshIfRequested();
    }
}
