//
// Hooks for Studio
//
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
        internal partial class Hooks
        {
            private static AnimationClipsCache _animationClipsCache = new();
            private static AnimationClipsByType _animationClipsByType = new();

            /// <summary>
            /// Load animation clips in Studio
            /// </summary>
            /// <param name="__instance"></param>
            /// <param name="__result"></param>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Studio.Info), nameof(Studio.Info.LoadExcelDataCoroutine))]
            private static void LoadStudioAnims(Studio.Info __instance, ref IEnumerator __result)
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
                                var path = sex == 0 
                                    ? swapAnimInfo.PathMale : swapAnimInfo.PathFemale;
                                var ctrl = sex == 0 
                                    ? swapAnimInfo.ControllerMale : swapAnimInfo.ControllerFemale;

                                if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(ctrl))
                                {
                                    continue;
                                }

                                var animName = string.IsNullOrEmpty(swapAnimInfo.AnimationName) 
                                    ? ctrl : swapAnimInfo.AnimationName;
                                grp.dicCategory.Add(swapAnimInfo.StudioId, animName);
                                var animCat = new Dictionary<int, Info.AnimeLoadInfo>();
                                animGrp.Add(swapAnimInfo.StudioId, animCat);

                                var elementKey = (keyVal.Key == HFlag.EMode.houshi) 
                                    ? $"{keyVal.Key}-{swapAnimInfo.kindHoushi}" : $"{keyVal.Key}";

                                if (!AnimationClips.Clips.ContainsKey(elementKey))
                                {
                                    continue;
                                }
                                var clips = AnimationClips.Clips[elementKey];

                                for (var i = 0; i < clips.Count; i++)
                                {
                                    var newSlot = UniversalAutoResolver.GetUniqueSlotID();

                                    UniversalAutoResolver.LoadedStudioResolutionInfo
                                        .Add(new StudioResolveInfo { 
                                            GUID = swapAnimInfo.Guid,
                                            Slot = i,
                                            ResolveItem = true,
                                            LocalSlot = newSlot,
                                            Group = grpId,
                                            Category = swapAnimInfo.StudioId}
                                        );

                                    animCat.Add(newSlot, new Info.AnimeLoadInfo {
                                        name = clips[i],
                                        bundlePath = path,
                                        fileName = ctrl,
                                        clip = clips[i],
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

            // TODO: Modify this for testing when new animations are added.
            private static void LoadStudioAnimsCachedVersion(
                Studio.Info __instance,
                ref IEnumerator __result)
            {
                var bCached = true;

                _animationClipsCache.Read();
                _animationClipsByType.Read();

                if (_animationClipsCache.Clips.Keys.Count > 0)
                {
                    bCached = true;
                }
                else
                {
                    Log.Warning("Cache not found!");
                }

                if (bCached)
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

                                    var animName = string.IsNullOrEmpty(swapAnimInfo.AnimationName) ? ctrl : swapAnimInfo.AnimationName;
                                    grp.dicCategory.Add(swapAnimInfo.StudioId, animName);
                                    var animCat = new Dictionary<int, Info.AnimeLoadInfo>();
                                    animGrp.Add(swapAnimInfo.StudioId, animCat);
                                    //List<string> clipsName = new();
                                    var elementKey = (keyVal.Key == HFlag.EMode.houshi) ? $"{keyVal.Key}-{swapAnimInfo.kindHoushi}" : $"{keyVal.Key}";

                                    //var clips = _animationClips.Clips[$"{grpKey}-{ctrl}"];
                                    var clips = AnimationClips.Clips[elementKey];

                                    //if (clipsStatic == null)
                                    //{
                                    //    Log.Error($"Static Fail with null Key {elementKey} for {ctrl}");
                                    //}
                                    //else if (!clips.SequenceEqual(clipsStatic))
                                    //{
                                    //    Log.Error($"Static Fail Key does not match {elementKey} for {ctrl}");
                                    //}

                                    for (var i = 0; i < clips.Count; i++)
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
                                            name = clips[i],
                                            bundlePath = path,
                                            fileName = ctrl,
                                            clip = clips[i],
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
                else
                {
                    __result = __result.AppendCo(() =>
                    {
                        foreach (var keyVal in animationDict)
                        {
                            CreateGroup(0);
                            CreateGroup(1);

                            void CreateGroup(byte sex)
                            {
                                var grp = new Info.GroupInfo { 
                                    name = $"AL {(sex == 0 ? "M" : "F")} {keyVal.Key}" };
                                var animGrp = 
                                    new Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>();

                                var grpKey = $"{keyVal.Key}{sex}";
                                if (!EModeGroups.TryGetValue(grpKey, out var grpId))
                                {
                                    return;
                                }

                                foreach (
                                    var swapAnimInfo in keyVal.Value.Where(x => x.StudioId >= 0))
                                {
                                    var path = sex == 0 
                                        ? swapAnimInfo.PathMale : swapAnimInfo.PathFemale;
                                    var ctrl = sex == 0 
                                        ? swapAnimInfo.ControllerMale : swapAnimInfo.ControllerFemale;
                                    
                                    if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(ctrl))
                                    {
                                        continue;
                                    }

                                    var animName = string.IsNullOrEmpty(swapAnimInfo.AnimationName)
                                        ? ctrl : swapAnimInfo.AnimationName;
                                    grp.dicCategory.Add(swapAnimInfo.StudioId, animName);
                                    var animCat = new Dictionary<int, Info.AnimeLoadInfo>();
                                    animGrp.Add(swapAnimInfo.StudioId, animCat);
                                    List<string> clipsName = new();

                                    var controller = AssetBundleManager.LoadAsset(
                                        path,
                                        ctrl,
                                        typeof(RuntimeAnimatorController))
                                            .GetAsset<RuntimeAnimatorController>();

                                    var clips = controller.animationClips;

                                    if (controller == null)
                                    {
                                        continue;
                                    }

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
                                        clipsName.Add(clips[i].name);
                                    }
                                    if (!_animationClipsCache.Clips.ContainsKey($"{grpKey}-{ctrl}"))
                                    {
                                        _animationClipsCache.Clips.Add($"{grpKey}-{ctrl}", clipsName);
                                    }

                                    var elementKey = string.Empty;

                                    if (keyVal.Key == HFlag.EMode.houshi)
                                    {
                                        elementKey = $"{keyVal.Key}-{swapAnimInfo.kindHoushi}";
                                    }
                                    else
                                    {
                                        elementKey = $"{keyVal.Key}";
                                    }

                                    if (!_animationClipsByType.Clips.ContainsKey(elementKey))
                                    {
                                        _animationClipsByType.Clips.Add(elementKey, clipsName);
                                    }
                
                                    if (!_animationClipsByType.Clips[elementKey]
                                        .SequenceEqual(clipsName))
                                    {
                                        Log.Error($"Key does not match {elementKey} for {ctrl}");
                                    }
                                }

                                if (animGrp.Count > 0)
                                {
                                    __instance.dicAGroupCategory.Add(grpId, grp);
                                    __instance.dicAnimeLoadInfo.Add(grpId, animGrp);
                                }
                            }
                        }
                        _animationClipsCache.Save();
                        _animationClipsByType.Save();
                    });
                }

            }


            /// <summary>
            /// For reference
            /// </summary>
            /// <param name="__instance"></param>
            /// <param name="__result"></param>
            private static void LoadStudioAnimsOriginal(
                Studio.Info __instance,
                ref IEnumerator __result)
            {
                Log.Warning("Starting the plugin");

                __result = __result.AppendCo(() =>
                {
                    foreach (var keyVal in animationDict)
                    {
                        CreateGroup(0);
                        CreateGroup(1);

                        void CreateGroup(byte sex)
                        {
                            var grp = new Info.GroupInfo { name = $"AL {(sex == 0 ? "M" : "F")} {keyVal.Key}" };
                            var animGrp =
                                new Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>();

                            var grpKey = $"{keyVal.Key}{sex}";
                            if (!EModeGroups.TryGetValue(grpKey, out var grpId))
                            {
                                return;
                            }

                            foreach (var swapAnimInfo in keyVal.Value.Where(x => x.StudioId >= 0))
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
                                        typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();

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

                            if (animGrp.Count > 0)
                            {
                                __instance.dicAGroupCategory.Add(grpId, grp);
                                __instance.dicAnimeLoadInfo.Add(grpId, animGrp);
                            }
                        }
                    }
                });
            }
        }
    }
}
