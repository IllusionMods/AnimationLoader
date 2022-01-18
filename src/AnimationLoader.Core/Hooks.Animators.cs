using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using HarmonyLib;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        // TODO: Change to a CharaCustomFunctionController when logic works
        internal static object _hprocObjInstance;
        internal static ChaControl _heroine;
        internal static ChaControl _heroine3P;
        internal static List<ChaControl> _lstHeroines;
        internal static ChaControl _player;
        internal static HFlag _flags;
        
        internal partial class Hooks
        {
            /// <summary>
            /// Set the new original position when changing positions not using the H point picker
            /// </summary>
            /// <param name="_nextAinmInfo"></param>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeAnimator))]
            private static void ChangeAnimatorPrefix(object __instance, HSceneProc.AnimationListInfo _nextAinmInfo)
            {
                if (_nextAinmInfo == null)
                {
                    return;
                }

                try
                {
#if DEBUG
                    Log.Warning($"0007: Animator changing - " +
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
                                    GetMoveController(_heroine).Move(nextAnim.SwapAnim.PositionHeroine);
                                }
                                if (nextAnim.SwapAnim.PositionPlayer != Vector3.zero)
                                {
                                    GetMoveController(_player).Move(nextAnim.SwapAnim.PositionPlayer);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"0008: Error - {e}");
                }
            }

            internal static Func<string, string> SiruPaste = x => x == string.Empty ?
                $"None" : $"{x}";

            /// <summary>
            /// Initialize MoveController for characters
            /// </summary>
            /// <param name="__instance"></param>
            /// <param name="___lstFemale"></param>
            /// <param name="___male"></param>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.SetShortcutKey))]
            private static void SetShortcutKeyPrefix(
                object __instance,
                List<ChaControl> ___lstFemale,
                ChaControl ___male)
            {
                _hprocObjInstance = __instance;
                _lstHeroines = ___lstFemale;
                _heroine = _lstHeroines[0];
                GetMoveController(_heroine).Init();

                if (___lstFemale.Count > 1)
                {
                    _heroine3P = _lstHeroines[1];
                    GetMoveController(_heroine3P).Init();
                }

                _player = ___male;
                GetMoveController(_player).Init();

                _flags = Traverse
                    .Create(__instance)
                    .Field<HFlag>("flags").Value;
            }
        }
    }
}
