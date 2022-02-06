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
            private static RuntimeAnimatorController _controller;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Studio.Info), nameof(Studio.Info.LoadExcelDataCoroutine))]
            private static void LoadStudioAnims(Studio.Info __instance, ref IEnumerator __result)
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

                                if (_controller is null)
                                {
                                    _controller = AssetBundleManager.LoadAsset(
                                    path,
                                    ctrl,
                                    typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
                                }

                                if (_controller == null)
                                {
                                    continue;
                                }

                                var animName = string.IsNullOrEmpty(swapAnimInfo.AnimationName) ?
                                    ctrl : swapAnimInfo.AnimationName;
                                grp.dicCategory.Add(swapAnimInfo.StudioId, animName);
                                var animCat = new Dictionary<int, Info.AnimeLoadInfo>();
                                animGrp.Add(swapAnimInfo.StudioId, animCat);

                                var clips = _controller.animationClips;
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


            private static void LoadStudioAnimsOriginal(Studio.Info __instance, ref IEnumerator __result)
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

                                if (_controller == null)
                                {
                                    continue;
                                }

                                var animName = string.IsNullOrEmpty(swapAnimInfo.AnimationName) ?
                                    ctrl : swapAnimInfo.AnimationName;
                                grp.dicCategory.Add(swapAnimInfo.StudioId, animName);
                                var animCat = new Dictionary<int, Info.AnimeLoadInfo>();
                                animGrp.Add(swapAnimInfo.StudioId, animCat);

                                var clips = _controller.animationClips;
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
