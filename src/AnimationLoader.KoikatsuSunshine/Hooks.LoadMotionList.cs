using System;
using System.Collections.Generic;
using System.Linq;

using TMPro;
using UnityEngine;
using UnityEngine.UI;

using IllusionUtility.GetUtility;
using SceneAssist;

using HarmonyLib;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal static UsedAnimations _usedAnimations = new();
        internal static Dictionary<int, Dictionary<int, int>> _dicExpAddTaii = new();
        internal static Dictionary<string,
            Dictionary<int, Dictionary<string, int>>> _alDicExpAddTaii = new();

        internal partial class Hooks
        {
            /// <summary>
            /// In order to expand more easily had to replicate the original function and 
            /// adjust to needs.
            /// 
            /// TODO: Work on grid UI this is a maybe.
            /// 
            /// </summary>
            /// <param name="__instance"></param>
            /// <param name="_lstAnimInfo"></param>
            /// <param name="_objParent"></param>
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSprite), nameof(HSprite.LoadMotionList))]
            private static void LoadMotionListPostfix(
                HSprite __instance,
                List<HSceneProc.AnimationListInfo> _lstAnimInfo,
                GameObject _objParent)
            {
                var buttonParent = _objParent.transform;
                var hExp = __instance.flags.lstHeroine[0].hExp;

                if (buttonParent.childCount > 0)
                {
                    // remove buttons
                    buttonParent.Cast<Transform>().ToList().ForEach(x => Destroy(x.gameObject));
                }

                if (_lstAnimInfo == null || _lstAnimInfo.Count == 0)
                {
                    return;
                }

                var toggleGroup = _objParent.GetComponent<ToggleGroup>();
                var animationOn = false;
                var isALAnim = false;

                // Loop through selected animations _lstAnimInfo only has the animations
                // selected according to category, experience, etc
                for (var index = 0; index < _lstAnimInfo.Count; ++index)
                {
                    isALAnim = false;
                    swapAnimationMapping.TryGetValue(_lstAnimInfo[index], out var swap);
                    if (swap is not null)
                    {
                        isALAnim = true;
                        if (!AnimationCheckOk(__instance, swap))
                        {
                            continue;
                        }
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

                    if (textMesh.TryGetComponent<TextMeshProUGUI>(out var label))
                    {
                        // Text label
                        label.text = animationInfoComponent.info.nameAnimation;
                        if (isALAnim)
                        {
                            label.color = Utilities.yellow;

                            if (swap is not null)
                            {
#if DEBUG
                                var tmp = $"  E({swap.ExpTaii})";
                                label.text = $"{animationInfoComponent.info.nameAnimation}{tmp}";
#endif
                                if (HighLight.Value)
                                {
                                    if (swap.MotionIKDonor > 0)
                                    {
                                        // MotionIK information
                                        label.color = Utilities.gold;
                                    }
                                    else if((swap.ExpTaii >= 5) && (hExp < swap.ExpTaii))
                                    {
                                        // Foreground color yellow for loaded animations
                                        //label.color = Utilities.orange;
                                        label.color = Utilities.pink;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var tmp = Utilities.GetExpTaii((int)animationInfoComponent.info.mode,
                                animationInfoComponent.info.id);
#if DEBUG                           
                            label.text = $"{Utilities.TranslateName(animationInfoComponent.info.nameAnimation)}  E({tmp})";
#endif
                            if (HighLight.Value)
                            {
                                // Emphasize effects of the store plug-in
                                if ((tmp >= 50) && (hExp < tmp))
                                {
                                    label.color = Utilities.cyan;
                                }
                                var c = animationInfoComponent.info.lstCategory;
                                if ((c[0].category == 12) || (c[0].category >= 1000))
                                {
                                    label.color = Utilities.lime;
                                }
                            }
                        }

                    }

                    var toggle = button.GetComponent<Toggle>();

                    if ((toggle is not null) && (toggleGroup is not null))
                    {
                        // Assign button to toggle group 
                        toggle.group = toggleGroup;  
                        // ?? Magic operations
                        toggle.enabled = false;
                        toggle.enabled = true;
                    }

                    // Look for New label
                    var newLabel = button.transform.FindLoop("New");

                    // Activate New label according to previous usage saved in playHlist
                    if ((bool)(UnityEngine.Object)newLabel)
                    {
                        if (isALAnim)
                        {
                            // Manage new status for loaded animations it depended 
                            if (_usedAnimations.Keys.Count > 0)
                            {
                                newLabel.SetActive(!_usedAnimations.Keys.Contains(
                                    GetAnimationKey(animationInfoComponent.info)));
                            }
                            if (__instance.flags.isFreeH && EnableAllFreeH.Value)
                            {
                                newLabel.SetActive(false);
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
                            var playHlist = Manager.Game.globalData.playHList;
                            
                            if (!playHlist.TryGetValue((int)animationInfoComponent.info.mode, out var intSet))
                            {
                                // Add missing category
                                playHlist[(int)animationInfoComponent.info.mode] = intSet = new HashSet<int>();
                            }
                            // Show new if animation is not in used animation list.
                            newLabel.SetActive(!intSet.Contains(animationInfoComponent.info.id));
                        }
                    }

                    var nowPosition = button.transform.FindLoop("NowPos");
                    
                    if (nowPosition is not null)
                    {
                        // Check animation to see if it correspond to current catogory
                        var foundCategory = false;
                        foreach (var category in _lstAnimInfo[index].lstCategory)
                        {
                            if (__instance.lstCategory.Contains(category.category))
                            {
                                foundCategory = true;
                                break;
                            }
                        }
                        // Turn on change category icon if animation categories
                        // not have the current category
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
                        if (toggle is not null)
                        {
                            toggle.isOn = true;
                        }
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

                // sort buttons by name
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
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.LoadAddTaii), new Type[] { typeof(List<AddTaiiData.Param>) })]
            private static void LoadAddTaiiPostfix(object __instance, List<AddTaiiData.Param> param)
            {
                var hsceneTraverse = Traverse.Create(__instance);
                var dicExpAddTaii = hsceneTraverse
                    .Field<Dictionary<int, Dictionary<int, int>>>("dicExpAddTaii").Value;

                foreach (var item in dicExpAddTaii)
                {
                    if (_dicExpAddTaii != null)
                    {
                        if (!_dicExpAddTaii.ContainsKey(item.Key))
                        {
                            // Save the original dictionary for testing
                            _dicExpAddTaii.Add(item.Key, new Dictionary<int, int>(item.Value));
                        }
                    }
                }
            }

            internal static bool AnimationCheckOk(HSprite hsprite, SwapAnimationInfo anim)
            {
#if DEBUG
                if (TestMode.Value)
                {
                    return true;
                }
#endif
                if (hsprite.flags.isFreeH)
                {
                    if (EnableAllFreeH.Value)
                    {
                        return true;
                    }
                    // Only show used animations in FreeH
                    if (_usedAnimations.Keys.Count < 0)
                    {
                        return false;
                    }
                    if (!_usedAnimations.Keys.Contains(GetAnimationKey(anim)))
                    {
                        return false;
                    }
                }
                else if (UseAnimationLevels.Value && !CheckExperince(hsprite, anim))
                {
                    // Not enough experience
                    return false;
                }
                return true;
            }

            /// <summary>
            /// Checks heroine experience against ExpTaii of swap animation
            /// </summary>
            /// <param name="anim"></param>
            /// <returns></returns>
            internal static bool CheckExperince(HSprite hsprite, SwapAnimationInfo anim)
            {
                var hExp = hsprite.flags.lstHeroine[0].hExp;
                var expTaii = (double)anim.ExpTaii;

                if(_alDicExpAddTaii.ContainsKey(anim.Guid))
                {
                    if (_alDicExpAddTaii[anim.Guid].ContainsKey((int)anim.Mode)
                    && _alDicExpAddTaii[anim.Guid][(int)anim.Mode]
                        .ContainsKey($"{anim.ControllerFemale}{anim.StudioId}"))
                    { 
                        expTaii = _alDicExpAddTaii[anim.Guid][(int)anim.Mode][$"{anim.ControllerFemale}{anim.StudioId}"];
                    }
                    else
                    {
                        expTaii = -1;
                    }
                }
                if ((double)hExp >= expTaii)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
