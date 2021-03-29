using System;
using BepInEx;
using HarmonyLib;
using IllusionUtility.GetUtility;
using SceneAssist;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static HFlag;

[assembly: System.Reflection.AssemblyFileVersion(AnimationLoader.Koikatu.SwapAnim.Version)]

namespace AnimationLoader.Koikatu
{
    [BepInPlugin(GUID, "SwapAnim", Version)]
    public class SwapAnim : BaseUnityPlugin
    {
        public const string GUID = "SwapAnim";
        public const string Version = "1.0.0";

        private Harmony harmony;
        private static Dictionary<EMode, List<SwapAnimationInfo>> AnimationDict = new Dictionary<EMode, List<SwapAnimationInfo>>();
        private static SwapAnimationInfo swapAnimationInfo;
        private static List<HSceneProc.AnimationListInfo>[] lstAnimInfo;
        private static PositionCategory category;

        private void Awake()
        {
            harmony = Harmony.CreateAndPatchAll(typeof(SwapAnim), nameof(SwapAnim));
        }

        private void OnDestroy()
        {
            harmony.UnpatchAll(nameof(SwapAnim));
        }
        
#if DEBUG
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.RightControl))
            {
                Init();
            }
        }
#endif

        [HarmonyPostfix, HarmonyPatch(typeof(HSceneProc), "ChangeAnimator")]
        private static void post_HSceneProc_ChangeAnimator(HSceneProc.AnimationListInfo _nextAinmInfo)
        {
            SwapAnimation();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), "ChangeCategory")]
        private static void pre_HSceneProc_ChangeCategory(int _category)
        {
            category = (PositionCategory)_category;
        }

        //todo: not this
        private static HSceneProc.AnimationListInfo Clone_AnimationListInfo(HSceneProc.AnimationListInfo orig)
        {
            return JsonUtility.FromJson<HSceneProc.AnimationListInfo>(JsonUtility.ToJson(orig));
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
            if(_lstAnimInfo == null || _lstAnimInfo.Count == 0) return;
            var first = _lstAnimInfo[0];

            //TODO: scrollable list?
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

            if(!AnimationDict.TryGetValue(first.mode, out var swapAnimations)) return;

            foreach(var anim in swapAnimations.Where(x => (int)x.kindHoushi == first.kindHoushi && (!x.categories.Any() || x.categories.Contains(category))))
            {
                var tg = _objParent.GetComponent<ToggleGroup>();
                var btn = Instantiate(__instance.objMotionListNode, _objParent.transform, false);
                var aic = btn.AddComponent<HSprite.AnimationInfoComponent>();

                aic.info = Clone_AnimationListInfo(lstAnimInfo[(int)first.mode].First(x => x.id == anim.DonorPoseId));

                var label = btn.transform.Find("TextMeshPro Text").GetComponent<TextMeshProUGUI>();

                var image = btn.transform.FindLoop("Background").gameObject.GetComponent<Image>();
                image.color = new Color(0.7f, 0.9f, 1); //TODO: customise

                label.text = anim.AnimationName;
                label.color = new Color(0.5f, 0.5f, 1f); //TODO: customise

                //TODO: wat
                var tgl = btn.GetComponent<Toggle>();
                tgl.group = tg;
                tgl.enabled = false;
                tgl.enabled = true;

                //todo: hide all new indicators?
                var newIndicator = btn.transform.FindLoop("New");
                newIndicator.SetActive(false);

                btn.GetComponent<PointerAction>().listClickAction.Add(() =>
                {
                    //var nai = __instance.flags.nowAnimationInfo;
                    //force position change even if position appears to match. Prevents clicks from being eaten.
                    //if (nai.mode != aic.info.mode || nai.id != aic.info.id)
                    {
                        swapAnimationInfo = anim;
                        __instance.OnChangePlaySelect(btn);
                    }
                });
                btn.SetActive(true);
                if(__instance.flags.nowAnimationInfo == aic.info)
                    btn.GetComponent<Toggle>().isOn = true;
            }
        }

        private static void SwapAnimation()
        {
            if(swapAnimationInfo == null) return;

            var abF = AssetBundle.LoadFromFile(swapAnimationInfo.PathFemale);
            var racF = abF.LoadAllAssets<RuntimeAnimatorController>().First(x => x.animationClips.Length > 0 && x.animationClips[0] != null && !string.IsNullOrEmpty(x.animationClips[0].name)); //thanks omega/katarsys
            abF.Unload(false);

            var abM = AssetBundle.LoadFromFile(swapAnimationInfo.PathMale);
            var racM = abM.LoadAllAssets<RuntimeAnimatorController>().First(x => x.animationClips.Length > 0 && x.animationClips[0] != null && !string.IsNullOrEmpty(x.animationClips[0].name)); //thanks omega/katarsys
            abM.Unload(false);

            var t_hsp = Traverse.Create(Singleton<HSceneProc>.Instance);

            var female = t_hsp.Field<List<ChaControl>>("lstFemale").Value[0];
            var male = t_hsp.Field<ChaControl>("male").Value;
            ////TODO: lstFemale[1], male1

            female.animBody.runtimeAnimatorController = SetupAnimatorOverrideController(female.animBody.runtimeAnimatorController, racF);
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
            Init();
        }

        private static void Init()
        {
            // "master" list, can we still use this term
            lstAnimInfo = Traverse.Create(Singleton<HSceneProc>.Instance).Field<List<HSceneProc.AnimationListInfo>[]>("lstAnimInfo").Value;

            AnimationDict = new Dictionary<EMode, List<SwapAnimationInfo>>();

            //todo: manifests
            foreach(var f in new DirectoryInfo("anim_imports").GetFiles("*.json"))
            {
                var sai = JsonUtility.FromJson<SwapAnimationInfo>(File.ReadAllText(f.FullName));
                if(!AnimationDict.TryGetValue(sai.Mode, out var list))
                    AnimationDict[sai.Mode] = list = new List<SwapAnimationInfo>();
                list.Add(sai);
            }
        }

        private static AnimatorOverrideController SetupAnimatorOverrideController(RuntimeAnimatorController src, RuntimeAnimatorController over)
        {
            if(src == null || over == null) return null;
            var aoc = new AnimatorOverrideController(src);
            var target = new AnimatorOverrideController(over);
            foreach(var ac in src.animationClips.Where(x => x != null)) //thanks omega/katarsys
                aoc[ac.name] = ac;
            foreach(var ac in target.animationClips.Where(x => x != null)) //thanks omega/katarsys
                aoc[ac.name] = ac;
            aoc.name = over.name;
            return aoc;
        }
        
        public class SwapAnimationInfo
        {
            public string PathFemale;
            public string PathMale;

            public string AnimationName;

            public EMode Mode;
            public KindHoushi kindHoushi;
            public PositionCategory[] categories;
            public int DonorPoseId;
        }

        public enum KindHoushi
        {
            Hand = 0,
            Mouth = 1,
            Breasts = 2
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
        }
    }
}
