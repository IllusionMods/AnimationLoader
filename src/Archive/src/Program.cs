using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

using Newtonsoft.Json;

/*
<Animation>
    <StudioId>28</StudioId>
    <PathFemale>anim_imports/kplug/female/01_65.unity3d</PathFemale>
    <PathMale>anim_imports/kplug/male/01_65.unity3d</PathMale>
    <ControllerFemale>khh_f_87</ControllerFemale>
    <ControllerMale>khh_m_87</ControllerMale>
    <AnimationName>Standing Deepthroat</AnimationName>
    <Mode>houshi</Mode>
    <kindHoushi>Mouth</kindHoushi>
    <categories>
        <category>Stand</category>
        <category>LieDown</category>
    </categories>
    <DonorPoseId>39</DonorPoseId>
    <GameSpecificOverrides>
        <!-- anything in here overrides/replaces equivalent entries  -->
        <KoikatsuSunshine>
            <DonorPoseId>42</DonorPoseId>
            <NeckDonorId>55</NeckDonorId> 
        </KoikatsuSunshine>
    </GameSpecificOverrides>
</Animation>
*/

namespace TesteApplication
{
    public class Program
    {
        public static Dictionary<EMode, List<SwapAnimationInfo>> animationDict;
        public const string ManifestRootElement = "AnimationLoader";
        public const string ManifestGameRootElement = "KoikatsuSunshine";
        public const string ManifestArrayItem = "Animation";
        public static readonly XmlSerializer xmlSerializer = new(typeof(SwapAnimationInfo));

        public static readonly XmlSerializer xmlKKSSerializer = new(typeof(KKSOverrideInfo));
        public static readonly XmlSerializer xmlKKSerializer = new(typeof(KKOverrideInfo));

        public static XElement animRoot;
        private static XElement animRootGS;

        public static bool IsNew { get; set; } = false;
        public static bool IsOld { get; set;} = false;
        public static bool HasNewElements { get; set; } = false;
        public static bool HasOldElements { get; set; } = false;
        public static bool IsMix { get; set; } = false;

        internal const string _gameName = "KoikatsuSunshine";

        static void Main(string[] args)
        {
            LoadTestXml();
        }

        public static void LoadTestXml()
        {
            //var path = Path.Combine(Paths.ConfigPath, "AnimationLoader");
            var path = Path.Combine(".\\", "AnimationLoader");
            if (Directory.Exists(path))
            {
                var docs = Directory.GetFiles(path, "*.xml").Select(XDocument.Load).ToList();
                if (docs.Count > 0)
                {
                    //Logger.LogMessage("Loading test animations");
                    Console.WriteLine("Loading test animations\n");
                    LoadXmls(docs);
                    return;
                }
            }

            //Logger.LogMessage("Make a manifest format .xml in the config/AnimationLoader folder to" +            " test animations");
            Console.WriteLine("Make a manifest format .xml in the config/AnimationLoader folder to" + " test animations");
        }

        public static void LoadXmls(IEnumerable<XDocument> manifests)
        {
            animationDict = new Dictionary<EMode, List<SwapAnimationInfo>>();
            int count = 0;
            foreach (var manifest in manifests.Select(x => x.Root))
            {
                var guid = manifest.Element("guid").Value;
                // Game specific elements
                animRootGS = manifest.Element(ManifestRootElement)?.Element(_gameName);
                // Game neutral elements
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
                            var overrideReader = data.GameSpecificOverrides.CreateReader();
                            var overrideData = (KKSOverrideInfo)xmlKKSSerializer.Deserialize(overrideReader);
                            DoOverrides(ref data, overrideData);
                        }
                        if (!animationDict.TryGetValue(data.Mode, out var list))
                            animationDict[data.Mode] = list = new List<SwapAnimationInfo>();
                        list.Add(data);
                        Console.WriteLine($"Animation Name = {data.AnimationName}");
                        count++;
                    } 
                }
                Console.WriteLine($"Total animations {count}");
                Console.WriteLine($"\n\nDone!\n\n");
            }
        }

        private static void DoOverrides(ref SwapAnimationInfo data, object dataOverride)
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
            if (overrides.categories != Array.Empty<PositionCategory>())
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

        private static bool AnalyzeManifest(XElement manifest)
        {
            var format = manifest.Element("format")?.Value;
            var kkNew = manifest.Element(ManifestRootElement).Element("Koikatu");
            var kksNew = manifest.Element(ManifestRootElement).Element("KoidatsuSunshine");
            var oldFormat = manifest.Element(ManifestRootElement).Element(ManifestArrayItem);

            if (!string.IsNullOrEmpty(format))
            {
                IsNew = true;
            }
            else
            {
                IsOld = true;
            }
            if (kkNew != null)
            {
                HasNewElements = true;
            }
            if (kksNew != null)
            {
                HasNewElements = true;
            }
            if (oldFormat != null)
            {
                HasOldElements = true;
            }
            if (IsNew)
            {
                if (HasOldElements)
                {
                    Console.WriteLine("Error old elements in new format file");
                }
            }
            if (IsOld)
            {
                if (HasOldElements || HasNewElements)
                {
                    Console.WriteLine("Error has new elements in old format");
                }
            }            
            return true;
        }

        [XmlRoot("Animation")]
        [Serializable]
        public class SwapAnimationInfo
        {
            [XmlIgnore]
            public string Guid;

            [XmlElement]
            public int StudioId = -1;

            [XmlElement]
            public string PathFemale;

            [XmlElement]
            public string ControllerFemale;

            [XmlElement]
            public string PathMale;

            [XmlElement]
            public string ControllerMale;

            [XmlElement]
            public string AnimationName;

            [XmlElement]
            public EMode Mode;

            [XmlElement]
            public KindHoushi kindHoushi;

            [XmlArray]
            [XmlArrayItem("category", Type = typeof(PositionCategory))]
            public PositionCategory[] categories = Array.Empty<PositionCategory>();

            [XmlElement]
            public int DonorPoseId;

            [XmlElement]
            public int NeckDonorId = -1;

            [XmlElement]
            public string FileMotionNeck;

            [XmlElement]
            public bool? IsFemaleInitiative;

            [XmlElement]
            public string FileSiruPaste;

            [XmlElement]
            public int MotionIKDonor = -1;

            [XmlElement]
            public XElement GameSpecificOverrides;
        }

        [XmlRoot("KoikatsuSunshine")]
        [Serializable]
        public class KKSOverrideInfo
        {
            [XmlIgnore]
            public string Guid;

            [XmlElement]
            public int StudioId = -1;

            [XmlElement]
            public string PathFemale;

            [XmlElement]
            public string ControllerFemale;

            [XmlElement]
            public string PathMale;

            [XmlElement]
            public string ControllerMale;

            [XmlElement]
            public string AnimationName;

            [XmlElement]
            public EMode Mode = EMode.none;

            [XmlElement]
            public KindHoushi kindHoushi = KindHoushi.none;

            [XmlArray]
            [XmlArrayItem("category", Type = typeof(PositionCategory))]
            public PositionCategory[] categories = Array.Empty<PositionCategory>();

            [XmlElement]
            public int DonorPoseId;

            [XmlElement]
            public int NeckDonorId = -1;

            [XmlElement]
            public string FileMotionNeck;

            [XmlElement]
            public bool? IsFemaleInitiative;

            [XmlElement]
            public string FileSiruPaste;

            [XmlElement]
            public int MotionIKDonor = -1;
        }

        [XmlRoot("Koikatu")]
        [Serializable]
        public class KKOverrideInfo
        {
            [XmlIgnore]
            public string Guid;

            [XmlElement]
            public int StudioId = -1;

            [XmlElement]
            public string PathFemale;

            [XmlElement]
            public string ControllerFemale;

            [XmlElement]
            public string PathMale;

            [XmlElement]
            public string ControllerMale;

            [XmlElement]
            public string AnimationName;

            [XmlElement]
            public EMode Mode = EMode.none;

            [XmlElement]
            public KindHoushi kindHoushi = KindHoushi.none;

            [XmlArray]
            [XmlArrayItem("category", Type = typeof(PositionCategory))]
            public PositionCategory[] categories = Array.Empty<PositionCategory>();

            [XmlElement]
            public int DonorPoseId;

            [XmlElement]
            public int NeckDonorId = -1;

            [XmlElement]
            public string FileMotionNeck;

            [XmlElement]
            public bool? IsFemaleInitiative;

            [XmlElement]
            public string FileSiruPaste;

            [XmlElement]
            public int MotionIKDonor = -1;
        }

        public enum KindHoushi
        {
            Hand = 0,
            Mouth = 1,
            Breasts = 2,
            none = -1,
        }

        public enum PositionCategory
        {
            LieDown = 0,
            Stand = 1,
            SitChair = 2,
            Stool = 3,
            SofaBench = 4,
            BacklessBench = 5,
            SchoolDesk = 6,
            Desk = 7,
            Wall = 8,
            StandPool = 9,
            SitDesk = 10,
            SquadDesk = 11,
            Ground3P = 1100,
            none = -1,
        }

        public enum EMode
        {
            none = -1,
            aibu,
            houshi,
            sonyu,
            masturbation,
            peeping,
            lesbian,
            houshi3P,
            sonyu3P,
            houshi3PMMF,
            sonyu3PMMF
        }
    }
}

