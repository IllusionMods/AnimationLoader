using System;
using System.Collections.Generic;
using System.Linq;

using BepInEx.Configuration;
using BepInEx.Logging;

using KKAPI;
using KKAPI.Chara;
using KKAPI.MainGame;
using KKAPI.Utilities;

using UnityEngine;

using static HFlag;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal static ConfigEntry<bool> SortPositions { get; set; }
#if KKS
        internal static ConfigEntry<bool> LoadInCharStudio { get; set; }
#endif
#if KK
        private static ConfigEntry<bool> UseGrid { get; set; }
#endif
        internal static ConfigEntry<KeyboardShortcut> ReloadManifests { get; set; }
        internal static ConfigEntry<bool> DebugInfo { get; set; }
        internal static ConfigEntry<bool> Reposition { get; set; }
        internal static ConfigEntry<bool> UseAnimationLevels { get; set; }

        internal const string GeneralSection = "General";

        internal void ConfigEntries()
        {
            ReloadManifests = Config.Bind(
                section: GeneralSection,
                key: nameof(ReloadManifests),
                defaultValue: new KeyboardShortcut(KeyCode.None),
                configDescription: new ConfigDescription(
                    description: "Load positions from all manifest format xml files inside " +
                        "config/AnimationLoader folder",
                    tags: new ConfigurationManagerAttributes { Order = 10 }));

            SortPositions = Config.Bind(
            section: GeneralSection,
            key: nameof(SortPositions),
            defaultValue: true,
            configDescription: new ConfigDescription(
                description: "Sort positions alphabetically",
                tags: new ConfigurationManagerAttributes { Order = 8 }));

#if KKS

            LoadInCharStudio = Config.Bind(
                section: GeneralSection,
                key: "Character Studio",
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Disabled module for Studio",
                    tags: new ConfigurationManagerAttributes { Order = 6 }));
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
                key: "Use Animation Levels",
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Apply optional experience levels to animations",
                    tags: new ConfigurationManagerAttributes { Order = 5, IsAdvanced = true }));

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

            // Reposition characters in the animations it can help with clipping
            Reposition = Config.Bind(
                section: GeneralSection,
                key: "Reposition Characters",
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Reposition characters in the animation before it starts",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 4, IsAdvanced = true }));

            // To generate debug information this has to be enabled
            // Almost all Logs are in conditional compilation
            DebugInfo = Config.Bind(
                section: "Debug",
                key: "Debug Information",
                defaultValue: false,
                configDescription: new ConfigDescription(
                    description: "Show debug information",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { Order = 2, IsAdvanced = true }));
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
            Log.Level(LogLevel.Info, $"0028B: Levels set to {UseAnimationLevels.Value}");
#endif
        }
    }
}
