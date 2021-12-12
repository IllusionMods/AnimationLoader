using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using IllusionUtility.GetUtility;

using TMPro;

using UnityEngine;
using UnityEngine.UI;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal partial class Hooks
        {
            /// <summary>
            /// The functionality of this patch differs to much between versions.
            /// The method just highlight the animation and sort the list if selected.
            /// The is no grid UI implementation if needed for VR can be implemented.
            /// There is no scroll text functions these are not needed for KKS.
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
                if (_lstAnimInfo == null || _lstAnimInfo.Count == 0)
                    return;

                int countKKS = 0;
                int countAL = 0;
                var buttonParent = _objParent.transform;
                List<Transform> buttons = _objParent.transform.Cast<Transform>().ToList();

                foreach (var button in buttons)
                {
                    // button.gameObject.SetActive(false);
                    var label = button.GetComponentInChildren<TextMeshProUGUI>();

                    try
                    {
                        var anim = button
                            .GetComponentInChildren<HSprite.AnimationInfoComponent>().info;

                        if (anim != null)
                        {
#if DEBUG
                            Logger.LogWarning($"Loaded button =" +
                                $" {Utilities.Translate(anim.nameAnimation)}");
#endif
                            swapAnimationMapping.TryGetValue(anim, out var swap);
                            if (swap != null)
                            {
                                //
                                // swap.Guid, swap.StudioId key for loaded animation
                                //
                                button.transform
                                    .FindLoop("Background")
                                    .GetComponent<Image>().color = buttonColor;
                                label.color = Color.yellow;
                                countAL++;
                            }
                            else
                            {
                                countKKS++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Logger.LogInfo(ex);
#endif
                    }
                }
#if DEBUG
                Logger.LogWarning($"System animations {countKKS} Animation Loader {countAL}");
#endif
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
