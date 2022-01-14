using System;
using System.Collections.Generic;
using System.Linq;

using BepInEx.Configuration;
using BepInEx.Logging;

using KKAPI;
using KKAPI.Chara;
using KKAPI.Utilities;

using UnityEngine;
using static HFlag;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        private static ConfigEntry<bool> SortPositions { get; set; }
#if KKS
        private static ConfigEntry<bool> LoadInCharStudio { get; set; }
#endif
#if KK
        private static ConfigEntry<bool> UseGrid { get; set; }
#endif
        private static ConfigEntry<KeyboardShortcut> ReloadManifests { get; set; }
        internal static ConfigEntry<bool> DebugInfo { get; set; }
        internal static ConfigEntry<bool> Reposition { get; set; }
        private const string GeneralSection = "General";
        private static readonly Color buttonColor = new(0.96f, 1f, 0.9f);

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

        private void Awake()
        {
            Log.SetLogSource(Logger); ;

            SortPositions = Config.Bind(
                section: GeneralSection,
                key: nameof(SortPositions),
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Sort positions alphabetically",
                    tags: new ConfigurationManagerAttributes {Order = 4}));
            ReloadManifests = Config.Bind(
                section: GeneralSection,
                key: nameof(ReloadManifests),
                defaultValue: new KeyboardShortcut(KeyCode.None),
                configDescription: new ConfigDescription(
                    description: "Load positions from all manifest format xml files inside " +
                        "config/AnimationLoader folder",
                    tags: new ConfigurationManagerAttributes { Order = 5 }));
            // For KKS the application code handles the display of animators buttons no grid UI.
#if KKS
            LoadInCharStudio = Config.Bind(
                section: GeneralSection,
                key: "Character Studio",
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Disabled module for Studio",
                    tags: new ConfigurationManagerAttributes { Order = 3 }));
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
            {
                if (!LoadInCharStudio.Value)
                {
                    Log.Message("0013: MOD disabled in configuration.");
                    enabled = false;
                    return;
                }
            }
#endif
#if KK
            // TODO: Grid UI for KKS
            // How many animations will require a scrollable grid
            UseGrid = Config.Bind(
                section: GeneralSection,
                key: nameof(UseGrid),
                defaultValue: false,
                configDescription: new ConfigDescription(
                    description: "If you don't want to use the scrollable" +
                        " list for some reason",
                    tags: new ConfigurationManagerAttributes { Order = 3 }));
#endif
            // Reposition characters in the animations it can help with clipping
            Reposition = Config.Bind(
                section: "Debug",
                key: "Reposition Characters",
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Reposition characters in the animation before it starts",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 2, IsAdvanced = true }));
            // To generate debug information this has to be enabled
            // Almost all Logs are in conditional compilation
            DebugInfo = Config.Bind(
                section: "Debug",
                key: "Debug Information",
                defaultValue: false,
                configDescription: new ConfigDescription(
                    description: "Show debug information",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 1, IsAdvanced = true }));
            DebugInfo.SettingChanged += (_sender, _args) =>
            {
                Log.Enabled = DebugInfo.Value;
#if DEBUG
                Log.Level(LogLevel.Info, $"0028: Log.Enabled set to {Log.Enabled}");
#endif
            };
            Log.Enabled = DebugInfo.Value;
#if DEBUG
            Log.Level(LogLevel.Info, $"0028: Log.Enabled set to {Log.Enabled}");
#endif
            Hooks.Init();
            // Register move characters controller
            CharacterApi.RegisterExtraBehaviour<MoveController>(PInfo.GUID);
        }

        private void Start()
        {
            // TODO: Save names for animations for players who change them
            LoadXmls(Sideloader.Sideloader.Manifests.Values.Select(x => x.manifestDocument));
#if DEBUG
            //
            // For test environment animations manifest are kept in config/AnimationLoader
            // when the plug-in starts it will load them if no zipmod with manifests found
            //
            if (animationDict.Count < 1)
            {
                LoadTestXml();
            }
#endif
        }

        private void Update()
        {
            if(ReloadManifests.Value.IsDown())
            {
                LoadTestXml();
            }
        }

        private static AnimatorOverrideController SetupAnimatorOverrideController(
            RuntimeAnimatorController src,
            RuntimeAnimatorController over)
        {
            if(src == null || over == null)
            {
                return null;
            }

            var aoc = new AnimatorOverrideController(src);
            var target = new AnimatorOverrideController(over);
            foreach(var ac in src.animationClips.Where(x => x != null)) //thanks omega/katarsys
            {
                aoc[ac.name] = ac;
            }

            foreach (var ac in target.animationClips.Where(x => x != null)) //thanks omega/katarsys
            {
                aoc[ac.name] = ac;
            }

            aoc.name = over.name;
            return aoc;
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
