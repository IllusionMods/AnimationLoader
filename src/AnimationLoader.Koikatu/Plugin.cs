//
// Entry point for AnimationLoader.Koikatu
//
using BepInEx;
using KKAPI;

namespace AnimationLoader
{

    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency(Sideloader.Sideloader.GUID, Sideloader.Sideloader.Version)]
    [BepInPlugin(PInfo.GUID, PInfo.PluginDisplayName, PInfo.Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInProcess(KoikatuAPI.GameProcessNameSteam)]
    [BepInProcess(KoikatuAPI.VRProcessName)]
    [BepInProcess(KoikatuAPI.VRProcessNameSteam)]
    [BepInProcess(KoikatuAPI.StudioProcessName)]
    public partial class SwapAnim : BaseUnityPlugin
    {
    }
}
