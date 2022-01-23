using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IllusionUtility.GetUtility;
using Manager;
using SceneAssist;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

using HarmonyLib;

using Newtonsoft.Json;
using static HScene.AddParameter;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal static UsedAnimations _usedAnimations = new();

        internal partial class Hooks
        {
            /// <summary>
            /// The functionality of this patch differs to much between versions.
            /// The method just highlight the animation and sort the list if selected.
            /// The is no grid UI implementation if needed for VR can be implemented.
            /// There is no scroll text functions these are not needed for KKS.
            /// 
            /// The stated previously is no more.  In order to expand more easily had to replicate
            /// the original function and adjust to needs.
            /// TODO: Work on grid UI.
            /// 
            /// </summary>
            /// <param name="__instance"></param>
            /// <param name="_lstAnimInfo"></param>
            /// <param name="_objParent"></param>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSprite), nameof(HSprite.LoadMotionList))]
            private static void LoadMotionList(
                HSprite __instance,
                List<HSceneProc.AnimationListInfo> _lstAnimInfo,
                GameObject _objParent)
            {
                var buttonParent = _objParent.transform;

                if (buttonParent.childCount > 0)
                {
                    buttonParent.Cast<Transform>().ToList().ForEach(x => Destroy(x.gameObject));
#if DEBUG
                    Log.Info($"0024: Destroy Buttons");
#endif
                }
                if (_lstAnimInfo == null || _lstAnimInfo.Count == 0)
                {
                    return;
                }

                //var buttons = _objParent.transform.Cast<Transform>().ToList();
                var toggleGroup = _objParent.GetComponent<ToggleGroup>();
                var animationOn = false;
                var isALAnim = false;

                for (var index = 0; index < _lstAnimInfo.Count; ++index)
                {
                    isALAnim = false;
                    swapAnimationMapping.TryGetValue(_lstAnimInfo[index], out var swap);
                    if (swap is not null)
                    {
                        isALAnim = true;
                    }

                    var button = Instantiate<GameObject>(__instance.objMotionListNode);

                    var animationInfoComponent =
                        button.AddComponent<HSprite.AnimationInfoComponent>();

                    // Assign animation
                    animationInfoComponent.info = _lstAnimInfo[index];
                    // Assign button to parent transform
                    button.transform.SetParent(buttonParent, false);

                    // TextMesh of button
                    var textMesh = button.transform.FindLoop("TextMeshPro Text");
                    var label = textMesh.GetComponent<TextMeshProUGUI>();

                    if (label != null)
                    {
                        // Text label
                        label.text = animationInfoComponent.info.nameAnimation;
                        if (isALAnim)
                        {
                            // Foreground color yellow for loaded animations
                            label.color = Color.yellow;
                        }
                    }

                    var toggle = button.GetComponent<Toggle>();

                    if ((toggle is not null) && (toggleGroup is not null))
                    {
                        toggle.group = toggleGroup;  // Assign button to toggle group 
                        // Magic operations
                        toggle.enabled = false;
                        toggle.enabled = true;
                    }

                    var newLabel = button.transform.FindLoop("New"); // Look for New label

                    // Activate New label according to previous usage saved in playHlist
                    if ((bool)(UnityEngine.Object)newLabel)
                    {
                        if (isALAnim)
                        {
                            // TODO: Lookup animation key in list of used animations.
                            // Will maintain new status.
                            if (_usedAnimations.Keys.Count > 0)
                            {
                                newLabel.SetActive(!_usedAnimations.Keys.Contains(
                                    AnimationInfo.GetKey(animationInfoComponent.info)));
                            }
                        }
                        else
                        {
                            //
                            // This dictionary is filled with the id's of animations used at least
                            // once by category
                            //
                            // playHList =
                            //    {0:[0,16,4,3,7],
                            //     1:[1,33,32,36,59,63,17],
                            //     2:[44,23,3,12,8,19,9,47,40,1,49,42],
                            //     ..}
                            //
                            //Dictionary<int, HashSet<int>> playHlist = Manager.Game.globalData.playHList;
                            var playHlist = Manager.Game.globalData.playHList;
                            // HashSet<int> intSet;
                            if (!playHlist.TryGetValue((int)animationInfoComponent.info.mode, out var intSet))
                            {
                                playHlist[(int)animationInfoComponent.info.mode] = intSet = new HashSet<int>();
                            }
                            newLabel.SetActive(!intSet.Contains(animationInfoComponent.info.id));
                        }
                    }

                    // Check where are the characters to see what categories
                    // are available
                    var nowPosition = button.transform.FindLoop("NowPos");

                    if (nowPosition is not null)
                    {
                        var foundCategory = false;
                        // Check to see if animation is enabled for current category
                        foreach (var category in _lstAnimInfo[index].lstCategory)
                        {
                            if (__instance.lstCategory.Contains(category.category))
                            {
                                foundCategory = true;
                                break;
                            }
                        }
                        nowPosition.SetActive(!foundCategory);
                    }

                    // Assign actions to take on user interaction
                    button.GetComponent<PointerAction>().listClickAction.Add(
                        () => __instance.OnChangePlaySelect(button));
                    button.GetComponent<PointerAction>().listDownAction.Add(
                        () => __instance.OnMouseDownSlider());
                    button.SetActive(true);

                    // Highlight current animation
                    if (_lstAnimInfo[index] == __instance.flags.nowAnimationInfo)
                    {
                        toggle.isOn = true;
                        animationOn = true;
                    }
                }

                if (!animationOn)
                {
                    for (var index = 0; index < _objParent.transform.childCount; ++index)
                    {
                        var componentInChildren =
                            _objParent.transform
                                .GetChild(index)
                                .GetComponentInChildren<SpriteClickChangeCtrl>();
                        if (componentInChildren != null)
                        {
                            componentInChildren.OnChangeValueEnable(false);
                        }
                    }
                }

                // sort all buttons by name
                if (SortPositions.Value)
                {
                    var allButtons = buttonParent.Cast<Transform>().OrderBy(
                       x => x.GetComponentInChildren<TextMeshProUGUI>().text).ToList();
                    foreach (var t in allButtons)
                    {
                        t.SetAsLastSibling();
                    }
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.CreateListAnimationFileName))]
            private static void WhatIsThis(
                object __instance, bool _isAnimListCreate = true)
            {
                PPrintUseAnimInfo(__instance, _isAnimListCreate);

                return;

                /*var hsceneTraverse = Traverse.Create(__instance);

                var flags = hsceneTraverse
                    .Field<HFlag>("flags").Value;
                var hExp = flags.lstHeroine[0].hExp;

                //var lstUseAnimInfo = Traverse
                //    .Create(__instance)
                //    .Field<List<HSceneProc.AnimationListInfo>[]>("lstUseAnimInfo").Value;

                var categorys = hsceneTraverse
                    .Field<List<int>>("categorys").Value;
                var useCategorys = hsceneTraverse
                    .Field<List<int>>("useCategorys").Value;
                var lstAnimInfo = hsceneTraverse
                    .Field<List<HSceneProc.AnimationListInfo>[]>("lstAnimInfo").Value;
                var lstUseAnimInfo = new List<HSceneProc.AnimationListInfo>[8];
                //var theThis = (HSceneProc)__instance;
 
                var checkExpAddTaii = hsceneTraverse
                    .Method("CheckExpAddTaii", 
                        new Type[] { typeof(int), typeof(int), typeof(float) });
                var checkShopAdd = hsceneTraverse
                    .Method("CheckShopAdd",
                        new Type[] { typeof(HashSet<int>), typeof(int), typeof(int) });

                var saveData = Game.saveData;
                var playHlist = Game.globalData.playHList;

                // Test for range 1010-1099 and 1100-1199
                var flagRange1 = categorys.Any<int>((Func<int, bool>)(c =>
                    MathfEx.IsRange<int>(1010, c, 1099, true)
                    || MathfEx.IsRange<int>(1100, c, 1199, true)));

                // Test for range 3000-3099
                var flagRange2 = categorys.Any<int>((Func<int, bool>)(c =>
                    MathfEx.IsRange<int>(3000, c, 3099, true)));

                // index1 loop through categories aibu, hoshi, sonyu, ...
                for (var index1 = 0; index1 < lstAnimInfo.Length; ++index1)
                {
                    // clear animations in category index1
                    lstUseAnimInfo[index1] = new List<HSceneProc.AnimationListInfo>();
                    HashSet<int> intSet;

                    // get list of already used animations for category index1
                    if (!playHlist.TryGetValue(index1, out intSet))
                    {
                        intSet = new HashSet<int>();
                    }

                    // if not Free-h or inset.count != 0 or in range1 or in range2
                    if (!flags.isFreeH || intSet.Count != 0 || flagRange1 || flagRange2)
                    {
                        // index2 loop through animations in category index1
                        for (var index2 = 0; index2 < lstAnimInfo[index1].Count; ++index2)
                        {
                            var anim = lstAnimInfo[index1][index2];
                            // Not clear: is animation in range1 continue
                            if (flagRange1)
                            {
                                if (!lstAnimInfo[index1][index2].lstCategory
                                    .Any<HSceneProc.Category>(
                                        (Func<HSceneProc.Category, bool>)
                                        (c => categorys.Contains(c.category))))
                                {
                                    continue;
                                }
                            }
                            // Animation 
                            else if (!lstAnimInfo[index1][index2].lstCategory
                                .Any<HSceneProc.Category>(
                                    (Func<HSceneProc.Category, bool>)
                                    (c => useCategorys.Contains(c.category))))
                            {
                                continue;
                            }

                            if (!flags.isFreeH)
                            {
                                if (((!lstAnimInfo[index1][index2].isRelease ?
                                    0 : (!checkExpAddTaii.GetValue<bool>(
                                            index1,
                                            lstAnimInfo[index1][index2].id,
                                            flags.lstHeroine[0].hExp) ? 1 : 0))
                                      | (!checkShopAdd.GetValue<bool>(
                                            new HashSet<int>((IEnumerable<int>)saveData.player.buyNumTable.Keys),
                                            index1,
                                            lstAnimInfo[index1][index2].id) ? 1 : 0)) != 0 
                                            || lstAnimInfo[index1][index2].isExperience != 2 
                                            && (HSceneProc.EExperience)lstAnimInfo[index1][index2].isExperience > flags.experience)
                                {
                                    continue;
                                }
                            }
                            else if ((SaveData.Heroine.HExperienceKind)lstAnimInfo[index1][index2].stateRestriction 
                                        > flags.lstHeroine[0].HExperience 
                                     || !intSet.Contains(lstAnimInfo[index1][index2].id) 
                                     && !flagRange1 
                                     && !flagRange2)
                            {
                                continue;
                            }
                            lstUseAnimInfo[index1].Add(lstAnimInfo[index1][index2]);
                        }
                    }
                }*/
            }

            private static void PPrintUseAnimInfo(
                object __instance, bool _isAnimListCreate = true)
            {
                var lines = new StringBuilder();

                var hsceneTraverse = Traverse.Create(__instance);

                var flags = hsceneTraverse
                    .Field<HFlag>("flags").Value;
                var hExp = flags.lstHeroine[0].hExp;
                var dicExpAddTaii = hsceneTraverse
                    .Field<Dictionary<int, Dictionary<int, int>>>("dicExpAddTaii").Value;
                lines.Append($"Heroine: {flags.lstHeroine[0].Name} with Experience {hExp}\n");

                var lstUseAnimInfo = hsceneTraverse
                    .Field<List<HSceneProc.AnimationListInfo>[]>("lstUseAnimInfo").Value;
                var dictTaii = JsonConvert.SerializeObject(dicExpAddTaii);
                var categorys = hsceneTraverse.Field<List<int>>("categorys").Value;
                var strCategories = Utilities.CategoryList(categorys, true, false);
                var playHlist = JsonConvert.SerializeObject(Game.globalData.playHList);

                if (_isAnimListCreate)
                {
                    lines.Append("Asked to create animation list\n");
                }

                lines.Append($"category: {strCategories}\n");
                lines.Append($"dictExpAddTaii:\n{dictTaii}\n");
                lines.Append($"playHList:\n{playHlist}\n");

                for (var i = 0; i < lstUseAnimInfo.Length; ++i)
                {
                    for (var j = 0; j < lstUseAnimInfo[i].Count; ++j)
                    {
                        var anim = lstUseAnimInfo[i][j];
                        var prefix = AnimationInfo.IsAnimationLoader(anim) ? "AL" : "GA";
                        lines.Append($"{prefix}-{i}-{anim.mode,-6}-{anim.id:D2}-{anim.sysTaii:D2}-" +
                            $"{anim.stateRestriction:D2} {Utilities.TranslateName(anim.nameAnimation)} - " +
                            $"{Utilities.CategoryList(anim.lstCategory, true, false)}\n");
                    }
                }

                lines.Append('\n');
                var countGameA = Utilities.CountAnimations(lstUseAnimInfo);
                lines.Append($"Used Animations - {countGameA}\n");

                Log.Info($"0025: [CreateListAnimationFileName] Selected animations:\n\n{lines}\n");
            }
        }
    }
}
