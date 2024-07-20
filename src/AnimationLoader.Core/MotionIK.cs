//
// MotionIK
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BepInEx.Logging;
using HarmonyLib;

// Depends on XUnity.AutoTranslator copy TODO: study using MessagePack for distribution
using Newtonsoft.Json;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
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

            // Is MotionIK setup disabled?
            var justClear = !MotionIK.Value;

#if KK
            // Hard disable for KK TODO: Test for KK
            // justClear = true;
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

            // Only use Json files this way should work on KK (TODO: Test this.)
            if (MotionIK.Value && (flags.mode == HFlag.EMode.sonyu))
            {
                // This are set when MotionIKDataDonor is not equal to DonorPoseId
                if (motionIKFemale != null || motionIKMale != null)
                {
                    string path;
                    MotionIKData motionIKData = null;
                    int totalDonorPoseIdStates;
                    int totalMotionDonorStates;
                    var dataFound = false;

                    // Copy motionIK data from suitable animation
                    if (motionIKFemale is not null)
                    {
                        dataFound = false;
                        totalDonorPoseIdStates = lstMotionIK[0].data.states.Length;
                        path = motionIKFemale;

                        motionIKData = ReadJsonFile(motionIKFemale);
                        if (motionIKData != null)
                        {
                            
                            dataFound = true;
#if DEBUG
                            Log.Level(LogLevel.Warning, $"[SwapAnimation] Found JsonFile " +
                                $"{path}. States=[{motionIKData?.states.Length}].");
#endif
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
                        totalDonorPoseIdStates = lstMotionIK[1].data.states.Length;
                        path = motionIKMale;
                        motionIKData = ReadJsonFile(motionIKMale);
                        if (motionIKData != null)
                        {
                            dataFound = true;
#if DEBUG
                            Log.Level(LogLevel.Warning, $"[SwapAnimation] Found JsonFile " +
                                    $"{path}. States=[{motionIKData?.states.Length}].");
#endif
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

                    if (dataFound)
                    {
                        clearMotionIK = false;

                        try
                        {
                            // Causing problems when Json file were not found.
                            lstMotionIK.Where((MotionIK motionIK) => motionIK.ik != null)
                                .ToList()
                                .ForEach(delegate (MotionIK motionIK) { motionIK.Calc("Idle"); }
                                );
                        }
                        catch
                        {
                            // Error making IK calculations clear configuration
                            clearMotionIK = true;
                        }
                    }
                    else
                    {
                        // Set lstMotionIK to empty configuration
                        clearMotionIK = true;
                    }
                }
            }

            if ( clearMotionIK )
            {
                // Clear the motion IK configuration from the donor animation
                if (motionIKDonor != nextAinmInfo.id)
                {
#if DEBUG
                    Log.Level(LogLevel.Warning, "[SetupMotionIK] Clearing motion IK " +
                        "configuration.");
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
