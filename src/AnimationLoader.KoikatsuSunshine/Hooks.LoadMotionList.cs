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

                if (animationDict.Count == 0)
                {
                    // Let game code handle this if no animations loaded
#if DEBUG
                    Log.Warning($"0020: No animations loaded.");
#endif
                    return;
                }

                Log.Warning($"XXXX: [LoadMotionListPostfix] Scene=[{Manager.Scene.ActiveScene.name}] " +
                    $"Categories=[{Utilities.CategoryList(__instance.lstCategory, quotes: false)}]");

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
#if DEBUG
                            var tmp = (swap is not null) ? $" E({swap.ExpTaii})" : "";
                            label.text = $"{animationInfoComponent.info.nameAnimation}{tmp}";
#endif
                            // Foreground color yellow for loaded animations
                            label.color = Color.yellow;
                        }
#if DEBUG && TEST
                        else
                        {
                            var tmp = Utilities.GetExpTaii((int)animationInfoComponent.info.mode, 
                                animationInfoComponent.info.id);
                            label.text = $"{animationInfoComponent.info.nameAnimation} E({tmp})";
                        }
#endif

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
                                    AnimationInfo.GetKey(animationInfoComponent.info)));
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
                    if (!_usedAnimations.Keys.Contains(AnimationInfo.GetKey(anim)))
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

                if ((double)hExp >= (double)anim.ExpTaii)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
