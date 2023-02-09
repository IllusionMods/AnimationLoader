//
// MotionIK
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using H;
using Illusion.Extensions;

using BepInEx.Logging;
using HarmonyLib;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        // Additional state names included in the 48 states animations
        private static readonly List<string> aSates = new()
            {
                "A_Idle",
                "A_Insert",
                "A_InsertIdle",
                "A_WLoop",
                "A_SLoop",
                "A_OLoop",
                "A_M_IN_Start",
                "A_M_IN_Loop",
                "A_WF_IN_Start",
                "A_WF_IN_Loop",
                "A_WS_IN_Start",
                "A_WS_IN_Loop",
                "A_WS_IN_A",
                "A_SF_IN_Start",
                "A_SF_IN_Loop",
                "A_SS_IN_Start",
                "A_SS_IN_Loop",
                "A_SS_IN_A",
                "A_IN_A",
                "A_M_OUT_Start",
                "A_M_OUT_Loop",
                "A_OUT_A",
                "A_Pull",
                "A_Drop"
            };

        /// <summary>
        ///
        /// This is valid for sonyu animations:
        /// 
        /// Add motion IK for animations that have another animation that works as
        /// a model. In some cases a 24 state animation can be expanded to 48 states.
        ///
        /// MotionIKDonor controls the loading of the motion IK model.
        ///
        /// When MotionIKDonor equals DonorPoseId it does nothing. When they are
        /// different it will load the model animation IK configuration and do any
        /// necessary adjustments
        ///
        /// TODO: Create custom TextAsset configurations (BIG maybe)
        /// 
        /// This is a humble start to work with motion IK. My knowledge of animations is
        /// VERY limited. Is mainly a POC right now.
        /// 
        /// </summary>
        /// <param name="hSceneProcInstance">HSceneProc object instance</param>
        /// <param name="swapAnimationInfo">current animation loaded from zipmod</param>
        /// <param name="nextAinmInfo">game animation been loaded</param>
        private static void SetupMotionIK(object hSceneProcInstance,
            SwapAnimationInfo swapAnimationInfo,
            HSceneProc.AnimationListInfo nextAinmInfo
            )
        {
            var motionIKFemale = swapAnimationInfo.MotionIKDonorFemale;
            var motionIKMale = swapAnimationInfo.MotionIKDonorMale;

            var hspTraverse = Traverse.Create(hSceneProcInstance);
            var lstFemale = hspTraverse.Field<List<ChaControl>>("lstFemale").Value;
            var female = lstFemale[0];
            var female1 = ((lstFemale.Count > 1) ? lstFemale[1] : null);
            var male = hspTraverse.Field<ChaControl>("male").Value;
            var mi = hspTraverse.Field<List<MotionIK>>("lstMotionIK").Value;

            var motionIKDonor = -2;
            var clearMotionIK = true;

            // If MotionIKDonor is a number then MotionIKDonor equals DonorPoseId
            if (swapAnimationInfo.MotionIKDonor != null)
            {
                if (!int.TryParse(swapAnimationInfo.MotionIKDonor, out motionIKDonor))
                {
                    motionIKDonor = -2;
                }
            }

            if (MotionIK.Value)
            {
                // This are set when MotionIKDataDonor is not equal to DonorPoseId
                if (motionIKFemale != null || motionIKMale != null)
                {
                    // Copy motionIK data from suitable animation
                    if (motionIKFemale is not null)
                    {
                        var path = motionIKFemale;
                        var textAsset = GlobalMethod.LoadAllFolderInOneFile<TextAsset>("h/list/", path);

                        var len = mi[0].data.states.Length;
                        var moIK = new MotionIK(female);
                        var moIKA = new MotionIK(female);

                        moIK.LoadData(textAsset);
                        if (moIK.data.states.Length < len)
                        {
                            // sonyu have 24 or 48 states when motion IK model is loaded
                            // with 24 states for a 48 states animation load a second
                            // copy for the bottom 24. Unable to do a copy by value with
                            // other methods
                            moIKA.LoadData(textAsset);
                        }

                        Log.Level(LogLevel.Warning, $"[SwapAnimation] MotionIK female mi[0] total {len}" +
                            $" moIK total {moIK.data.states.Length} {path}\n");


                        if (moIK.data.states != null)
                        {
                            var loadLen = moIK.data.states.Length;

                            for (var i = 0; i < moIK.data.states.Length; i++)
                            {
                                mi[0].data.states[i] = moIK.data.states[i];
                                if (loadLen < len)
                                {
                                    // copy to additional states for short loaded motion IK data
                                    mi[0].data.states[i + 24] = moIKA.data.states[i];
                                    mi[0].data.states[i + 24].name = aSates[i];
                                }
                            }
                        }
                        else
                        {
                            mi[0].Release();
                        }

                    }

                    if (motionIKMale is not null)
                    {
                        var path = motionIKMale;
                        var textAsset = GlobalMethod.LoadAllFolderInOneFile<TextAsset>("h/list/", path);

                        var len = mi[1].data.states.Length;
                        var moIK = new MotionIK(male);
                        var moIKA = new MotionIK(female);

                        moIK.LoadData(textAsset);
                        moIKA.LoadData(textAsset);

                        Log.Level(LogLevel.Warning, $"[SwapAnimation] MotionIK male mi[1] total {len} " +
                            $"moIK total {moIK.data.states.Length} {path}\n");

                        if (moIK.data.states != null)
                        {
                            var loadLen = moIK.data.states.Length;
                            for (var i = 0; i < loadLen; i++)
                            {
                                mi[1].data.states[i] = moIK.data.states[i];
                                if (loadLen < len)
                                {
                                    // copy to additional states for short loaded motion IK data
                                    mi[1].data.states[i + 24] = moIKA.data.states[i];
                                    mi[1].data.states[i + 24].name = aSates[i];
                                }
                            }
                        }
                        else
                        {
                            mi[1].Release();
                        }

                    }

                    mi.Where((MotionIK motionIK) => motionIK.ik != null)
                        .ToList()
                        .ForEach(delegate (MotionIK motionIK) { motionIK.Calc("Idle"); }
                        );

                    /*mi.ForEach(mik =>
                    {
                        mik.SetPartners(mi);
                        mik.Reset();
                        mik.Calc("Idle");
                    });*/

                    clearMotionIK = false;
                }
            }

            if ( clearMotionIK )
            {
                // If true clear motion IK information if false use the motion IK from
                // DonorPoseId
                if (motionIKDonor != nextAinmInfo.id)
                {
                    mi.ForEach(mik => mik.Release());
                    mi.Clear();

                    mi.Add(new MotionIK(female));
                    mi.Add(new MotionIK(male));
                    mi.ForEach(mik =>
                        {
                            mik.SetPartners(mi);
                            mik.Reset();
                        }
                    );
                }
            }
        }
    }
}
