//
// MotionIK
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnityEngine;

using H;
using Illusion.Extensions;

using BepInEx.Logging;
using HarmonyLib;

using Newtonsoft.Json;



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
            var hspTraverse = Traverse.Create(hSceneProcInstance);
            var lstMotionIK = hspTraverse.Field<List<MotionIK>>("lstMotionIK").Value;
            var lstFemale = hspTraverse.Field<List<ChaControl>>("lstFemale").Value;
            var female = lstFemale[0];
            var female1 = ((lstFemale.Count > 1) ? lstFemale[1] : null);
            var male = hspTraverse.Field<ChaControl>("male").Value;
            var flags = hspTraverse.Field<HFlag>("flags").Value;

            var justClear = !MotionIK.Value;

#if KK
            justClear = true;
#endif

            if (justClear)
            {
#if DEBUG
                Log.Level(LogLevel.Warning, "[SetupMotionIK] Clearing motion IK setup " +
                    "is disabled.");
#endif
                lstMotionIK.ForEach(mik => mik.Release());
                lstMotionIK.Clear();

                lstMotionIK.Add(new MotionIK(female));
                lstMotionIK.Add(new MotionIK(male));
                if (female1 != null)
                {
                    lstMotionIK.Add(new MotionIK(female1));
                }
                lstMotionIK.ForEach(mik =>
                    {
                        mik.SetPartners(lstMotionIK);
                        mik.Reset();
                    }
                );
                return;
            }

            var motionIKFemale = swapAnimationInfo.MotionIKDonorFemale;
            var motionIKMale = swapAnimationInfo.MotionIKDonorMale;

            var motionIKDonor = -2;
            var clearMotionIK = true;
#if DEBUG
            Log.Level(LogLevel.Warning, $"[SwapAnimation] MotionIK female is " +
                $"null={motionIKFemale == null} MotionIK male is " +
                $"null={motionIKMale == null} enabled={MotionIK.Value}");
#endif
            // If MotionIKDonor is a number then MotionIKDonor equals DonorPoseId
            if (swapAnimationInfo.MotionIKDonor != null)
            {
                if (!int.TryParse(swapAnimationInfo.MotionIKDonor, out motionIKDonor))
                {
                    motionIKDonor = -2;
                }
            }

            if (MotionIK.Value && (flags.mode == HFlag.EMode.sonyu))
            {
                // This are set when MotionIKDataDonor is not equal to DonorPoseId
                if (motionIKFemale != null || motionIKMale != null)
                {
                    string path;
                    TextAsset textAsset;
                    MotionIK motionIK = null;
                    MotionIKData motionIKData = null;
                    MotionIK additionalMotionIK = null;
                    MotionIKData additionalMotionIKData = null;
                    int totalDonorPoseIdStates;
                    int totalMotionDonorStates;
                    var dataFound = false;

                    // Copy motionIK data from suitable animation
                    if (motionIKFemale is not null)
                    {
                        dataFound = false;
                        path = motionIKFemale;
                        textAsset = GlobalMethod
                            .LoadAllFolderInOneFile<TextAsset>("h/list/", path);
                        totalDonorPoseIdStates = lstMotionIK[0].data.states.Length;

                        if (textAsset != null)
                        {
                            motionIK = new MotionIK(female);
                            additionalMotionIK = new MotionIK(female);

                            motionIK.LoadData(textAsset);
                            motionIKData = motionIK.data;
                            if (motionIKData.states.Length < totalDonorPoseIdStates)
                            {
                                // sonyu have 24 or 48 states when motion IK model is loaded
                                // with 24 states for a 48 states animation load a second
                                // copy for the bottom 24. Unable to do a copy by value with
                                // other methods
                                additionalMotionIK.LoadData(textAsset);
                                additionalMotionIKData = additionalMotionIK.data;
                            }
                            dataFound = true;
                        }
                        else
                        {
#if DEBUG
                            Log.Level(LogLevel.Warning, $"[SwapAnimation] Found JsonFile " +
                                $"{path}.");
#endif
                            motionIKData = ReadJsonFile(motionIKFemale);
                            if (motionIKData != null)
                            {
                                dataFound = true;
                            }
                        }

                        if (dataFound)
                        { 
#if DEBUG
                            Log.Level(LogLevel.Warning, $"[SwapAnimation] MotionIK female " +
                                $"mi[0] total {totalDonorPoseIdStates} motionIKData total " +
                                $"{motionIKData?.states.Length} " +
                                $"{path}.");
#endif
                            if (motionIKData?.states != null)
                            {
                                totalMotionDonorStates = motionIKData.states.Length;

                                for (var i = 0; i < motionIKData.states.Length; i++)
                                {
                                    lstMotionIK[0].data.states[i] = motionIKData.states[i];
                                    if (totalMotionDonorStates < totalDonorPoseIdStates)
                                    {
                                        // copy to additional states for short loaded
                                        // motion IK data
                                        if (additionalMotionIKData != null)
                                        {
                                            lstMotionIK[0].data.states[i + 24] =
                                                additionalMotionIKData?.states[i];
                                            lstMotionIK[0].data.states[i + 24].name = aSates[i];
#if DEBUG
                                            Log.Warning($"Nena Additional " +
                                                $"name={additionalMotionIKData?.states[i].name} " +
                                                $"name={aSates[i]} for index={i + 24}");
#endif
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Log.Debug($"[SwapAnimation] Animation states can't be " +
                                    $"matched for TextAsset {motionIKFemale}.");
                                lstMotionIK[0] = new MotionIK(female);
                            }
                        }
                        else
                        {
                            Log.Debug($"[SwapAnimation] TextAsset {motionIKFemale} not " +
                                $"found.");
                            lstMotionIK[0] = new MotionIK(female);
                        }   
                    }
                    else
                    {
#if DEBUG
                        Log.Level(LogLevel.Warning, $"[SwapAnimation] MotionIK female " +
                            $"reset.");
#endif
                        lstMotionIK[0] = new MotionIK(female);
                    }

                    if (motionIKMale is not null)
                    {
                        dataFound = false;
                        path = motionIKMale;
                        textAsset = GlobalMethod
                            .LoadAllFolderInOneFile<TextAsset>("h/list/", path);
                        totalDonorPoseIdStates = lstMotionIK[1].data.states.Length;

                        if (textAsset != null)
                        {
#if DEBUG
                            Log.Level(LogLevel.Warning, $"[SwapAnimation] Found TextAsset " +
                                $"{path}.");
#endif
                            motionIK = new MotionIK(male);
                            additionalMotionIK = new MotionIK(male);

                            motionIK.LoadData(textAsset);
                            motionIKData = motionIK.data;

                            if (motionIKData.states.Length < totalDonorPoseIdStates)
                            {
                                // sonyu have 24 or 48 states when motion IK model is loaded
                                // with 24 states for a 48 states animation load a second
                                // copy for the bottom 24. Unable to do a copy by value with
                                // other methods
                                additionalMotionIK.LoadData(textAsset);
                                additionalMotionIKData = additionalMotionIK.data;
                            }
                            dataFound = true;
                        }
                        else
                        {
#if DEBUG
                            Log.Level(LogLevel.Warning, $"[SwapAnimation] Found JsonFile " +
                                $"{path}.");
#endif
                            motionIKData = ReadJsonFile(motionIKMale);
                            if (motionIKData != null)
                            {
                                dataFound = true;
                            }
                        }

                        if (dataFound)
                        {
#if DEBUG
                            Log.Level(LogLevel.Warning, $"[SwapAnimation] MotionIK male " +
                                $"mi[1] total {totalDonorPoseIdStates} motionIKData " +
                                $"total {motionIKData?.states.Length} {path}.");
#endif
                            if (motionIKData?.states != null)
                            {
                                totalMotionDonorStates = motionIKData.states.Length;
                                for (var i = 0; i < totalMotionDonorStates; i++)
                                {
                                    lstMotionIK[1].data.states[i] = motionIKData.states[i];
                                    if (totalMotionDonorStates < totalDonorPoseIdStates)
                                    {
                                        // copy to additional states for short loaded
                                        // motion IK data
                                        if (additionalMotionIKData != null)
                                        {
                                            lstMotionIK[1].data.states[i + 24] =
                                                additionalMotionIKData?.states[i];
                                            lstMotionIK[1].data.states[i + 24].name =
                                                aSates[i];
#if DEBUG
                                            Log.Warning($"Nene Additional " +
                                                $"name={additionalMotionIKData?.states[i].name} " +
                                                $"name={aSates[i]} for index={i + 24}");
#endif
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Log.Debug($"[SwapAnimation] Animation states can't be " +
                                    $"matched for TextAsset {motionIKMale}.");
                                lstMotionIK[1] = new MotionIK(male);
                            }
                        }
                        else
                        {
                            Log.Debug($"[SwapAnimation] TextAsset {motionIKMale} not " +
                                $"found.");
                            lstMotionIK[1] = new MotionIK(male);
                        }
                    }
                    else
                    {
#if DEBUG
                        Log.Level(LogLevel.Warning, $"[SwapAnimation] MotionIK male reset.");
#endif
                        lstMotionIK[1] = new MotionIK(male);
                    }

                    lstMotionIK.Where((MotionIK motionIK) => motionIK.ik != null)
                        .ToList()
                        .ForEach(delegate (MotionIK motionIK) { motionIK.Calc("Idle"); }
                        );

                    /*lstMotionIK.ForEach(mik =>
                    {
                        mik.SetPartners(lstMotionIK);
                        mik.Reset();
                        mik.Calc("Idle");
                    });

                    lstMotionIK.ForEach(mik =>
                    {
                        mik.SetPartners(lstMotionIK);
                        mik.Reset();
                        mik.Calc("Idle");
                    });
                    */

                    clearMotionIK = false;
                }
            }

            if ( clearMotionIK )
            {
                // If true clear motion IK information
                // If false keep motion IK from DonorPoseId
                if (motionIKDonor != nextAinmInfo.id)
                {
#if DEBUG
                    Log.Level(LogLevel.Warning, "[SetupMotionIK] Clearing motion IK.");
#endif
                    lstMotionIK.ForEach(mik => mik.Release());
                    lstMotionIK.Clear();

                    lstMotionIK.Add(new MotionIK(female));
                    lstMotionIK.Add(new MotionIK(male));
                    if (female1 != null)
                    {
                        lstMotionIK.Add(new MotionIK(female1));
                    }
                    lstMotionIK.ForEach(mik =>
                        {
                            mik.SetPartners(lstMotionIK);
                            mik.Reset();
                        }
                    );
                }
            }
        }

        /// <summary>
        /// Read IK data from json file
        /// </summary>
        /// <param name="strFile">file name</param>
        /// <returns></returns>
        public static MotionIKData ReadJsonFile(string strFile)
        {
            var rootPath = Path.Combine(UserData.Path, "AnimationLoader/MotionIK");
            var rootDirectory = new DirectoryInfo(rootPath);

            try
            {
                var files = rootDirectory.GetFiles("*.json", SearchOption.AllDirectories);

                var fileName = strFile;
                string stem;
#if DEBUG
                Log.Warning($"[ReadJsonFile] Name={fileName}");
#endif
                foreach (var f in files)
                {
                    stem = Path.GetFileNameWithoutExtension(f.Name);
                    if (stem == fileName)
                    {
                        using var file = File.OpenText(f.FullName);

                        var serializer = new JsonSerializer();
                        var motionIK = (MotionIKDataSerializable)serializer
                            .Deserialize(file, typeof(MotionIKDataSerializable));
                        if (motionIK != null)
                        {
                            return motionIK.MotionIKData();
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning($"ReadJsonFile: File={strFile} Error={e.Message}");
            }
            return null;
        }

        /// <summary>
        /// Read IK data from json file for the specific state
        /// </summary>
        /// <param name="strFile">file name</param>
        /// <param name="state">state name</param>
        /// <returns></returns>
        public static MotionIKData.State ReadJsonFile(string strFile, string state = "")
        {
            var rootPath = Path.Combine(UserData.Path, "AnimationLoader/MotionIK");
            var rootDirectory = new DirectoryInfo(rootPath);

            try
            {
                var files = rootDirectory.GetFiles("*.json", SearchOption.AllDirectories);
                var fileName = strFile + (state == "" ? "" : $"-{state}");
                string stem;
#if DEBUG
                Log.Warning($"[ReadJsonFile.State] Name={fileName}");
#endif
                foreach (var f in files)
                {
                    stem = Path.GetFileNameWithoutExtension(f.Name);
                    if (stem == fileName)
                    {
                        using var file = File.OpenText(f.FullName);

                        var serializer = new JsonSerializer();
                        var motionIK = (MotionIKDataSerializable.State)serializer
                            .Deserialize(file, typeof(MotionIKDataSerializable.State));
                        if (motionIK != null)
                        {
                            return motionIK.ToState();
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Log.Warning($"State.ReadJsonFile: File={strFile} Error={e.Message}");
            }
            return null;
        }

    }
}
