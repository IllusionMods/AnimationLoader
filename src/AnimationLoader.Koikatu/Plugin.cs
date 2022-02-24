//
// Entry point for AnimationLoader.Koikatu
//
using BepInEx;
using KKAPI;

//[UnityEngine.Scripting.Preserve]

namespace AnimationLoader
{
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInDependency(Sideloader.Sideloader.GUID, Sideloader.Sideloader.Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInProcess(KoikatuAPI.GameProcessNameSteam)]
    [BepInProcess(KoikatuAPI.StudioProcessName)]
    [BepInProcess(KoikatuAPI.VRProcessName)]
    [BepInProcess(KoikatuAPI.VRProcessNameSteam)]
    [BepInPlugin(PInfo.GUID, PInfo.PluginDisplayName, PInfo.Version)]
    public partial class SwapAnim : BaseUnityPlugin
    {
    }
}
