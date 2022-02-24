//
// Entry point for AnimationLoader.KoikatsuSunshine
//
using BepInEx;

using KKAPI;


namespace AnimationLoader
{
    [BepInDependency(Sideloader.Sideloader.GUID, Sideloader.Sideloader.Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInPlugin(GUID, PluginDisplayName, Version)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInProcess(KoikatuAPI.StudioProcessName)]
    [BepInProcess(KoikatuAPI.VRProcessName)]
    public partial class SwapAnim : BaseUnityPlugin
    {
    }
}
