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
        internal static HSceneProc _hprocInstance;
        internal static ChaControl _heroine;
        internal static ChaControl _heroine3P;
        internal static List<ChaControl> _lstHeroines;
        internal static ChaControl _player;
        internal static string _animationKey;
        
        internal partial class Hooks
        {
            /// <summary>
            /// Set the new original position when changing positions not using the H point picker
            /// </summary>
            /// <param name="_nextAinmInfo"></param>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeAnimator))]
            private static void ChangeAnimatorPrefix(HSceneProc.AnimationListInfo _nextAinmInfo)
            {
                if (_nextAinmInfo == null)
                {
                    return;
                }

                try
                {
#if DEBUG
                    Logger.LogWarning($"0006: Animator changing - " +
                        $"{Utilities.TranslateName(_nextAinmInfo.nameAnimation)} " +
                        $"Key {AnimationInfo.GetKey(_nextAinmInfo)} " +
                        $"SiruPaste {_nextAinmInfo.paramFemale.fileSiruPaste}.");
#endif
                    Utilities.SetOriginalPositionAll();
                    // TODO: Look to fix this in the animation files.
                    // undo movement if still in save position
                    var nowAnimationInfo = _hprocInstance.flags.nowAnimationInfo;
                    var nowAnim = new AnimationInfo(nowAnimationInfo);
                    if (nowAnim != null)
                    {
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
                catch (Exception e)
                {
                    Logger.LogError($"0008: Error - {e}");
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.SetShortcutKey))]
            private static void SetShortcutKeyPrefix(
                object __instance,
                List<ChaControl> ___lstFemale,
                ChaControl ___male)
            {
                Utilities.SaveHProcInstance(__instance);
                _lstHeroines = ___lstFemale;
                _heroine = _lstHeroines[0];
                GetMoveController(_heroine).Init();

                if (___lstFemale.Count > 1)
                {
                    _heroine3P = _lstHeroines[1];
                }

                _player = ___male;
            }
        }
    }
}
