using System;

using HarmonyLib;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
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
                    swapAnimationMapping.TryGetValue(_nextAinmInfo, out var anim);
                    if (anim != null)
                    {
                        Logger.LogWarning($"Animator {anim.StudioId}");
                    }                    
                    Logger.LogWarning($"Animator mode {_nextAinmInfo.mode} ID: " +
                        $"{_nextAinmInfo.id} name {Utilities.Translate(_nextAinmInfo.nameAnimation)} - " +
                        $"Asset {_nextAinmInfo.pathFemaleBase.assetpath} restriction " +
                        $"{_nextAinmInfo.stateRestriction}");
#endif
                }
                catch (Exception e)
                {
                    Logger.LogError($"Error - {e}");
                }
            }

        }
    }
}
