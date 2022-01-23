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
                        if (__instance.flags.isFreeH)
                        {
                            if (_usedAnimations.Keys.Count < 0)
                            {
                                continue;
                            }
                            if (!_usedAnimations.Keys.Contains(AnimationInfo.GetKey(_lstAnimInfo[index])))
                            {
                                continue;
                            }
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
        }
    }
}
