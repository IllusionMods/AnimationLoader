using System;
using System.Collections.Generic;
using System.Linq;
#if DEBUG
using System.Diagnostics;
#endif

using BepInEx.Logging;

using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;

#if KK
using UnityEngine;
#endif

using static HFlag;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
#if KK
        private static readonly Color buttonColor = new(0.96f, 1f, 0.9f);
#endif
        private static Dictionary<EMode, List<SwapAnimationInfo>> animationDict;
        private static Dictionary<HSceneProc.AnimationListInfo, SwapAnimationInfo>
            swapAnimationMapping;
        private static readonly Type VRHSceneType = Type.GetType("VRHScene, Assembly-CSharp");

        private static readonly Dictionary<string, string> SiruPasteFiles = new() {
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

        private static readonly Dictionary<string, int> EModeGroups = new() {
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

        /// <summary>
        /// Make information available for other plugins
        /// </summary>
        public static Dictionary<EMode, List<SwapAnimationInfo>> AnimationsDict {
            get { return animationDict; }
            private set { animationDict = value; }
        }

        /// <summary>
        /// Make information available for other plugins
        /// </summary>
        public static Dictionary<HSceneProc.AnimationListInfo, SwapAnimationInfo> SwapAnimationMapping {
            get { return swapAnimationMapping; }
            private set { swapAnimationMapping = value; }
        }

        private void Awake()
        {
            Log.LogSource = Logger; ;

            ConfigEntries();

            Log.Enabled = DebugInfo.Value;
            Log.DebugToConsole = DebugToConsole.Value;
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
            // To save used animations on H exit
            GameAPI.RegisterExtraBehaviour<AnimationLoaderGameController>(GUID);
#endif
        }

        private void Start()
        {
#if DEBUG
            var stopWatch = new Stopwatch();

            stopWatch.Start();
#endif
#if KKS
            // Read used animations
            _usedAnimations.Read();
            _animationsUseStats.Read();
#endif
            // Read foot job animations
            _footJobAnimations.Read();

            //
            // Save names for animations for users who update them and not overwritten with updates
            //
            LoadNamesXml();

            //
            // Load manifests
            //

            LoadXmls(Sideloader.Sideloader.Manifests.Values.Select(x => x.manifestDocument));
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;

            var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00000}",
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
            if (ReloadManifests.Value.IsDown())
            {
                LoadTestXml();
                _footJobAnimations.Read();
            }
        }

        /// <summary>
        /// Get move controller for characters
        /// </summary>
        /// <param name="chaControl"></param>
        /// <returns></returns>
        public static MoveController GetMoveController(ChaControl chaControl) =>
            (chaControl == null) || (chaControl.gameObject == null)
            ? null : chaControl.GetComponent<MoveController>();
    }
}
