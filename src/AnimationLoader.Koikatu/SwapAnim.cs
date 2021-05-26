using System;
using System.Collections;
using BepInEx;
using HarmonyLib;
using IllusionUtility.GetUtility;
using SceneAssist;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Xml.Linq;
using System.Xml.Serialization;
using BepInEx.Configuration;
using BepInEx.Logging;
using Illusion.Extensions;
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
        public const string Version = "1.0.4";

        private static ConfigEntry<bool> SortPositions { get; set; }

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

            SortPositions = Config.Bind("", nameof(SortPositions), true);
            
            var harmony = Harmony.CreateAndPatchAll(typeof(SwapAnim), nameof(SwapAnim));
            if(vrType != null)
            {
                harmony.Patch(AccessTools.Method(vrType, "ChangeAnimator"), postfix: new HarmonyMethod(AccessTools.Method(typeof(SwapAnim), nameof(SwapAnimation))));
                harmony.Patch(AccessTools.Method(vrType, "ChangeCategory"), prefix: new HarmonyMethod(AccessTools.Method(typeof(SwapAnim), nameof(pre_HSceneProc_ChangeCategory))));
                harmony.Patch(AccessTools.Method(vrType, "CreateAllAnimationList"), postfix: new HarmonyMethod(AccessTools.Method(typeof(SwapAnim), nameof(post_HSceneProc_CreateAllAnimationList))));
            }
        }

        private void Start()
        {
            LoadXmls(Sideloader.Sideloader.Manifests.Values.Select(x => x.manifestDocument));
        }

#if DEBUG
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.RightControl))
                LoadTestXml();
        }

        private void LoadTestXml()
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
#endif

        private void LoadXmls(IEnumerable<XDocument> documents)
        {
            animationDict = new Dictionary<EMode, List<SwapAnimationInfo>>();

            var animationElements = documents.Select(x => x.Root?.Element(ManifestRootElement)).Where(x => x != null).SelectMany(x => x.Elements(ManifestArrayItem));
            foreach(var animElem in animationElements)
            {
                var reader = animElem.CreateReader();
                var data = (SwapAnimationInfo)xmlSerializer.Deserialize(reader);
                reader.Close();
                            
                if(!animationDict.TryGetValue(data.Mode, out var list))
                    animationDict[data.Mode] = list = new List<SwapAnimationInfo>();
                list.Add(data);
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), "ChangeCategory")]
        private static void pre_HSceneProc_ChangeCategory(int _category)
        {
            category = (PositionCategory)_category;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(HSprite), "OnChangePlaySelect")]
        private static IEnumerable<CodeInstruction> tpl_HSprite_OnChangePlaySelect(IEnumerable<CodeInstruction> instructions)
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
        private static void post_HSprite_LoadMotionList(HSprite __instance, List<HSceneProc.AnimationListInfo> _lstAnimInfo, GameObject _objParent)
        {
            if(_lstAnimInfo == null || _lstAnimInfo.Count == 0)
                return;
            
            var buttons = _objParent.transform.Cast<Transform>().ToList();

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
                
                if(anim.NeckDonorId != null && anim.NeckDonorId != anim.DonorPoseId)
                    donorInfo.paramFemale.fileMotionNeck = animListInfo.First(x => x.id == anim.NeckDonorId).paramFemale.fileMotionNeck;
                if(anim.FileMotionNeck != null)
                    donorInfo.paramFemale.fileMotionNeck = anim.FileMotionNeck;
                if(anim.IsFemaleInitiative != null)
                    donorInfo.isFemaleInitiative = anim.IsFemaleInitiative.Value;
                if(anim.FileSiruPaste != null && SiruPasteFiles.TryGetValue(anim.FileSiruPaste.ToLower(), out var fileSiruPaste))
                    donorInfo.paramFemale.fileSiruPaste = fileSiruPaste;
                //if(anim.MotionIKDonor != null && anim.NeckDonorId != anim.DonorPoseId)
                //    donorInfo.paramFemale.path.file = animListInfo.First(x => x.id == anim.MotionIKDonor).paramFemale.path.file;
                
                var btn = Instantiate(__instance.objMotionListNode, scroll.content, false);
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

                btn.GetComponent<PointerAction>().listClickAction.Add(() =>
                {
                    swapAnimationInfo = anim;
                    __instance.OnChangePlaySelect(btn);
                });
                
                btn.SetActive(true);
                if(__instance.flags.nowAnimationInfo == donorInfo)
                    btn.GetComponent<Toggle>().isOn = true;
            }

            // order all buttons by name and disable New
            foreach(var t in scroll.content.Cast<Transform>().OrderBy(x => x.GetComponentInChildren<TextMeshProUGUI>().text))
            {
                var newT = t.FindLoop("New");
                if(newT) newT.gameObject.SetActive(false);
                
                if(SortPositions.Value)
                    t.SetAsLastSibling();
            }
            
            // save scroll position;
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
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSceneProc), "ChangeAnimator")]
        private static void SwapAnimation()
        {
            if(swapAnimationInfo == null)
                return;

            var racF = AssetBundleManager.LoadAsset(swapAnimationInfo.PathFemale, swapAnimationInfo.ControllerFemale, typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
            var racM = AssetBundleManager.LoadAsset(swapAnimationInfo.PathMale, swapAnimationInfo.ControllerMale, typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
            
            var instance = vrType == null ? Singleton<HSceneProc>.Instance : FindObjectOfType(vrType);
            var t_hsp = Traverse.Create(instance);

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
        private static void post_HSceneProc_CreateAllAnimationList(HSceneProc __instance)
        {
            var instance = vrType == null ? Singleton<HSceneProc>.Instance : FindObjectOfType(vrType);
            lstAnimInfo = Traverse.Create(instance).Field<List<HSceneProc.AnimationListInfo>[]>("lstAnimInfo").Value;
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
