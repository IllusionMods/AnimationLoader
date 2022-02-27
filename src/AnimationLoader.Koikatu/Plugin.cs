//
// Entry point for AnimationLoader.Koikatu
//
using BepInEx;

using KKAPI;


namespace AnimationLoader
{
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency(Sideloader.Sideloader.GUID, Sideloader.Sideloader.Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInProcess(KoikatuAPI.GameProcessNameSteam)]
    [BepInProcess(KoikatuAPI.StudioProcessName)]
    [BepInProcess(KoikatuAPI.VRProcessName)]
    [BepInProcess(KoikatuAPI.VRProcessNameSteam)]
    [BepInPlugin(GUID, PluginDisplayName, Version)]
    public partial class SwapAnim : BaseUnityPlugin
    {
    }
}
