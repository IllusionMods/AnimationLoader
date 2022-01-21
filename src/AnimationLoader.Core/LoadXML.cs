using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

using BepInEx;
using BepInEx.Logging;
using KKAPI;

using UnityEngine;
using static HFlag;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        private const string ManifestRootElement = "AnimationLoader";
        private const string ManifestArrayItem = "Animation";
        private const string ManifestGSArrayItem = KoikatuAPI.GameProcessName;

        private static readonly XmlSerializer xmlSerializer = new(typeof(SwapAnimationInfo));
        // TODO: Read game overrides when running in KK
#if KKS
        private const string ManifestOverride = "GameSpecificOverrides";
        private static readonly XmlSerializer xmlOverrideSerializer = new(typeof(OverrideInfo));
#endif
        private static XElement animRoot;
        private static XElement animRootGS;

        private static void LoadTestXml()
        {
            var path = Path.Combine(Paths.ConfigPath, "AnimationLoader");
            if(Directory.Exists(path))
            {
                var docs = Directory.GetFiles(path, "*.xml").Select(XDocument.Load).ToList();
                if(docs.Count > 0)
                {
                    Log.Level(LogLevel.Message, $"0014: [{PInfo.PluginName}] Loading test " +
                        $"animations");
                    LoadXmls(docs);
                    return;
                }
            }
            Log.Level(LogLevel.Message, "0015: Make a manifest format .xml in the " +
                "config/AnimationLoader folder to test animations");
        }

        private static void LoadXmls(IEnumerable<XDocument> manifests)
        {
            animationDict = new Dictionary<EMode, List<SwapAnimationInfo>>();
            var count = 0;
            var logLines = new StringBuilder();

            // Select the only manifests that AnimationLoader will process
            foreach (var manifest in manifests
                .Select(x => x.Root)
                .Where(x => x?.Element(ManifestRootElement) != null))
            {
                animRoot = manifest?.Element(ManifestRootElement);
                // Look for game specific configuration
                animRootGS = manifest
                    .Element(ManifestRootElement)?
                    .Element(KoikatuAPI.GameProcessName);

                if ((animRoot is null) && (animRootGS is null))
                {
                    continue;
                }
                var guid = manifest.Element("guid").Value;
                var version = manifest.Element("version").Value;
                // Process elements valid for any game
                count += ProcessArray(animRoot, guid, version, ref logLines);
                if (animRootGS is null)
                {
                    continue;
                }
                // Process game specific animations
                foreach (var gameSpecificElement in animRoot.Elements(ManifestGSArrayItem))
                {
                    if (gameSpecificElement is null)
                    {
                        continue;
                    }
                    count += ProcessArray(gameSpecificElement, guid, version, ref logLines);
                }
            }
            if (count > 0)
            {
                logLines.Append($"\n{count} animations processed from manifests.\n");
#if DEBUG
                Log.Info($"0016: Animations:\n\n{logLines}");
#else
                Log.Debug($"0016: Animations:\n\n{logLines}");
#endif
            }
            else
            {
                Log.Level(LogLevel.Warning, "0017: No animation manifests found.");
            }
        }

        private static int ProcessArray(
            XElement root, 
            string guid, 
            string version, 
            ref StringBuilder logLines)
        {
            var count = 0;
            logLines.Append($"From {guid}-{version}: {RootText(root.Name)}\n");
            foreach (var animElem in root.Elements(ManifestArrayItem))
            {
                if (animElem == null)
                {
                    continue;
                }
                var reader = animElem.CreateReader();
                var data = (SwapAnimationInfo)xmlSerializer.Deserialize(reader);
                data.Guid = guid;
                reader.Close();
#if KKS
                var overrideRoot = animElem?
                    .Element(ManifestOverride)?
                    .Element(KoikatuAPI.GameProcessName);
                if (overrideRoot != null)
                {
                    var overrideReader = overrideRoot.CreateReader();
                    var overrideData = (OverrideInfo)xmlOverrideSerializer
                        .Deserialize(overrideReader);
                    overrideReader.Close();
                    DoOverrides(ref data, overrideData);
                }
#endif
                if (!animationDict.TryGetValue(data.Mode, out var list))
                {
                    animationDict[data.Mode] = list = new List<SwapAnimationInfo>();
                }
                list.Add(data);
                logLines.Append($"{AnimationInfo.GetKey(data),-30} - {data.AnimationName}\n");
                count++;
            }
            return count;
        }

        internal static Func<XName, string> RootText = x => x == KoikatuAPI.GameProcessName ?
            $"Game specific elements of {x}" : $"Root elements of {x}";

#if KKS
        private static void DoOverrides(
            ref SwapAnimationInfo data, 
            OverrideInfo overrides)
        {
            if (overrides.PathFemale != null)
            {
                data.PathFemale = string.Copy(overrides.PathFemale);
            }
            if (overrides.ControllerFemale != null)
            {
                data.ControllerFemale = string.Copy(overrides.ControllerFemale);
            }
            if (overrides.PathMale != null)
            {
                data.PathMale = string.Copy(overrides.PathMale);
            }
            if (overrides.ControllerMale != null)
            {
                data.ControllerMale = string.Copy(overrides.ControllerMale);
            }
            if (overrides.AnimationName != null)
            {
                data.AnimationName = string.Copy(overrides.AnimationName);
            }
            if (overrides.Mode >= 0)
            {
                data.Mode = overrides.Mode;
            }
            if (overrides.kindHoushi >= 0)
            {
                data.kindHoushi = overrides.kindHoushi;
            }
            if (overrides.categories != null)
            {
                overrides.categories.CopyTo(data.categories, 0);
            }
            if (overrides.DonorPoseId >= 0)
            {
                data.DonorPoseId = overrides.DonorPoseId;
            }
            if (overrides.NeckDonorId >= 0)
            {
                data.NeckDonorId = overrides.NeckDonorId;
            }
            if (overrides.FileMotionNeck != null)
            {
                data.FileMotionNeck = string.Copy(overrides.FileMotionNeck);
            }
            if (overrides.IsFemaleInitiative != null)
            {
                data.IsFemaleInitiative = overrides.IsFemaleInitiative;
            }
            if (overrides.FileSiruPaste != null)
            {
                data.FileSiruPaste = string.Copy(overrides.FileSiruPaste);
            }
            if (overrides.MotionIKDonor >= 0)
            {
                data.MotionIKDonor = overrides.MotionIKDonor;
            }
            if (overrides.PositionHeroine != Vector3.zero)
            {
                data.PositionHeroine = overrides.PositionHeroine;
            }
            if (overrides.PositionPlayer != Vector3.zero)
            {
                data.PositionPlayer = overrides.PositionPlayer;
            }
        }
#endif
    }
}
