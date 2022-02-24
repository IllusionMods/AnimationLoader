using System;
using System.Collections.Generic;

using UnityEngine;

using H;

using HarmonyLib;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal partial class Hooks
        {
            /// <summary>
            /// Set the new original position when changing positions
            /// </summary>
            /// <param name="_nextAinmInfo"></param>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeAnimator))]
            private static void ChangeAnimatorPrefix(
                object __instance,
                HSceneProc.AnimationListInfo _nextAinmInfo)
            {
                if (_nextAinmInfo == null)
                {
                    return;
                }

                try
                {
#if DEBUG
                    Log.Warning($"0007: Animator changing - [{Manager.Scene.ActiveScene.name}] - " +
                        $"{AnimationInfo.TranslateName(_nextAinmInfo)}, " +
                        $"Key={AnimationInfo.GetKey(_nextAinmInfo)}, " +
                        $"SiruPaste={SiruPaste(_nextAinmInfo.paramFemale.fileSiruPaste)}.");
#endif
                    // Reposition characters before animation starts
                    if (Reposition.Value)
                    {                        
                        var flags = Traverse
                            .Create(__instance)
                            .Field<HFlag>("flags").Value;
                        Utilities.SetOriginalPositionAll();
                        var nowAnimationInfo = flags.nowAnimationInfo;
                        var nowAnim = new AnimationInfo(nowAnimationInfo);
                        if (nowAnim != null)
                        {
                            // If there is a position adjustment
                            // Reset position for new animation in same HPoint
                            if (Utilities.HasMovement(nowAnim))
                            {
                                if (nowAnim.SwapAnim.PositionHeroine != Vector3.zero)
                                {
                                    if (!Utilities.IsNewPosition(_heroine))
                                    {
                                        GetMoveController(_heroine).ResetPosition();
                                    }
                                }
                                if (nowAnim.SwapAnim.PositionPlayer != Vector3.zero)
                                {
                                    if (!Utilities.IsNewPosition(_player))
                                    {
                                        GetMoveController(_player).ResetPosition();
                                    }
                                }
                            }
                        }
                        var nextAnim = new AnimationInfo(_nextAinmInfo);
                        if (nextAnim != null)
                        {
                            // Move characters
                            if (Utilities.HasMovement(nextAnim))
                            {
                                if (nextAnim.SwapAnim.PositionHeroine != Vector3.zero)
                                {
                                    GetMoveController(_heroine)
                                        .Move(nextAnim.SwapAnim.PositionHeroine);
                                }
                                if (nextAnim.SwapAnim.PositionPlayer != Vector3.zero)
                                {
                                    GetMoveController(_player)
                                        .Move(nextAnim.SwapAnim.PositionPlayer);
                                }
                            }
#if KKS
                            // Save used animation
                            if (nextAnim.IsAnimationLoader())
                            {
                                _usedAnimations.Keys.Add(nextAnim.Key);
                            }
#endif
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"0008: Error={e}");
                }
            }

            internal static Func<string, string> SiruPaste = x => x == string.Empty ?
                $"None" : $"{x}";

            /// <summary>
            /// Swap animation if found in mapping dictionary
            /// </summary>
            /// <param name="__instance"></param>
            /// <param name="_nextAinmInfo"></param>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeAnimator))]
            private static void SwapAnimation(
                object __instance,
                HSceneProc.AnimationListInfo _nextAinmInfo)
            {
                if (!swapAnimationMapping.TryGetValue(_nextAinmInfo, out var swapAnimationInfo))
                {
                    return;
                }

                RuntimeAnimatorController femaleCtrl = null;
                RuntimeAnimatorController maleCtrl = null;
                if (!string.IsNullOrEmpty(swapAnimationInfo.PathFemale)
                    || !string.IsNullOrEmpty(swapAnimationInfo.ControllerFemale))
                {
                    femaleCtrl = AssetBundleManager.LoadAsset(
                        swapAnimationInfo.PathFemale,
                        swapAnimationInfo.ControllerFemale,
                        typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
                }
                if (!string.IsNullOrEmpty(swapAnimationInfo.PathMale)
                    || !string.IsNullOrEmpty(swapAnimationInfo.ControllerMale))
                {
                    maleCtrl = AssetBundleManager.LoadAsset(
                        swapAnimationInfo.PathMale,
                        swapAnimationInfo.ControllerMale,
                        typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
                }
                var t_hsp = Traverse.Create(__instance);
                var female = t_hsp.Field<List<ChaControl>>("lstFemale").Value[0];
                var male = t_hsp.Field<ChaControl>("male").Value;
                ////TODO: lstFemale[1], male1

                if (femaleCtrl != null)
                {
                    female.animBody.runtimeAnimatorController = SetupAnimatorOverrideController(
                        female.animBody.runtimeAnimatorController,
                        femaleCtrl);
                }
                if (maleCtrl != null)
                {
                    male.animBody.runtimeAnimatorController = SetupAnimatorOverrideController(
                        male.animBody.runtimeAnimatorController, maleCtrl);
                }
                var mi = t_hsp.Field<List<MotionIK>>("lstMotionIK").Value;

                Log.Warning($"MotionIk for {swapAnimationInfo.StudioId} - " +
                    $"{swapAnimationInfo.MotionIKDonor} donor {_nextAinmInfo.id}");
                if (swapAnimationInfo.MotionIKDonor != _nextAinmInfo.id)
                {
                    mi.ForEach(mik => mik.Release());
                    mi.Clear();

                    //TODO: MotionIKData.
                    mi.Add(new MotionIK(female));
                    mi.Add(new MotionIK(male));
                    mi.ForEach(mik =>
                    {
                        mik.SetPartners(mi);
                        mik.Reset();
                    });
                }
                else
                {
                    Log.Warning($"Testing MotionIk for {swapAnimationInfo.StudioId}-" +
                        $"{swapAnimationInfo.AnimationName} donor " +
                        $"{swapAnimationInfo.MotionIKDonor}");
                }
            }

            /// <summary>
            /// Debug only method
            /// </summary>
            private static void ChangeCategoryPostfix(HPointData _data, int _category)
            {
                Log.Warning(
                    $"0021: HPoint mode={(PositionCategory)_category} " +
                    $"name={_data.name} " +
                    $"Experience={_data.Experience} " +
                    $"position {_data.transform.position}");
            }
        }
    }
}
