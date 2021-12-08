using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using UnityEngine;

using static HFlag;

[assembly: System.Reflection.AssemblyFileVersion(AnimationLoader.SwapAnim.Version)]

namespace AnimationLoader
{
    public partial class SwapAnim
    {
        public const string GUID = "essuhauled.animationloader";
        public const string DisplayName = "Animation Loader";
        public const string Version = "1.0.9";

        private static ConfigEntry<bool> SortPositions { get; set; }
        private static ConfigEntry<bool> UseGrid { get; set; }
        private static ConfigEntry<KeyboardShortcut> ReloadManifests { get; set; }
        private const string GeneralSection = "General";

        private const string ManifestRootElement = "AnimationLoader";
        private const string ManifestArrayItem = "Animation";
        private static readonly XmlSerializer xmlSerializer = new XmlSerializer(typeof(SwapAnimationInfo));

        private new static ManualLogSource Logger;
        
        private static Dictionary<EMode, List<SwapAnimationInfo>> animationDict;
        private static Dictionary<HSceneProc.AnimationListInfo, SwapAnimationInfo> swapAnimationMapping;
        private static readonly Type vrType = Type.GetType("VRHScene, Assembly-CSharp");
        private static readonly Color buttonColor = new Color(0.96f, 1f, 0.9f);

        private static readonly Dictionary<string, string> SiruPasteFiles = new Dictionary<string, string>
        {
            {"", ""},
            {"butt", "siru_t_khs_n06"},
            {"facetits", "siru_t_khh_32"},
            {"facetitspussy", "siru_t_khh_32"}, // have to make this manually, for now copy FaceTits
            {"titspussy", "siru_t_khs_n07"},
            {"tits", "siru_t_khh_11"},
            {"pussy", "siru_t_khs_n07"}, // have to make this manually, for now copy TitsPussy
            {"kksbutt", "siru_t_khs_14" },
            {"kksfacetits", "siru_t_khh_33" },
        };

        private static readonly Dictionary<string, int> EModeGroups = new Dictionary<string, int>
        {
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
            Logger = base.Logger;

            // 
            SortPositions = Config.Bind(
                GeneralSection,
                nameof(SortPositions),
                true,
                new ConfigDescription("Sort positions alphabetically"));
            ReloadManifests = Config.Bind(
                GeneralSection,
                nameof(ReloadManifests),
                new KeyboardShortcut(KeyCode.None),
                new ConfigDescription("Load positions from all manifest format xml files inside" +
                " config/AnimationLoader folder"));
            // Standard game code manages the display of animators buttons no grid UI
#if KK
            UseGrid = Config.Bind(
                GeneralSection,
                nameof(UseGrid),
                false,
                new ConfigDescription("If you don't want to use the scrollable list for some reason"));
#endif
            Hooks.Init();
        }

        private void Start()
        {
            LoadXmls(Sideloader.Sideloader.Manifests.Values.Select(x => x.manifestDocument));
        }

        private void Update()
        {
            if(ReloadManifests.Value.IsDown())
                LoadTestXml();
        }

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
            
            Logger.LogMessage("Make a manifest format .xml in the config/AnimationLoader folder to" +
                " test animations");
        }

        private static void LoadXmls(IEnumerable<XDocument> manifests)
        {
            animationDict = new Dictionary<EMode, List<SwapAnimationInfo>>();
            foreach(var manifest in manifests.Select(x => x.Root))
            {
                var guid = manifest.Element("guid").Value;
                var animRoot = manifest.Element(ManifestRootElement);
                if(animRoot == null)
                    continue;
                
                foreach(var animElem in animRoot.Elements(ManifestArrayItem))
                {
                    var reader = animElem.CreateReader();
                    var data = (SwapAnimationInfo)xmlSerializer.Deserialize(reader);
                    data.Guid = guid;
                    reader.Close();
                    
                    if(!animationDict.TryGetValue(data.Mode, out var list))
                        animationDict[data.Mode] = list = new List<SwapAnimationInfo>();
                    list.Add(data);
                }
            }
        }

        private static AnimatorOverrideController SetupAnimatorOverrideController(RuntimeAnimatorController src, RuntimeAnimatorController over)
        {
            if(src == null || over == null)
                return null;
            
            var aoc = new AnimatorOverrideController(src);
            var target = new AnimatorOverrideController(over);
            foreach(var ac in src.animationClips.Where(x => x != null)) //thanks omega/katarsys
                aoc[ac.name] = ac;
            foreach(var ac in target.animationClips.Where(x => x != null)) //thanks omega/katarsys
                aoc[ac.name] = ac;
            aoc.name = over.name;
            return aoc;
        }
        
        private static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            var type = original.GetType();
            var dst = destination.GetComponent(type) as T;
            if (!dst) dst = destination.AddComponent(type) as T;
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                if (field.IsStatic) continue;
                field.SetValue(dst, field.GetValue(original));
            }
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
                prop.SetValue(dst, prop.GetValue(original, null), null);
            }
            return dst;
        }

    }
}
