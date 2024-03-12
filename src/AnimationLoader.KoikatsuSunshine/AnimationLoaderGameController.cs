using System;

using UnityEngine;

using KKAPI.MainGame;

using static AnimationLoader.SwapAnim;

namespace AnimationLoader
{
    /// <summary>
    /// Save use animations.
    /// </summary>
    internal class AnimationLoaderGameController : GameCustomFunctionController
    {
        protected override void OnEndH(MonoBehaviour proc, HFlag flags, bool vr)
        {
            if (flags.isFreeH)
            {
                return;
            }
 
            try 
            { 
                _usedAnimations.Save();
                _animationsUseStats.Save();
            }
            catch (Exception ex)
            {
                Log.Error($"0033: Error saving used animations - {ex}");
            }
        }
    }
}
