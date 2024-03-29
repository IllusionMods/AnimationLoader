﻿using System.Collections.Generic;
using System.Linq;
using System.Text;

using Illusion.Extensions;

using BepInEx.Logging;
using HarmonyLib;

namespace AnimationLoader
{
    public partial class SwapAnim
    {
#if KKS
        internal static UsedAnimations _usedAnimations = new();
#endif
        internal static AnimationsUseStats _animationsUseStats = new();

        internal partial class Hooks
        {
            internal static bool once = false;
            private static List<HSceneProc.AnimationListInfo>[] _gameAnimations = 
                new List<HSceneProc.AnimationListInfo>[8];

            /// <summary>
            /// Add new animations to lstAnimInfo (list of all animations available)
            /// aibu and sonyu
            /// </summary>
            /// <param name="__instance"></param>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.CreateAllAnimationList))]
            private static void ExtendList(object __instance)
            {
                var hSceneTraverse = Traverse.Create(__instance);
                var addedAnimations = new StringBuilder();
                var lstAnimInfo = hSceneTraverse
                    .Field<List<HSceneProc.AnimationListInfo>[]>("lstAnimInfo").Value;

                var countGA = 0;
                var countAL = 0;
                var strTmp = string.Empty;
                countGA = Utilities.CountAnimations(lstAnimInfo);
#if DEBUG
                Utilities.SaveAnimInfo(lstAnimInfo);
#endif
                swapAnimationMapping =
                    [];

                _gameAnimations = lstAnimInfo;

                if (animationDict.Count > 0)
                {
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
                            Log.Level(LogLevel.Error, $"0009: No donor found: " +
                                $"mode={anim.Mode} DonorPoseId={anim.DonorPoseId} " +
                                $"animation={anim.AnimationName}");
                            continue;
                        }
                        //
                        // There are cases where one Id works for the female but not the
                        // male and vice-versa
                        //
                        // Female donor
#if KK
                        // Don't touch KK for now treat NeckDonorId like previous version
                        // only apply it to the female
                        if (anim.NeckDonorId >= 0 && anim.NeckDonorId != anim.DonorPoseId)
                        {
                            // PR #23 Change to Log.Level to always show log,
                            // update log ID's use temp variable to add log to log list
                            var newNeckDonor = animListInfo
                                .FirstOrDefault(x => x.id == anim.NeckDonorId);
                            if (newNeckDonor == null)
                            {
                                strTmp = $"0029: Invalid or missing " +
                                    $"NeckDonorId: mode={anim.Mode} " +
                                    $"NeckDonorId={anim.NeckDonorId}";
                                Log.Level(LogLevel.Warning, strTmp);
                                addedAnimations.Append(strTmp);
                            }
                            else
                            {
                                var newMotionNeck =
                                    newNeckDonor?.paramFemale?.fileMotionNeck;
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
#else
                        if (anim.NeckDonorIdFemale >= 0 && anim.NeckDonorIdFemale != anim.DonorPoseId)
                        {
                            // PR #23 Change to Log.Level to always show log, update log ID's
                            // use temp variable to add log to log list
                            var newNeckDonor = animListInfo
                                .FirstOrDefault(x => x.id == anim.NeckDonorIdFemale);
                            if (newNeckDonor == null)
                            {
                                strTmp = $"0029: Invalid or missing " +
                                    $"NeckDonorId: mode={anim.Mode} " +
                                    $"NeckDonorId={anim.NeckDonorIdFemale} " +
                                    $"Animation={anim.AnimationName}";
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
                                        $"mode={anim.Mode} NeckDonorId={anim.NeckDonorIdFemale} " +
                                        $"Animation={anim.AnimationName}";
                                    Log.Level(LogLevel.Warning, strTmp);
                                    addedAnimations.Append(strTmp);
                                }
                                else
                                {
                                    donorInfo.paramFemale.fileMotionNeck = newMotionNeck;
                                }
                            }
                        }
#endif
                        // Female1 donor
                        if (anim.ControllerFemale1 != null)
                        {
                            if (anim.NeckDonorIdFemale1 >= 0
                                && anim.NeckDonorIdFemale1 != anim.DonorPoseId)
                            {
                                var newNeckDonor = animListInfo
                                    .FirstOrDefault(x => x.id == anim.NeckDonorIdFemale1);
                                if (newNeckDonor == null)
                                {
                                    strTmp = $"0029: Invalid or missing " +
                                        $"NeckDonorId: mode={anim.Mode} " +
                                        $"NeckDonorId={anim.NeckDonorIdFemale1} " +
                                        $"Animation={anim.AnimationName}";
                                    Log.Level(LogLevel.Warning, strTmp);
                                    addedAnimations.Append(strTmp);
                                }
                                else
                                {
                                    var newMotionNeck =
                                        newNeckDonor?.paramFemale1?.fileMotionNeck;
                                    if (newMotionNeck == null)
                                    {
                                        strTmp = $"0030: NeckDonorId didn't point to" +
                                            $" a usable fileMotionNeck: " +
                                            $"mode={anim.Mode} " +
                                            $"NeckDonorId={anim.NeckDonorIdFemale1} " +
                                            $"Animation={anim.AnimationName}";
                                        Log.Level(LogLevel.Warning, strTmp);
                                        addedAnimations.Append(strTmp);
                                    }
                                    else
                                    {
                                        donorInfo.paramFemale1.fileMotionNeck =
                                            newMotionNeck;
                                    }
                                }
                            }
                        }

                        // Male donor did not have NeckDonor applied to it
                        // TODO: Treat NeckDonorId as global and apply it to everyone
                        // when there no specific one must remove from manifest
                        if (anim.NeckDonorIdMale >= 0
                            && anim.NeckDonorIdMale != anim.DonorPoseId)
                        {
                            var newNeckDonor = animListInfo
                                .FirstOrDefault(x => x.id == anim.NeckDonorIdMale);
                            if (newNeckDonor == null)
                            {
                                strTmp = $"0029B: Invalid or missing " +
                                    $"NeckDonorIdMale: mode={anim.Mode} " +
                                    $"NeckDonorIdMale={anim.NeckDonorIdMale} " +
                                    $"Animation={anim.AnimationName}";
                                Log.Level(LogLevel.Warning, strTmp);
                                addedAnimations.Append(strTmp);
                            }
                            else
                            {
                                var newMotionNeck = newNeckDonor?.paramMale?.fileMotionNeck;
                                if (newMotionNeck == null)
                                {
                                    strTmp = $"0030B: NeckDonorIdMale didn't point to" +
                                        $" a usable fileMotionNeck: " +
                                        $"mode={anim.Mode} " +
                                        $"NeckDonorIdMale={anim.NeckDonorIdMale} " +
                                        $"Animation={anim.AnimationName}";
                                    Log.Level(LogLevel.Warning, strTmp);
                                    addedAnimations.Append(strTmp);
                                }
                                else
                                {
                                    donorInfo.paramMale.fileMotionNeck = newMotionNeck;
                                }
                            }
                        }

                        if (anim.IsFemaleInitiative != null)
                        {
                            donorInfo.isFemaleInitiative = anim.IsFemaleInitiative.Value;
                        }

                        if (!string.IsNullOrEmpty(anim.FileSiruPaste))
                        {
                            // Check if FileSiruPaste is on dictionary first
                            if (SiruPasteFiles.TryGetValue(
                                anim.FileSiruPaste.ToLower(), out var fileSiruPaste))
                            {
                                donorInfo.paramFemale.fileSiruPaste = fileSiruPaste;
                            }
                            else
                            {
                                // TODO: Check that it is a valid name in
                                // anim.FileSiruPaste
                                donorInfo.paramFemale.fileSiruPaste =
                                    anim.FileSiruPaste.ToLower();
                            }
                        }

                        // Category
                        //     int category
                        //  string fileMove
                        donorInfo.lstCategory = anim.categories.Select(c =>
                        {
                            var cat = new HSceneProc.Category {
                                category = (int)c
                            };
                            return cat;
                        }).ToList();

                        // The mode and kindHoushi is not honored
                        if (anim.Mode == HFlag.EMode.houshi)
                        {
                            donorInfo.kindHoushi = (int)anim.kindHoushi;
                        }
#if KKS
                        // Update name so it shows on button text label correctly
                        donorInfo.nameAnimation = anim.AnimationName;
#endif
                        animListInfo.Add(donorInfo);
                        swapAnimationMapping[donorInfo] = anim;
                        countAL++;
                    }


                }

#if KKS
                _animationsUseStats.Init(lstAnimInfo);
#endif

                // log footer
                addedAnimations.Append($"\n{countAL + countGA} animations available: Game " +
                    $"standard = {countGA} " +
                    $"AnimationLoader = {countAL}\n");
#if DEBUG
                if (!once)
                {
#if KKS
                    Log.Warning($"0012: Added animations:\n{addedAnimations}\n\n" +
                        $"Stats:\n{_animationsUseStats}");
#else
                    Log.Warning($"0012: Added animations:\n{addedAnimations}");
#endif
                    var logLines = new StringBuilder();

                    foreach (var fj in _footJobAnimations)
                    {
                        logLines.AppendLine(fj.ToString());
                    }
                    Log.Warning($"0012: Foot jobs.\n{logLines}");
                    once = true;
                }
#else
                // For release log animations added
                if (!once)
                {
                    Log.Debug($"0012: Added animations:\n{addedAnimations}");
                    once = true;
                }
#endif
                }
            }
    }
}
