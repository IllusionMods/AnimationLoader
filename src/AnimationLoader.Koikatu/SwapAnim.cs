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

namespace AnimationLoader.Koikatu
{
    [BepInPlugin(nameof(SwapAnim), nameof(SwapAnim), "1.0")]
    public class SwapAnim : BaseUnityPlugin
    {
        //void Awake()
        //{
        //    var nai = Singleton<HSceneProc>.Instance.flags.nowAnimationInfo;
        //    Console.WriteLine($"{nai.id}, {nai.mode}, {nai.nameAnimation}, {nai.posture}, {nai.numCtrl}, {nai.kindHoushi}, {nai}");
        //    var lstAnimInfo = Traverse.Create(Singleton<HSceneProc>.Instance).Field<List<HSceneProc.AnimationListInfo>[]>("lstAnimInfo").Value;
        //    for (var i = 0; i < 10; i++)
        //    {
        //        File.WriteAllLines($"lst{i}.txt", lstAnimInfo[i].Select(x => $"{x.id}, {x.mode}, {x.nameAnimation}, {x.posture}, {x.numCtrl}, {x.kindHoushi}, {x}").ToArray());
        //    }
        //}

        internal class SwapAnimationInfo
        {
            public string PathFemale;
            public string PathMale;

            public string AnimationName;

            public EMode Mode;
            public KindHoushi kindHoushi; //0, 1, 2 -> Hand/Mouth/Breasts
            public PositionCategory[] categories; //0, 1, 2 -> Hand/Mouth/Breasts
            public int DonorPoseId;
        }

        internal enum KindHoushi
        {
            Hand = 0,
            Mouth = 1,
            Breasts = 2
        }

        internal enum PositionCategory
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

        Harmony hi = Harmony.CreateAndPatchAll(typeof(SwapAnim), nameof(SwapAnim));
        void OnDestroy() => hi.UnpatchAll(nameof(SwapAnim));

        private static Dictionary<EMode, List<SwapAnimationInfo>> AnimationDict = new Dictionary<EMode, List<SwapAnimationInfo>>();
        private static SwapAnimationInfo swapAnimationInfo;
        private static List<HSceneProc.AnimationListInfo>[] lstAnimInfo;

        private static PositionCategory category;

        [HarmonyPostfix, HarmonyPatch(typeof(HSceneProc), "ChangeAnimator")]
        static void post_HSceneProc_ChangeAnimator(HSceneProc.AnimationListInfo _nextAinmInfo)
        {
            SwapAnimation();
        }

        [HarmonyPrefix, HarmonyPatch(typeof(HSceneProc), "ChangeCategory")]
        static void pre_HSceneProc_ChangeCategory(int _category)
        {
            category = (PositionCategory)_category;
        }

        //todo: not this
        static HSceneProc.AnimationListInfo Clone_AnimationListInfo(HSceneProc.AnimationListInfo orig)
        {
            return JsonUtility.FromJson<HSceneProc.AnimationListInfo>(JsonUtility.ToJson(orig));
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(HSprite), "OnChangePlaySelect")]
        static IEnumerable<CodeInstruction> tpl_HSprite_OnChangePlaySelect(IEnumerable<CodeInstruction> instructions)
        {
            //force position change even if position appears to match. Prevents clicks from being eaten.
            return new CodeMatcher(instructions)
                .MatchForward(false, new[]
                {
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(HSprite), "flags")),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(HFlag), "nowAnimationInfo")),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(HSceneProc.AnimationListInfo), "id"))
                })
                .MatchForward(false, new[]
                {
                    new CodeMatch(OpCodes.Ret)
                })
                .SetAndAdvance(OpCodes.Nop, null)
                .InstructionEnumeration();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSprite), "LoadMotionList")]
        static void post_HSprite_LoadMotionList(HSprite __instance, List<HSceneProc.AnimationListInfo> _lstAnimInfo, GameObject _objParent)
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

            if(!AnimationDict.TryGetValue(first.mode, out List<SwapAnimationInfo> swapAnimations)) return;

            //Console.WriteLine(category);
            //Console.WriteLine(swapAnimations.Count);
            foreach(var anim in swapAnimations.Where(x => (int)x.kindHoushi == first.kindHoushi && (!x.categories.Any() || x.categories.Contains(category))))
            {
                var tg = _objParent.GetComponent<ToggleGroup>();
                var btn = Instantiate(__instance.objMotionListNode);
                var aic = btn.AddComponent<HSprite.AnimationInfoComponent>();

                aic.info = Clone_AnimationListInfo(lstAnimInfo[(int)first.mode].First(x => x.id == anim.DonorPoseId));

                btn.transform.SetParent(_objParent.transform, false);
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
                    var nai = __instance.flags.nowAnimationInfo;
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
            mi.Add(new MotionIK(female, null));
            mi.Add(new MotionIK(male, null));
            mi.ForEach(mik =>
            {
                mik.SetPartners(mi);
                mik.Reset();
            });

            swapAnimationInfo = null;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(HSceneProc), "CreateAllAnimationList")]
        static void post_HSceneProc_CreateAllAnimationList(HSceneProc __instance)
        {
            Init();
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.RightControl))
            {
                Init();
            }
        }

        static void Init()
        {
            // "master" list, can we still use this term
            lstAnimInfo = Traverse.Create(Singleton<HSceneProc>.Instance).Field<List<HSceneProc.AnimationListInfo>[]>("lstAnimInfo").Value;

            AnimationDict = new Dictionary<EMode, List<SwapAnimationInfo>>();

            //todo: manifests
            foreach(var f in new DirectoryInfo("anim_imports").GetFiles("*.json"))
            {
                var sai = JsonUtility.FromJson<SwapAnimationInfo>(File.ReadAllText(f.FullName));
                if(!AnimationDict.TryGetValue(sai.Mode, out List<SwapAnimationInfo> list))
                    AnimationDict[sai.Mode] = (list = new List<SwapAnimationInfo>());
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
    }
}
