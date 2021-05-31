using System;
using System.Collections;
using BepInEx;
using HarmonyLib;
using IllusionUtility.GetUtility;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Xml.Linq;
using System.Xml.Serialization;
using BepInEx.Configuration;
using BepInEx.Logging;
using Illusion.Extensions;
using Sideloader.AutoResolver;
using Studio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static HFlag;

[assembly: System.Reflection.AssemblyFileVersion(AnimationLoader.Koikatu.SwapAnim.Version)]

namespace AnimationLoader.Koikatu
{
    [BepInPlugin(GUID, "Animation Loader", Version)]
    public class SwapAnim : BaseUnityPlugin
    {
        public const string GUID = "SwapAnim";
        public const string Version = "1.0.6";

        private static ConfigEntry<bool> SortPositions { get; set; }
        private static ConfigEntry<bool> UseGrid { get; set; }
        private static ConfigEntry<KeyboardShortcut> ReloadManifests { get; set; }
        private const string GeneralSection = "General";

        private const string ManifestRootElement = "AnimationLoader";
        private const string ManifestArrayItem = "Animation";
        private static readonly XmlSerializer xmlSerializer = new XmlSerializer(typeof(SwapAnimationInfo));

        private new static ManualLogSource Logger;
        private static SwapAnim plugin;
        
        private static Dictionary<EMode, List<SwapAnimationInfo>> animationDict;
        private static SwapAnimationInfo swapAnimationInfo;
        private static List<HSceneProc.AnimationListInfo>[] lstAnimInfo;
        private static PositionCategory category;
        private static readonly Type vrType = Type.GetType("VRHScene, Assembly-CSharp");
        private static readonly Color buttonColor = new Color(0.96f, 1f, 0.9f);
        private static readonly Dictionary<GameObject, float> scrollPos = new Dictionary<GameObject, float>();

        private static readonly Dictionary<string, string> SiruPasteFiles = new Dictionary<string, string>
        {
            {"", ""},
            {"butt", "siru_t_khs_n06"},
            {"facetits", "siru_t_khh_32"},
            {"facetitspussy", "siru_t_khh_32"}, // have to make this manually, for now copy FaceTits
            {"titspussy", "siru_t_khs_n07"},
            {"tits", "siru_t_khh_11"},
            {"pussy", "siru_t_khs_n07"}, // have to make this manually, for now copy TitsPussy
        };

        private void Awake()
        {
            Logger = base.Logger;
            plugin = this;

            SortPositions = Config.Bind(GeneralSection, nameof(SortPositions), true, new ConfigDescription("Sort positions alphabetically"));
            ReloadManifests = Config.Bind(GeneralSection, nameof(ReloadManifests), new KeyboardShortcut(KeyCode.None), new ConfigDescription("Load positions from all manifest format xml files inside config/AnimationLoader folder"));
            UseGrid = Config.Bind(GeneralSection, nameof(UseGrid), false, new ConfigDescription("If you don't want to use the scrollable list for some reason"));
            
            var harmony = Harmony.CreateAndPatchAll(typeof(SwapAnim), nameof(SwapAnim));
            if(vrType != null)
            {
                harmony.Patch(AccessTools.Method(vrType, "ChangeAnimator"), postfix: new HarmonyMethod(typeof(SwapAnim), nameof(SwapAnimation)));
                harmony.Patch(AccessTools.Method(vrType, "ChangeCategory"), prefix: new HarmonyMethod(typeof(SwapAnim), nameof(ChangeCategory)));
                harmony.Patch(AccessTools.Method(vrType, "CreateAllAnimationList"), postfix: new HarmonyMethod(typeof(SwapAnim), nameof(RefreshAnimationList)));
            }
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
            
            Logger.LogMessage("Make a manifest format .xml in the config/AnimationLoader folder to test animations");
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

        [HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), "ChangeCategory")]
        private static void ChangeCategory(int _category)
        {
            category = (PositionCategory)_category;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(HSprite), "OnChangePlaySelect")]
        private static IEnumerable<CodeInstruction> OnChangePlaySelect(IEnumerable<CodeInstruction> instructions)
        {
            //force position change even if position appears to match. Prevents clicks from being eaten.
            return new CodeMatcher(instructions)
                .MatchForward(false, 
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(HSprite), "flags")),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(HFlag), "nowAnimationInfo")),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(HSceneProc.AnimationListInfo), "id"))
                )
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ret)
                )
                .SetAndAdvance(OpCodes.Nop, null)
                .InstructionEnumeration();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSprite), "LoadMotionList")]
        private static void LoadMotionList(HSprite __instance, List<HSceneProc.AnimationListInfo> _lstAnimInfo, GameObject _objParent)
        {
            if(_lstAnimInfo == null || _lstAnimInfo.Count == 0)
                return;


            var buttonParent = _objParent.transform;
            if(UseGrid.Value)
            {
                DestroyImmediate(_objParent.GetComponent<VerticalLayoutGroup>());
                DestroyImmediate(_objParent.GetComponent<GridLayoutGroup>());
                DestroyImmediate(_objParent.GetComponent<ContentSizeFitter>());
                var glg = _objParent.AddComponent<GridLayoutGroup>();
                glg.cellSize = new Vector2(200, 35);
                glg.startAxis = GridLayoutGroup.Axis.Vertical;
                glg.startCorner = GridLayoutGroup.Corner.UpperRight;
                glg.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                glg.constraintCount = 15;
                glg.childAlignment = TextAnchor.UpperRight;
            }
            else
            {
                var buttons = _objParent.transform.Cast<Transform>().ToList();

                //var tex = new Texture2D(1, 1);
                //tex.SetPixel(1, 1, Color.red);
                //var sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                var go = DefaultControls.CreateScrollView(new DefaultControls.Resources());
                go.transform.SetParent(_objParent.transform, false);
                var scroll = go.GetComponent<ScrollRect>();
                scroll.horizontal = false;
                scroll.scrollSensitivity = 32f;
                scroll.movementType = ScrollRect.MovementType.Clamped;
                scroll.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
                scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
                DestroyImmediate(scroll.horizontalScrollbar.gameObject);
                DestroyImmediate(scroll.verticalScrollbar.gameObject);
                DestroyImmediate(scroll.GetComponent<Image>());
                scroll.viewport.sizeDelta = new Vector2(0, 56f);

                var vlg = _objParent.GetComponent<VerticalLayoutGroup>();
                var csf = _objParent.GetComponent<ContentSizeFitter>();
                vlg.enabled = false;
                csf.enabled = false;
                CopyComponent(vlg, scroll.content.gameObject).enabled = true;
                CopyComponent(csf, scroll.content.gameObject).enabled = true;
            
                buttons.ForEach(x => x.SetParent(scroll.content));

                buttonParent = scroll.content;
            }
            
            
            var first = _lstAnimInfo[0];
            if(!animationDict.TryGetValue(first.mode, out var swapAnimations))
                return;

            var animListInfo = lstAnimInfo[(int)first.mode];
            foreach(var anim in swapAnimations.Where(x => (int)x.kindHoushi == first.kindHoushi && (!x.categories.Any() || x.categories.Contains(category))))
            {
                var donorInfo = animListInfo.FirstOrDefault(x => x.id == anim.DonorPoseId).DeepCopy();
                if(donorInfo == null)
                {
                    Logger.LogWarning($"No donor: {anim.Mode} {anim.DonorPoseId}");
                    continue;
                }
                
                if(anim.NeckDonorId >= 0 && anim.NeckDonorId != anim.DonorPoseId)
                    donorInfo.paramFemale.fileMotionNeck = animListInfo.First(x => x.id == anim.NeckDonorId).paramFemale.fileMotionNeck;
                if(anim.FileMotionNeck != null)
                    donorInfo.paramFemale.fileMotionNeck = anim.FileMotionNeck;
                if(anim.IsFemaleInitiative != null)
                    donorInfo.isFemaleInitiative = anim.IsFemaleInitiative.Value;
                if(anim.FileSiruPaste != null && SiruPasteFiles.TryGetValue(anim.FileSiruPaste.ToLower(), out var fileSiruPaste))
                    donorInfo.paramFemale.fileSiruPaste = fileSiruPaste;
                //if(anim.MotionIKDonor >= 0 && anim.NeckDonorId != anim.DonorPoseId)
                //    donorInfo.paramFemale.path.file = animListInfo.First(x => x.id == anim.MotionIKDonor).paramFemale.path.file;
                
                var btn = Instantiate(__instance.objMotionListNode, buttonParent, false);
                btn.AddComponent<HSprite.AnimationInfoComponent>().info = donorInfo;
                btn.transform.FindLoop("Background").GetComponent<Image>().color = buttonColor;

                var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                label.text = anim.AnimationName;
                label.color = Color.black;

                //TODO: wat
                var tgl = btn.GetComponent<Toggle>();
                tgl.group = _objParent.GetComponent<ToggleGroup>();
                tgl.enabled = false;
                tgl.enabled = true;

                btn.GetComponent<SceneAssist.PointerAction>().listClickAction.Add(() =>
                {
                    swapAnimationInfo = anim;
                    __instance.OnChangePlaySelect(btn);
                });
                
                btn.SetActive(true);
                if(__instance.flags.nowAnimationInfo == donorInfo)
                    btn.GetComponent<Toggle>().isOn = true;
            }

            // order all buttons by name and disable New
            foreach(var t in buttonParent.Cast<Transform>().OrderBy(x => x.GetComponentInChildren<TextMeshProUGUI>().text))
            {
                var newT = t.FindLoop("New");
                if(newT) newT.gameObject.SetActive(false);
                
                if(SortPositions.Value)
                    t.SetAsLastSibling();
            }
            
            // save scroll position, disabled for now because its trash
            /*
            if(scrollPos.TryGetValue(_objParent, out var val))
                plugin.StartCoroutine(SetScrollPosition(val));
            else
                scrollPos.Add(_objParent, 0f);
            scroll.onValueChanged.AddListener(v => scrollPos[_objParent] = v.y);

            IEnumerator SetScrollPosition(float value)
            {
                yield return new WaitForEndOfFrame();
                scroll.verticalNormalizedPosition = value;
            }
            */
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSceneProc), "ChangeAnimator")]
        private static void SwapAnimation(object __instance)
        {
            if(swapAnimationInfo == null)
                return;

            var racF = AssetBundleManager.LoadAsset(swapAnimationInfo.PathFemale, swapAnimationInfo.ControllerFemale, typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
            var racM = AssetBundleManager.LoadAsset(swapAnimationInfo.PathMale, swapAnimationInfo.ControllerMale, typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
            
            var t_hsp = Traverse.Create(__instance);
            var female = t_hsp.Field<List<ChaControl>>("lstFemale").Value[0];
            var male = t_hsp.Field<ChaControl>("male").Value;
            ////TODO: lstFemale[1], male1

            female.animBody.runtimeAnimatorController = SetupAnimatorOverrideController(female.animBody.runtimeAnimatorController, racF);
            if(racM != null)
                male.animBody.runtimeAnimatorController = SetupAnimatorOverrideController(male.animBody.runtimeAnimatorController, racM);

            var mi = t_hsp.Field<List<MotionIK>>("lstMotionIK").Value;
            mi.ForEach(mik => mik.Release());
            mi.Clear();

            //TODO: MotionIKData.
            mi.Add(new MotionIK(female));
            mi.Add(new MotionIK(male));
            mi.ForEach(mik =>
            {
                mik.SetPartners(mi);
                mik.Reset();
            });

            swapAnimationInfo = null;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSceneProc), "CreateAllAnimationList")]
        private static void RefreshAnimationList(object __instance)
        {
            lstAnimInfo = Traverse.Create(__instance).Field<List<HSceneProc.AnimationListInfo>[]>("lstAnimInfo").Value;
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

        [HarmonyPostfix, HarmonyPatch(typeof(Studio.Info), nameof(Studio.Info.LoadExcelDataCoroutine))]
        private static void LoadStudioAnims(Studio.Info __instance, ref IEnumerator __result)
        {
            __result = __result.AppendCo(() =>
            {
                int grpId = 999;
                foreach(var keyVal in animationDict)
                {
                    CreateGroup(0);
                    CreateGroup(1);

                    void CreateGroup(byte sex)
                    {
                        var grp = new Info.GroupInfo{ name = $"AL {(sex == 0 ? "M" : "F")} {keyVal.Key}" };
                        __instance.dicAGroupCategory.Add(grpId, grp);
                        var animGrp = new Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>();
                        __instance.dicAnimeLoadInfo.Add(grpId, animGrp);
            
                        foreach(var swapAnimInfo in keyVal.Value.Where(x => x.StudioId >= 0))
                        {
                            grp.dicCategory.Add(swapAnimInfo.StudioId, swapAnimInfo.AnimationName);
                            var animCat = new Dictionary<int, Info.AnimeLoadInfo>();
                            animGrp.Add(swapAnimInfo.StudioId, animCat);

                            var path = sex == 0 ? swapAnimInfo.PathMale : swapAnimInfo.PathFemale;
                            var ctrl = sex == 0 ? swapAnimInfo.ControllerMale : swapAnimInfo.ControllerFemale;
                
                            var controller = AssetBundleManager.LoadAsset(path, ctrl, typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
                            if(controller == null)
                                continue;
                            
                            var clips = controller.animationClips;
                            for(int i = 0; i < clips.Length; i++)
                            {
                                var newSlot = UniversalAutoResolver.GetUniqueSlotID();

                                UniversalAutoResolver.LoadedStudioResolutionInfo.Add(new StudioResolveInfo
                                {
                                    GUID = swapAnimInfo.Guid,
                                    Slot = i,
                                    ResolveItem = true,
                                    LocalSlot = newSlot,
                                    Group = grpId,
                                    Category = swapAnimInfo.StudioId
                                });
                                
                                animCat.Add(newSlot, new Info.AnimeLoadInfo
                                {
                                    name = clips[i].name,
                                    bundlePath = path,
                                    fileName = ctrl,
                                    clip = clips[i].name,
                                });
                            }
                        }

                        grpId++;
                    }
                }
            });
        }
    }
}
