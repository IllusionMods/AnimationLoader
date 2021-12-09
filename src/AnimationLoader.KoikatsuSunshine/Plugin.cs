//
// Entry for KKS
//
using BepInEx;
using KKAPI;


namespace AnimationLoader
{
    //
    // TODO:
    // Only load for main game no Studio
    // Not loading any type of animations in Studio the loading is just taking time at the moment
    //
    [BepInPlugin(GUID, DisplayName, Version)]
    [BepInDependency(Sideloader.Sideloader.GUID, Sideloader.Sideloader.Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInProcess(KoikatuAPI.GameProcessName)]
    [BepInProcess(KoikatuAPI.StudioProcessName)]
    public partial class SwapAnim : BaseUnityPlugin
    {
    }
}
