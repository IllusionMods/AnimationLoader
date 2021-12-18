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
        internal static HSceneProc _hprocInstance;
        internal static ChaControl _heroine;
        internal static ChaControl _heroine3P;
        internal static List<ChaControl> _lstHeroines;
        internal static ChaControl _player;

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
                    Logger.LogWarning($"0006: Animator changing - {_nextAinmInfo.nameAnimation}.");
                    var swapAnim = new AnimationInfo(_nextAinmInfo);
                    if (swapAnim != null)
                    {
                        Logger.LogWarning($"0007: Key {swapAnim.Key}");
                        if (swapAnim.SwapAnim != null)
                        {
                            if (swapAnim.SwapAnim.PositionHeroine != Vector3.zero)
                            {
                                MoveCharacter.Move(_heroine, swapAnim.SwapAnim.PositionHeroine);
                            }
                            if (swapAnim.SwapAnim.PositionPlayer != Vector3.zero)
                            {
                                MoveCharacter.Move(_player, swapAnim.SwapAnim.PositionPlayer);
                            }
                        }
                    }
#endif
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
                _hprocInstance = (HSceneProc)__instance;
                if (_hprocInstance == null)
                {
                    Logger.LogWarning($"0009: Failed to save _hprocInstance");
                }
                else
                {
                    Logger.LogWarning($"0010: _hprocInstance saved.");
                }
                _lstHeroines = ___lstFemale;
                _heroine = _lstHeroines[0];
                if (___lstFemale.Count > 1)
                {
                    _heroine3P = _lstHeroines[1];
                }

                _player = ___male;
            }
        }
    }
}
