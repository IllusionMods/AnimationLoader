using System.Collections.Generic;
using System.Linq;
using System.Text;

using Illusion.Extensions;
using Manager;

using BepInEx.Logging;
using HarmonyLib;

namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal partial class Hooks
        {
            /// <summary>
            /// Add new animations to lstAnimInfo aibu and sonyu
            /// </summary>
            /// <param name="__instance"></param>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.CreateAllAnimationList))]
            private static void ExtendList(object __instance)
            {
                _hprocEarlyObjInstance = __instance;
                var procObj = Traverse.Create(__instance);
                var addedAnimations = new StringBuilder();
#if KK
                var hlist = Singleton<Game>.Instance.glSaveData.playHList;
#elif KKS
                var hlist = Game.globalData.playHList;
                var flags = procObj.Field<HFlag>("flags").Value;
                var hExp = flags.lstHeroine[0].hExp;
#endif
                var lstAnimInfo = procObj
                    .Field<List<HSceneProc.AnimationListInfo>[]>("lstAnimInfo").Value;
                swapAnimationMapping =
                    new Dictionary<HSceneProc.AnimationListInfo, SwapAnimationInfo>();
                var countGameA = 0;
                var countAL = 0;
                var strTmp = string.Empty;
                countGameA = Utilities.CountAnimations(lstAnimInfo);
                foreach (var anim in animationDict.SelectMany(
                    e => e.Value,
                    (e, a) => a
                ))
                {
                    var mode = (int)anim.Mode;
                    if (mode < 0 || mode >= lstAnimInfo.Length)
                    {
                        continue;
                    }
                    var animListInfo = lstAnimInfo[(int)anim.Mode];

                    var donorInfo = animListInfo
                        .FirstOrDefault(x => x.id == anim.DonorPoseId)?.DeepCopy();

                    if (donorInfo == null)
                    {
                        Log.Level(LogLevel.Warning, $"0009: No donor: mode={anim.Mode} " +
                            $"DonorPoseId={anim.DonorPoseId}");
                        continue;
                    }

                    if (anim.NeckDonorId >= 0 && anim.NeckDonorId != anim.DonorPoseId)
                    {
                        // PR #23 Change to Log.Level to always show log, update log ID's
                        // use temp variable to add log to log list
                        var newNeckDonor = animListInfo
                            .FirstOrDefault(x => x.id == anim.NeckDonorId);
                        if (newNeckDonor == null)
                        {
                            strTmp = $"0029: Invalid or missing " +
                                $"NeckDonorId: mode={anim.Mode} NeckDonorId={anim.NeckDonorId}";
                            Log.Level(LogLevel.Warning, strTmp);
                            addedAnimations.Append(strTmp);
                        }
                        else
                        {
                            var newMotionNeck = newNeckDonor?.paramFemale?.fileMotionNeck;
                            if (newMotionNeck == null)
                            {
                                strTmp = $"0030: NeckDonorId didn't point to" +
                                    $" a usable fileMotionNeck: " +
                                    $"mode={anim.Mode} NeckDonorId={anim.NeckDonorId}";
                                Log.Level(LogLevel.Warning, strTmp);
                                addedAnimations.Append(strTmp);
                            }
                            else
                            {
                                donorInfo.paramFemale.fileMotionNeck = newMotionNeck;
                            }
                        }
                    }
                    if (anim.FileMotionNeck != null)
                    {
                        donorInfo.paramFemale.fileMotionNeck = anim.FileMotionNeck;
                    }
                    if (anim.IsFemaleInitiative != null)
                    {
                        donorInfo.isFemaleInitiative = anim.IsFemaleInitiative.Value;
                    }

                    if (!string.IsNullOrEmpty(anim.FileSiruPaste))
                    {
                        // Check if FileSuruPaset is on dictionary first
                        if (SiruPasteFiles.TryGetValue(
                            anim.FileSiruPaste.ToLower(), out var fileSiruPaste))
                        {
                            donorInfo.paramFemale.fileSiruPaste = fileSiruPaste;
                        }
                        else
                        {
                            // TODO: Check that it is a valid name
                            donorInfo.paramFemale.fileSiruPaste = anim.FileSiruPaste.ToLower();
                        }
                    }

                    donorInfo.lstCategory = anim.categories.Select(c =>
                    {
                        var cat = new HSceneProc.Category {
                            category = (int)c
                        };
                        return cat;
                    }).ToList();

                    // The mode and <kindHoushi>Hand</kindHoushi> is not honored
                    if (anim.Mode == HFlag.EMode.houshi)
                    {
                        donorInfo.kindHoushi = (int)anim.kindHoushi;
                    }
#if KKS
                    // Update name so it shows on button text label
                    donorInfo.nameAnimation = anim.AnimationName;
#endif
                    animListInfo.Add(donorInfo);
                    swapAnimationMapping[donorInfo] = anim;
                    addedAnimations.Append($"EMode={anim.Mode,6} Name={anim.AnimationName}, " +
                        $"[Key={AnimationInfo.GetKey(anim)}] donor release={donorInfo.isRelease}\n");
                    countAL++;
                }
                addedAnimations.Append($"\n{countAL + countGameA} animations available: Game " +
                    $"standard = {countGameA} " +
                    $"AnimationLoader = {countAL}\n");
#if DEBUG
                Log.Warning($"0012: Added animations:\n\n{addedAnimations}");
                Utilities.SaveAnimInfo();
#else
                Log.Level(LogLevel.Debug, $"0012: Added animations:\n\n{addedAnimations}");
#endif
            }
        }
    }
}
