using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Studio;

using BepInEx.Logging;
using Sideloader.AutoResolver;
using HarmonyLib;

namespace AnimationLoader
{
    public partial class SwapAnim
    {
        public delegate void WatDel(
            Studio.Info __instance, 
            HFlag.EMode mode, 
            List<SwapAnimationInfo> lstAnimations);

        internal partial class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Studio.Info), nameof(Studio.Info.LoadExcelDataCoroutine))]
            private static void LoadStudioAnims(Studio.Info __instance, ref IEnumerator __result)
            {
                var watHoushi = new WatDel(Wat);
                var watSonyu = new WatDel(Wat);

                __result = __result.AppendCo(() =>
                {
                    watHoushi(__instance, HFlag.EMode.houshi, animationDict[HFlag.EMode.houshi]);
                });
                __result = __result.AppendCo(() =>
                {
                    watSonyu(__instance, HFlag.EMode.sonyu, animationDict[HFlag.EMode.sonyu]);
                });
            }

            //[HarmonyPostfix]
            //[HarmonyPatch(typeof(Studio.Info), nameof(Studio.Info.LoadExcelDataCoroutine))]
            private static void LoadStudioAnimsOriginal(Studio.Info __instance, ref IEnumerator __result)
            {
                __result = __result.AppendCo(() =>
                {
                    foreach (var keyVal in animationDict)
                    {
                        CreateGroup(0);
                        CreateGroup(1);

                        void CreateGroup(byte sex)
                        {
                            var grp = new Info.GroupInfo { name = $"AL {(sex == 0 ? "M" : "F")} {keyVal.Key}" };
                            var animGrp = new Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>();

                            var grpKey = $"{keyVal.Key}{sex}";
                            if (!EModeGroups.TryGetValue(grpKey, out var grpId))
                            {
                                return;
                            }

                            foreach (var swapAnimInfo in keyVal.Value.Where(x => x.StudioId >= 0))
                            {
                                var path = sex == 0 ? swapAnimInfo.PathMale : swapAnimInfo.PathFemale;
                                var ctrl = sex == 0 ? swapAnimInfo.ControllerMale : swapAnimInfo.ControllerFemale;

                                if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(ctrl))
                                {
                                    continue;
                                }

                                var controller = AssetBundleManager.LoadAsset(path, ctrl, typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
                                if (controller == null)
                                {
                                    continue;
                                }

                                var animName = string.IsNullOrEmpty(swapAnimInfo.AnimationName) ? ctrl : swapAnimInfo.AnimationName;
                                grp.dicCategory.Add(swapAnimInfo.StudioId, animName);
                                var animCat = new Dictionary<int, Info.AnimeLoadInfo>();
                                animGrp.Add(swapAnimInfo.StudioId, animCat);

                                var clips = controller.animationClips;
                                for (int i = 0; i < clips.Length; i++)
                                {
                                    var newSlot = UniversalAutoResolver.GetUniqueSlotID();

                                    UniversalAutoResolver.LoadedStudioResolutionInfo.Add(new StudioResolveInfo {
                                        GUID = swapAnimInfo.Guid,
                                        Slot = i,
                                        ResolveItem = true,
                                        LocalSlot = newSlot,
                                        Group = grpId,
                                        Category = swapAnimInfo.StudioId
                                    });

                                    animCat.Add(newSlot, new Info.AnimeLoadInfo {
                                        name = clips[i].name,
                                        bundlePath = path,
                                        fileName = ctrl,
                                        clip = clips[i].name,
                                    });
                                }
                            }

                            if (animGrp.Count > 0)
                            {
                                __instance.dicAGroupCategory.Add(grpId, grp);
                                __instance.dicAnimeLoadInfo.Add(grpId, animGrp);
                            }
                        }
                    }
                });
            }

            private static void Wat(Studio.Info __instance, HFlag.EMode mode, List<SwapAnimationInfo> lstAnimations)
            {

                Log.Level(LogLevel.Warning, $"Entered Wat");

                CreateGroup(0);
                CreateGroup(1);

                void CreateGroup(byte sex)
                {
                    /*
                    public class GroupInfo
                    {
                        public string name;

                        public Dictionary<int, string> dicCategory;

                        [MethodImpl(MethodImplOptions.NoInlining)]
                        public GroupInfo()
                        {
                        }
                    }*/

                    //var sex = (byte)0;
                    //var sexFemale = (byte)1;

                    var grp = new Info.GroupInfo { name = $"AL {(sex == 0 ? "M" : "F")} {mode}" };
                    var animGrp = new Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>();
                    //var grpFemale = new Info.GroupInfo { name = $"AL F {mode}" };
                    //var animGrpFemale =
                    //    new Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>();

                    var grpKey = $"{mode}{sex}";
                    if (!EModeGroups.TryGetValue(grpKey, out var grpId))
                    {
                        return;
                    }

                    //var grpFemaleKey = $"{mode}{sexFemale}";
                    //if (!EModeGroups.TryGetValue(grpKey, out var grpIdFemale))
                    //{
                    //    return;
                    //}

                    // Male Group first
                    foreach (var swapAnimInfo in lstAnimations.Where(x => x.StudioId >= 0))
                    {
                        var path =
                            sex == 0 ? swapAnimInfo.PathMale
                            : swapAnimInfo.PathFemale;
                        var ctrl =
                            sex == 0 ? swapAnimInfo.ControllerMale
                            : swapAnimInfo.ControllerFemale;

                        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(ctrl))
                        {
                            continue;
                        }

                        var controller = AssetBundleManager.LoadAsset(
                            path,
                            ctrl,
                            typeof(RuntimeAnimatorController))
                                .GetAsset<RuntimeAnimatorController>();
                        if (controller == null)
                        {
                            continue;
                        }

                        var animName = string.IsNullOrEmpty(swapAnimInfo.AnimationName) ?
                            ctrl : swapAnimInfo.AnimationName;
                        grp.dicCategory.Add(swapAnimInfo.StudioId, animName);
                        var animCat = new Dictionary<int, Info.AnimeLoadInfo>();
                        animGrp.Add(swapAnimInfo.StudioId, animCat);

                        var clips = controller.animationClips;
                        for (var i = 0; i < clips.Length; i++)
                        {
                            var newSlot = UniversalAutoResolver.GetUniqueSlotID();

                            UniversalAutoResolver.LoadedStudioResolutionInfo
                                .Add(new StudioResolveInfo {
                                    GUID = swapAnimInfo.Guid,
                                    Slot = i,
                                    ResolveItem = true,
                                    LocalSlot = newSlot,
                                    Group = grpId,
                                    Category = swapAnimInfo.StudioId
                                });

                            animCat.Add(newSlot, new Info.AnimeLoadInfo {
                                name = clips[i].name,
                                bundlePath = path,
                                fileName = ctrl,
                                clip = clips[i].name,
                            });
                        }
                    }

                    // Female animation group
                    //grpFemale.dicCategory = grp.dicCategory;
                    Log.Level(LogLevel.Warning, $"For each should start {animGrp.Count} Wat");
                    foreach (var e in animGrp)
                    {
                        Log.Level(LogLevel.Warning, $"Key {e.Key}\n");
                        foreach (var c in e.Value)
                        {
                            Log.Level(LogLevel.Warning, $"Key {c.Key}\n");
                            break;
                        }
                    }
                    Log.Level(LogLevel.Warning, $"For each should ended Wat");
                    /*
                     * "1": {
                        "100010393": {
                          "clip": "L_Idle",
                          "option": null,
                          "name": "L_Idle",
                          "manifest": "",
                          "bundlePath": "anim_imports/kplug/male/01_40.unity3d",
                          "fileName": "khh_m_60",
                          "Check": true
                        }
                     */

                    if (animGrp.Count > 0)
                    {
                        Log.Level(LogLevel.Warning, $"Adding group Wat");
                        //Log.Level(LogLevel.Error, $"Group ID: {grpId}\n{JsonConvert.SerializeObject(grp)}\n");
                        __instance.dicAGroupCategory.Add(grpId, grp);
                        __instance.dicAnimeLoadInfo.Add(grpId, animGrp);
                    }
                }
            }
        }
    }
}
