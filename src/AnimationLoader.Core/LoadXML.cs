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
        public static readonly XmlSerializer xmlKKSSerializer = new(typeof(KKSOverrideInfo));
        public static readonly XmlSerializer xmlKKSerializer = new(typeof(KKOverrideInfo));

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
            foreach(var manifest in manifests.Select(x => x.Root))
            {
                var guid = manifest.Element("guid").Value;
                // TODO: Read both game specific and old format
                // Try game specific format
                animRootGS = manifest.Element(ManifestRootElement)?.Element(KoikatuAPI.GameProcessName);
                animRoot = manifest?.Element(ManifestRootElement);

                XElement[] roots = { animRootGS, animRoot };

                foreach (var root in roots)
                {
                    if (root == null)
                    {
                        continue;
                    }
                    foreach (var animElem in root.Elements(ManifestArrayItem))
                    {
                        var reader = animElem.CreateReader();
                        var data = (SwapAnimationInfo)xmlSerializer.Deserialize(reader);
                        data.Guid = guid;
                        reader.Close();
                        if (data.GameSpecificOverrides != null)
                        {
                            var overrideReader = data.GameSpecificOverrides.CreateReader();
                            var overrideData = (KKSOverrideInfo)xmlKKSSerializer.Deserialize(overrideReader);
                            DoOverrides(data, overrideData);
                        }
                        if (!animationDict.TryGetValue(data.Mode, out var list))
                            animationDict[data.Mode] = list = new List<SwapAnimationInfo>();
                        list.Add(data);
                    }
                }
            }
        }

        private static void DoOverrides(SwapAnimationInfo data, object dataOverride)
        {
            KKSOverrideInfo overrides = (KKSOverrideInfo)dataOverride;

            // Ugly hack for now

            if (overrides.StudioId >= 0)
            {
                data.StudioId = overrides.StudioId;
            }
            if (overrides.PathFemale != null)
            {
                data.PathFemale = overrides.PathFemale;
            }
            if (overrides.ControllerFemale != null)
            {
                data.ControllerFemale = overrides.ControllerFemale;
            }
            if (overrides.PathMale != null)
            {
                data.PathMale = overrides.PathMale;
            }
            if (overrides.ControllerMale != null)
            {
                data.ControllerMale = overrides.ControllerMale;
            }
            if (overrides.AnimationName != null)
            {
                data.AnimationName = overrides.AnimationName;
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
                data.categories = overrides.categories;
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
                data.FileMotionNeck = overrides.FileMotionNeck;
            }
            if (overrides.IsFemaleInitiative != null)
            {
                data.IsFemaleInitiative = overrides.IsFemaleInitiative;
            }
            if (overrides.FileSiruPaste != null)
            {
                data.FileSiruPaste = overrides.FileSiruPaste;
            }
            if (overrides.MotionIKDonor >= 0)
            {
                data.MotionIKDonor = overrides.MotionIKDonor;
            }
        }
    }
}
