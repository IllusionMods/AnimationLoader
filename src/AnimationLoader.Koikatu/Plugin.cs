using System;
using System.Collections;
using BepInEx;
using HarmonyLib;
using IllusionUtility.GetUtility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Xml.Linq;
using System.Xml.Serialization;
using BepInEx.Configuration;
using BepInEx.Logging;
using Illusion.Extensions;
using Sideloader.AutoResolver;
using Studio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static HFlag;
using Manager;
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
