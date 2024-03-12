using System;
using System.Collections.Generic;

using UnityEngine;

using KKAPI.MainGame;

#if DEBUG
using BepInEx.Logging;
#endif
using HarmonyLib;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal enum State { On = 0, Shift = 1, Hang = 2, Off = 3 }

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
                // Temporarily disable movement in KK TODO: Get start position without
                // using the characters current position to get the value.
                if (_nextAinmInfo == null)
                {
                    return;
                }

                if (_heroine == null)
                {
                    return;
                }

                var heroine = GameAPI.GetCurrentHeroine();
                var animationKey = GetAnimationKey(_nextAinmInfo);

                // Get shoes off for footjob animations in
                // <Game>/BepInEx/config/AnimationLoader/FootJob/FootJobAnimations.xml
                // TODO: Check in KK outside
                if ((heroine != null) && _footJobAnimations.Contains(animationKey))
                {
                    heroine.chaCtrl.SetClothesState(
                        (int)ChaFileDefine.ClothesKind.shoes_inner,
                        (byte)State.Off);
                    Log.Debug("0019: [ChangeAnimatorPrefix] Taking shoes off.");
                }
#if DEBUG
                if (heroine == null)
                {
                    Log.Debug("0019: [ChangeAnimatorPrefix] Heroine null.");
                }
#endif
                // This is not an error what I was thinking?
                //if (!_footJobAnimations.Contains(animationKey))
                //{
                //    Log.Debug($"0019: [ChangeAnimatorPrefix] key={animationKey} name={_nextAinmInfo.nameAnimation} not found.");
                //}
#if KKS
                try
                {
                    var hspTraverse = Traverse.Create(__instance);
                    var flags = hspTraverse.Field<HFlag>("flags").Value;
                    var position = hspTraverse
                        .Field<Vector3>("nowHpointDataPos").Value;
                    var lstFemales = hspTraverse
                        .Field<List<ChaControl>>("lstFemale").Value;
                    var key = GetAnimationKey(_nextAinmInfo);
                    if (_animationsUseStats.Stats.ContainsKey(key))
                    {
                        _animationsUseStats.Stats[key] += 1;
                    }
#if DEBUG
                    var nowAnimName = "None";
                    if (flags.nowAnimationInfo != null)
                    {
                        nowAnimName = Utilities
                            .TranslateName(flags.nowAnimationInfo.nameAnimation);
                    }

                    Log.Warning($"0007: [ChangeAnimatorPrefix] Animator changing - " +
                        $"[{Manager.Scene.ActiveScene.name}]\n" +
                        $"Now Animation {nowAnimName} " +
                        $"Animation {Utilities.TranslateName(_nextAinmInfo.nameAnimation)} " +
                        $"Key={GetAnimationKey(_nextAinmInfo)}  " +
                        $"SiruPaste={SiruPaste(_nextAinmInfo.paramFemale.fileSiruPaste)}\n" +
                        $"nowHpointDataPos={position.Format()}");
#endif
                    // Reposition characters before animation starts
                    if (Reposition.Value)
                    {
                        var nowAnimationInfo = flags.nowAnimationInfo;
                        var nowAnim = new AnimationInfo(flags.nowAnimationInfo);
                        // var heroinePos = GetMoveController(_heroine).ChaControl.transform.position;
                        // Cannot use _heroine this call is prior to HSceneProc.SetShortcut
                        // _heroine is still null
                        var heroinePos = lstFemales[0].transform.position;

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
                                // The position of characters as set by the current
                                // animation pose
                                position = hspTraverse
                                    .Field<Vector3>("nowHpointDataPos").Value;

                                Utilities.SetOriginalPositionAll(position);
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
                            // Save used animation
                            if (nextAnim.IsAnimationLoader)
                            {
                                _usedAnimations.Keys.Add(nextAnim.Key);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"0008: Error={e}");
                }
#endif
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
                RuntimeAnimatorController female1Ctrl = null;
                RuntimeAnimatorController maleCtrl = null;

                if (!string.IsNullOrEmpty(swapAnimationInfo.PathFemale)
                    || !string.IsNullOrEmpty(swapAnimationInfo.ControllerFemale))
                {
                    femaleCtrl = AssetBundleManager.LoadAsset(
                        swapAnimationInfo.PathFemale,
                        swapAnimationInfo.ControllerFemale,
                        typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
                }

                // Third wheel
                if (!string.IsNullOrEmpty(swapAnimationInfo.PathFemale1)
                    || !string.IsNullOrEmpty(swapAnimationInfo.ControllerFemale1))
                {
                    female1Ctrl = AssetBundleManager.LoadAsset(
                        swapAnimationInfo.PathFemale1,
                        swapAnimationInfo.ControllerFemale1,
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
                var lstFemale = t_hsp.Field<List<ChaControl>>("lstFemale").Value;
                var female = lstFemale[0];
                var female1 = ((lstFemale.Count > 1) ? lstFemale[1] : null);
                var male = t_hsp.Field<ChaControl>("male").Value;
                ////TODO: male1

                if (femaleCtrl != null)
                {
                    female.animBody.runtimeAnimatorController = SetupAnimatorOverrideController(
                        female.animBody.runtimeAnimatorController,
                        femaleCtrl);
                }
                if ((female1Ctrl != null)
                    && female1 != null)
                {
                    female1.animBody.runtimeAnimatorController = SetupAnimatorOverrideController(
                        female1.animBody.runtimeAnimatorController,
                        female1Ctrl);
                }
                if (maleCtrl != null)
                {
                    male.animBody.runtimeAnimatorController = SetupAnimatorOverrideController(
                        male.animBody.runtimeAnimatorController, maleCtrl);
                }
                // Temporarily disable in KK giving problem for some users TODO: More testing
                SetupMotionIK(__instance, swapAnimationInfo, _nextAinmInfo);
            }
        }
    }
}
