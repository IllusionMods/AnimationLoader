//
// Load XML animation information
//
//using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

using BepInEx;
using BepInEx.Logging;
using KKAPI;

//using UnityEngine;
using static HFlag;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        private const string ManifestRootElement = "AnimationLoader";
        private const string ManifestArrayItem = "Animation";
        private const string ManifestGSArrayItem = KoikatuAPI.GameProcessName;

        private static readonly XmlSerializer xmlSerializer =
            new(typeof(SwapAnimationInfo));
#if KKS
        private const string ManifestOverride = "GameSpecificOverrides";
        private static readonly XmlSerializer xmlOverrideSerializer =
            new(typeof(OverrideInfo));
#endif
        private static XElement _animRoot;
        private static XElement _animRootGS;
        private static XElement _animationLoaderVersion;

        private static bool _saveNames = false;

        private static void LoadTestXml()
        {
            var path = Path.Combine(Paths.ConfigPath, "AnimationLoader");
            if(Directory.Exists(path))
            {
                var docs = Directory.GetFiles(path, "*.xml")
                    .Select(XDocument.Load).ToList();
                if(docs.Count > 0)
                {
                    Log.Level(LogLevel.Message, $"0014: [{PluginName}] Loading test " +
                        $"animations");
                    LoadXml(docs);
                    return;
                }
            }
            Log.Level(LogLevel.Message, "0015: Make a manifest format .xml in the " +
                "config/AnimationLoader folder to test animations");
        }

        private static void LoadXml(IEnumerable<XDocument> manifests)
        {
            animationDict = [];
            var count = 0;
            var overrideNames = UserOverrides.Value;
            var logLines = new StringBuilder();

            // Select the only manifests that AnimationLoader will process
            foreach (var manifest in manifests
                .Select(x => x.Root)
                .Where(x => x?.Element(ManifestRootElement) != null))
            {
                _animRoot = manifest?.Element(ManifestRootElement);

                _animRootGS = manifest?
                    .Element(ManifestRootElement)?
                    .Element(KoikatuAPI.GameProcessName);

                if ((_animRoot is null) && (_animRootGS is null))
                {
                    continue;
                }
                var guid = manifest?.Element("guid").Value;
                var version = manifest?.Element("version").Value;

                VersionChecks(manifest);

                // Process elements valid for any game
                count += ProcessArray(
                    _animRoot, guid, version, overrideNames, ref logLines);
                if (_animRootGS is null)
                {
                    if (_saveNames)
                    {
                        SaveNames(animationNamesDict[guid], guid, true);
                    }
                    continue;
                }

                // Process game specific animations
                // Not needed from 1.5.3 on.
                var _animSpecific = _animRoot?.Elements(ManifestGSArrayItem);

                if (_animSpecific is not null)
                {
                    foreach (var gameSpecificElement in _animSpecific)
                    {
                        if (gameSpecificElement is null)
                        {
                            continue;
                        }
                        count += ProcessArray(
                            gameSpecificElement,
                            guid,
                            version,
                            overrideNames,
                            ref logLines);
                    }
                }

                if (_saveNames)
                {
                    SaveNames(animationNamesDict[guid], guid, true);
                }
            }
            if (count > 0)
            {
#if KKS
                Utilities.AlDicExpAddTaii();
#endif
                logLines.Append($"A total of {count} animations processed from " +
                    "manifests.");
                Log.Debug($"0016: Animations loaded:\n\n{logLines}\n");
            }
            else
            {
                Log.Level(
                    LogLevel.Message | LogLevel.Debug,
                    "0017: No animation manifests found.");
            }
        }
    }
}
