using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

using BepInEx;
using KKAPI;
using static HFlag;

#if DEBUG
using Newtonsoft.Json;
#endif

namespace AnimationLoader
{
    public partial class SwapAnim
    {
        private const string ManifestRootElement = "AnimationLoader";
        private const string ManifestArrayItem = "Animation";
        private const string ManifestGSArrayItem = KoikatuAPI.GameProcessName;

        private static readonly XmlSerializer xmlSerializer = new(typeof(SwapAnimationInfo));
        private static readonly XmlSerializer xmlOverrideSerializer = new(typeof(OverrideInfo));

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
                    Logger.LogMessage($"0012: [{PInfo.PluginName}] Loading test animations");
                    LoadXmls(docs);
                    return;
                }
            }
            Logger.LogMessage("0013: Make a manifest format .xml in the config/AnimationLoader folder " +
                "to test animations");
        }

        private static void LoadXmls(IEnumerable<XDocument> manifests)
        {
            animationDict = new Dictionary<EMode, List<SwapAnimationInfo>>();
            var count = 0;
            foreach(var manifest in manifests.Select(x => x.Root))
            {
                var guid = manifest.Element("guid").Value;

                // Look for game specific configuration
                animRootGS = manifest
                    .Element(ManifestRootElement)?
                    .Element(KoikatuAPI.GameProcessName);
                animRoot = manifest?.Element(ManifestRootElement);

                if ((animRoot is null) && (animRootGS is null))
                {
                    continue;
                }

                // Process elements valid for any game
                count += ProcessArray(animRoot, guid);

                // Process game specific animations
                if (animRootGS is null)
                {
                    continue;
                }
                foreach (var gameSpecificElement in animRoot.Elements(ManifestGSArrayItem))
                {
                    if (gameSpecificElement is null)
                    {
                        continue;
                    }
                    count += ProcessArray(gameSpecificElement, guid);
                }
            }
            //var dictionary = JsonConvert.SerializeObject(animationDict);
            Logger.LogWarning($"0014: Added {count} animations.");
        }

        private static int ProcessArray(XElement root, string guid)
        {
            var count = 0;

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
                // TODO: Look how to define XmlElement by-passing Unity managed code stripping
                // Json works (if really needed like adding support for another game) maybe
                // change to a Json configuration instead of XML
                if (data.GameSpecificOverrides is not null)
                {
                    var overrideReader = data.GameSpecificOverrides.CreateReader();
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
                count++;
            }
            return count;
        }

        private static void DoOverrides(ref SwapAnimationInfo data, OverrideInfo overrides)
        {
            // TODO: This is an Ugly hack check alternatives

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
            if (overrides.PositionHeroine != UnityEngine.Vector3.zero)
            {
                data.PositionHeroine = overrides.PositionHeroine;
            }
            if (overrides.PositionPlayer != UnityEngine.Vector3.zero)
            {
                data.PositionPlayer = overrides.PositionPlayer;
            }
        }
    }
}
