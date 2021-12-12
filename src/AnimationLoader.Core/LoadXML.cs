using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

using BepInEx;
using KKAPI;
using static HFlag;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        private const string ManifestRootElement = "AnimationLoader";
        private const string ManifestArrayItem = "Animation";
        private static readonly XmlSerializer xmlSerializer = new(typeof(SwapAnimationInfo));
        private static readonly XmlSerializer xmlKKSSerializer = new(typeof(KKSOverrideInfo));
        private static readonly XmlSerializer xmlKKSerializer = new(typeof(KKOverrideInfo));

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
                    Logger.LogMessage("Loading test animations");
                    LoadXmls(docs);
                    return;
                }
            }

            Logger.LogMessage("Make a manifest format .xml in the config/AnimationLoader folder " +
                "to test animations");
        }

        private static void LoadXmls(IEnumerable<XDocument> manifests)
        {
            animationDict = new Dictionary<EMode, List<SwapAnimationInfo>>();
            int count = 0;
            foreach(var manifest in manifests.Select(x => x.Root))
            {
                var guid = manifest.Element("guid").Value;

                // Try game specific format
                animRootGS = manifest.Element(ManifestRootElement)?.Element(KoikatuAPI.GameProcessName);
                animRoot = manifest?.Element(ManifestRootElement);

                if ((animRoot == null) && (animRootGS == null))
                {
                    continue;
                }

                XElement[] roots = { animRootGS, animRoot };

                foreach (var root in roots)
                {
                    if (root == null)
                    {
                        continue;
                    }
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
                        if (data.GameSpecificOverrides != null)
                        {
                            Logger.LogWarning($"Label to override = {data.AnimationName}");
#if KKS
                            var overrideReader = data.GameSpecificOverrides.CreateReader();
                            var overrideData = (KKSOverrideInfo)xmlKKSSerializer.Deserialize(overrideReader);
#elif KK
                            // Work around for KK GameSpecificOverrides is parsed as a string not XElement
                            var overrideElement = XElement.Parse(data.GameSpecificOverrides);
                            var overrideReader = overrideElement.CreateReader();
                            var overrideData = (KKOverrideInfo)xmlKKSerializer.Deserialize(overrideReader);
#endif
                            DoOverrides(ref data, overrideData);
                            data.GameSpecificOverrides = null;
                            Logger.LogWarning($"Label after attempt = {data.AnimationName}");
                        }
                        if (!animationDict.TryGetValue(data.Mode, out var list))
                            animationDict[data.Mode] = list = new List<SwapAnimationInfo>();
                        list.Add(data);
                        count++;
                    }
                }
            }
            Logger.LogWarning($"Added {count} animations.");
        }

        private static void DoOverrides(ref SwapAnimationInfo data, object dataOverride)
        {
#if KKS
            KKSOverrideInfo overrides = (KKSOverrideInfo)dataOverride;
#elif KK
            KKOverrideInfo overrides = (KKOverrideInfo)dataOverride;
#endif
            // TODO: Ugly hack for now check alternatives

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
        }
    }
}
