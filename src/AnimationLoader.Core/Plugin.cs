using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using UnityEngine;

using BepInEx.Logging;

using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;

using static HFlag;
namespace AnimationLoader
{
    public partial class SwapAnim
    {
#if KK
        static readonly private Color buttonColor = new(0.96f, 1f, 0.9f);
#endif

/* Unmerged change from project 'AnimationLoader.Koikatu'
Before:
        private static Dictionary<EMode, List<SwapAnimationInfo>> animationDict;
        private static Dictionary<HSceneProc.AnimationListInfo, SwapAnimationInfo>
After:
        static private Dictionary<EMode, List<SwapAnimationInfo>> animationDict;
        static private Dictionary<HSceneProc.AnimationListInfo, SwapAnimationInfo>
*/
        static private Dictionary<EMode, List<SwapAnimationInfo>> animationDict;
        static private Dictionary<HSceneProc.AnimationListInfo, SwapAnimationInfo>
            swapAnimationMapping;
        static readonly private Type VRHSceneType = Type.GetType("VRHScene, Assembly-CSharp");
        

        static readonly private Dictionary<string, string> SiruPasteFiles = new() {
            { "", "" },
            { "butt", "siru_t_khs_n06" },
            { "facetits", "siru_t_khh_32" },
            { "facetitspussy", "siru_t_khh_32" }, // have to make this manually, for now copy FaceTits
            { "titspussy", "siru_t_khs_n07" },
            { "tits", "siru_t_khh_11" },
            { "pussy", "siru_t_khs_n07" }, // have to make this manually, for now copy TitsPussy
            { "kksbutt", "siru_t_khs_14" },
            { "kksfacetits", "siru_t_khh_33" },
        };

        static readonly private Dictionary<string, int> EModeGroups = new() {
            { "aibu1", 998 },
            { "houshi0", 999 },
            { "houshi1", 1000 },
            { "sonyu0", 1001 },
            { "sonyu1", 1002 },
            { "masturbation1", 1003 },
            { "peeping0", 1004 },
            { "peeping1", 1005 },
            { "lesbian1", 1006 },
        };

        private void Awake()
        {
            Log.LogSource = Logger; ;

            ConfigEntries();
#if KKS
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
            {
                if (!LoadInCharStudio.Value)
                {
                    Log.Level(LogLevel.Message, "0013: Animation Loader disabled in configuration.");
                    enabled = false;
                    return;
                }
            }
#endif
            Hooks.Init();
            // Register move characters controller
            CharacterApi.RegisterExtraBehaviour<MoveController>(GUID);
#if KKS
            // Read used animations
            _usedAnimations.Read();
            // To save used animations on H exit
            GameAPI.RegisterExtraBehaviour<AnimationLoaderGameController>(GUID);
#endif
        }

        private void Start()
        {
            //
            // Save names for animations for users who update them and not overwrite with updates
            //
            LoadNamesXml();

            //
            // Load manifests
            //
#if DEBUG
            var stopWatch = new Stopwatch();

            stopWatch.Start();
#endif

            LoadXmls(Sideloader.Sideloader.Manifests.Values.Select(x => x.manifestDocument));

#if DEBUG
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds);
            Log.Level(LogLevel.Warning, $"Load time for LoadXmls {elapsedTime}");

            //
            // For test environment animations manifest are kept in config/AnimationLoader
            // when the plug-in starts it will load them if no zipmod with manifests found
            // May be a feature config flag for everybody or load from here not from there??
            // I like the last one.
            //
            if (animationDict.Count < 1)
            {
                stopWatch.Reset();
                stopWatch.Start();

                LoadTestXml();

                stopWatch.Stop();
                ts = stopWatch.Elapsed;
                elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00000}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds);
                Log.Level(LogLevel.Warning, $"Load time for LoadXmls {elapsedTime}");
            }
#endif
        }

        private void Update()
        {
            if(ReloadManifests.Value.IsDown())
            {
                LoadTestXml();
                Log.Warning($"Scene [{SceneApi.GetAddSceneName()}]");
            }
        }

        /// <summary>
        /// Get move controller for characters
        /// </summary>
        /// <param name="chaControl"></param>
        /// <returns></returns>
        static public MoveController GetMoveController(ChaControl chaControl) =>
            (chaControl == null) || (chaControl.gameObject == null)
            ? null : chaControl.GetComponent<MoveController>();
    }
}
