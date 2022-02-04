﻿//
// Entry point for AnimationLoader.KoikatsuSunshine
//
using BepInEx;
using KKAPI;


namespace AnimationLoader
{
    //
    // TODO:
    // Only load for main game no Studio
    // Not loading any type of animations in Studio the loading is just taking time at the moment
    // [BepInProcess(KoikatuAPI.StudioProcessName)]
    [BepInPlugin(PInfo.GUID + "_VR", PInfo.PluginDisplayName + " VR", PInfo.Version)]
    [BepInDependency(Sideloader.Sideloader.GUID, Sideloader.Sideloader.Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    [BepInProcess(KoikatuAPI.VRProcessName)]
    public partial class SwapAnim : BaseUnityPlugin
    {
    }
}
