﻿//
// Configuration entries
//
using ADV.Commands.Base;

using BepInEx.Configuration;
using BepInEx.Logging;

using KKAPI;
using KKAPI.Utilities;

using UnityEngine;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
#if KKS
        internal static ConfigEntry<bool> LoadInCharStudio { get; set; }
        internal static ConfigEntry<bool> UseAnimationLevels { get; set; }
        internal static ConfigEntry<bool> EnableAllFreeH { get; set; }
#endif
#if KK
        private static ConfigEntry<bool> UseGrid { get; set; }
#endif
#if DEBUG
        internal static ConfigEntry<bool> TestMode { get; set; }
#endif
        internal static ConfigEntry<bool> DebugInfo { get; set; }
        internal static ConfigEntry<bool> DebugToConsole { get; set; }
        internal static ConfigEntry<bool> UserOverrides { get; set; }
        internal static ConfigEntry<KeyboardShortcut> ReloadManifests { get; set; }

        internal static ConfigEntry<bool> Reposition { get; set; }
        internal static ConfigEntry<bool> SortPositions { get; set; }
        internal static ConfigEntry<bool> HighLight { get; set; }
        internal static ConfigEntry<bool> MotionIK { get; set; }

        internal const string GeneralSection = "General";
        internal const string DebugSection = "Debug";
        internal const string AdvanceSection = "Advance";

        internal void ConfigEntries()
        {
#if KKS
            LoadInCharStudio = Config.Bind(
                section: GeneralSection,
                key: "Character Studio",
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Enable/Disabled module for Studio",
                    tags: new ConfigurationManagerAttributes { Order = 35 }));

            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
            {
                if (!LoadInCharStudio.Value)
                {
                    Log.Message("0013: MOD disabled in configuration.");
                    enabled = false;
                    return;
                }
            }

            UseAnimationLevels = Config.Bind(
                section: GeneralSection,
                key: "Use Experience Levels",
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Apply experience levels to animations",
                    tags: new ConfigurationManagerAttributes { Order = 16}));

            EnableAllFreeH = Config.Bind(
                section: GeneralSection,
                key: "All Animations in FreeH",
                defaultValue: false,
                configDescription: new ConfigDescription(
                    description: "Enable all loaded animations in FreeH without needing to use them " +
                        "in story mode",
                    tags: new ConfigurationManagerAttributes { Order = 15}));
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
                    tags: new ConfigurationManagerAttributes { Order = 6 }));
#endif

            // Load manifests from configuration directory
            ReloadManifests = Config.Bind(
                section: GeneralSection,
                key: nameof(ReloadManifests),
                defaultValue: new KeyboardShortcut(KeyCode.None),
                configDescription: new ConfigDescription(
                    description: "Load positions from all manifest format xml files inside " +
                        "config/AnimationLoader folder",
                    tags: new ConfigurationManagerAttributes { Order = 39 }));

            SortPositions = Config.Bind(
            section: GeneralSection,
            key: nameof(SortPositions),
            defaultValue: true,
            configDescription: new ConfigDescription(
                description: "Sort positions alphabetically",
                tags: new ConfigurationManagerAttributes { Order = 37 }));

            // Reposition characters in the animations it can help with clipping
            Reposition = Config.Bind(
                section: AdvanceSection,
                key: "Reposition Characters",
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Some animations have information in the manifest to move "
                        + "the characters",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 19, IsAdvanced = true }));

            // Highlight text in buttons helps identify store effects when used
            HighLight = Config.Bind(
                section: AdvanceSection,
                key: "Highlight Buttons Text",
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Uses more color to highlight the effects of the " 
                        + "store plug-in and Free-H",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 18, IsAdvanced = true }));

            // Reposition characters in the animations it can help with clipping
            MotionIK = Config.Bind(
                section: AdvanceSection,
                key: "Setup Motion IK",
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Some animations have motion IK configurations if color" +
                        " is used they can be identified by a darker yellow color",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 17, IsAdvanced = true }));


            // Save names in UserData.  These can be expanded to other fields if need be.
            UserOverrides = Config.Bind(
                section: GeneralSection,
                key: "Save Names",
                defaultValue: false,
                configDescription: new ConfigDescription(
                    description: "Save and use names stored in UserData/AnimationLoader/Names. " +
                        "Names then can be customized and won't be overwritten by updates.",
                    tags: new ConfigurationManagerAttributes { Order = 33 }));

            // To generate debug information this has to be enabled
            // The majority of the Logs are in conditional compilation
            DebugInfo = Config.Bind(
                section: DebugSection,
                key: "Debug Information",
                defaultValue: false,
                configDescription: new ConfigDescription(
                    description: "Show debug information",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 29, IsAdvanced = true }));
            DebugInfo.SettingChanged += (_sender, _args) =>
            {
                Log.Enabled = DebugInfo.Value;
#if DEBUG
                Log.Level(LogLevel.Info, $"0028: Log.Enabled set to {Log.Enabled}");
#endif
            };

            DebugToConsole = Config.Bind(
                            section: DebugSection,
                            key: "Debug information to Console",
                            defaultValue: false,
                            configDescription: new ConfigDescription(
                                description: "Show debug information in Console",
                                acceptableValues: null,
                                tags: new ConfigurationManagerAttributes {
                                    Order = 27,
                                    IsAdvanced = true
                                }));
            DebugToConsole.SettingChanged += (_sender, _args) =>
            {
                Log.DebugToConsole = DebugToConsole.Value;
#if DEBUG
                Log.Level(LogLevel.Info, $"[ConfigEntries] Log.DebugToConsole set to " +
                    $"{Log.DebugToConsole}");
#endif
            };
#if DEBUG
            Log.Level(LogLevel.Info, $"0028: Log.Enabled set to {Log.Enabled}");


            TestMode = Config.Bind(
                section: DebugSection,
                key: "Enable Test Mode",
                defaultValue: false,
                configDescription: new ConfigDescription(
                    description: "Allow unused animations in Free-H",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 26, IsAdvanced = true }));
#endif
        }
    }
}
