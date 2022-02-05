//
// Entry point for AnimationLoader.Koikatu
//
using BepInEx;
using KKAPI;

namespace AnimationLoader
{
    [UnityEngine.Scripting.Preserve]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency(Sideloader.Sideloader.GUID, Sideloader.Sideloader.Version)]
    [BepInPlugin(PInfo.GUID, PInfo.PluginDisplayName, PInfo.Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInProcess(KoikatuAPI.GameProcessNameSteam)]
    [BepInProcess(KoikatuAPI.StudioProcessName)]
    [BepInProcess(KoikatuAPI.VRProcessName)]
    [BepInProcess(KoikatuAPI.VRProcessNameSteam)]
    public partial class SwapAnim : BaseUnityPlugin
    {
    }
}
