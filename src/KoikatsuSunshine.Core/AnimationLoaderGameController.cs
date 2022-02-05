﻿using System;

using KKAPI.MainGame;
using UnityEngine;

using static AnimationLoader.SwapAnim;

namespace AnimationLoader
{
    internal class AnimationLoaderGameController : GameCustomFunctionController
    {
        protected override void OnEndH(MonoBehaviour proc, HFlag flags, bool vr)
        {
            if (!vr)
            {
                if (flags.isFreeH)
                {
                    return;
                }
            }

            try 
            { 
                _usedAnimations.Save();
            }
            catch (Exception ex)
            {
                Log.Error($"0033: Error saving used animations - {ex}");
            }
        }
    }
}
