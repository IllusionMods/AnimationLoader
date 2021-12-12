using System;
using System.Text;

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
                    Logger.LogWarning($"Key {AnimationKey(_nextAinmInfo)} Animator mode" +
                        $" {_nextAinmInfo.mode} ID: {_nextAinmInfo.id} name" +
                        $" {Utilities.Translate(_nextAinmInfo.nameAnimation)} - " +
                        $"Asset {_nextAinmInfo.pathFemaleBase.assetpath} restriction " +
                        $"{_nextAinmInfo.stateRestriction}");
#endif
                }
                catch (Exception e)
                {
                    Logger.LogError($"Error - {e}");
                }
            }

            private static string AnimationKey(HSceneProc.AnimationListInfo animation)
            {
                string key;

                swapAnimationMapping.TryGetValue(animation, out var anim);
                if (anim != null)
                {
                    key = $"{anim.Guid} {animation.mode} {anim.StudioId}";
                }
                else
                {
                    key = $"gameAnimation {animation.mode} {animation.id}";
                }
                return key;
            }
        }
    }
}
