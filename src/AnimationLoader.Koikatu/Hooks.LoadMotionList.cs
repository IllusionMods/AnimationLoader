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
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static HFlag;
using Manager;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal partial class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSprite), nameof(HSprite.LoadMotionList))]
            private static void LoadMotionList(
                HSprite __instance,
                List<HSceneProc.AnimationListInfo> _lstAnimInfo,
                GameObject _objParent)
            {
                if (_lstAnimInfo == null || _lstAnimInfo.Count == 0)
                {
                    return;
                }

                var buttonParent = _objParent.transform;
                Transform scrollT = null;
                if (vrType != null || UseGrid.Value)
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

                    var copyTarget = GameObject
                        .Find("Canvas").transform
                        .Find("clothesFileWindow/Window/WinRect/ListArea/Scroll " +
                            "View/Scrollbar Vertical").gameObject;
                    var newScrollbar = Instantiate(copyTarget, go.transform);
                    scroll.verticalScrollbar = newScrollbar.GetComponent<Scrollbar>();
                    newScrollbar.transform.SetRect(1f, 0f, 1f, 1f, 0f, 0f, 18f);

                    var triggerEvent = new EventTrigger.TriggerEvent();
                    triggerEvent.AddListener(x => GlobalMethod.SetCameraMoveFlag(
                        __instance.flags.ctrlCamera,
                        false));
                    var eventTrigger = newScrollbar.AddComponent<EventTrigger>();
                    eventTrigger.triggers.Add(new EventTrigger.Entry { 
                        eventID = EventTriggerType.PointerDown, callback = triggerEvent });

                    var vlg = _objParent.GetComponent<VerticalLayoutGroup>();
                    var csf = _objParent.GetComponent<ContentSizeFitter>();
                    vlg.enabled = false;
                    csf.enabled = false;
                    CopyComponent(vlg, scroll.content.gameObject).enabled = true;
                    CopyComponent(csf, scroll.content.gameObject).enabled = true;

                    // remove the buttons as we're going to rebuild the entire list
                    buttons.ForEach(x => Destroy(x.gameObject));

                    buttonParent = scroll.content;
                    scrollT = scroll.gameObject.transform;
                }

                foreach (var anim in _lstAnimInfo)
                {
                    var btn = Instantiate(__instance.objMotionListNode, buttonParent, false);
                    btn.AddComponent<HSprite.AnimationInfoComponent>().info = anim;
                    var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                    label.text = anim.nameAnimation;
                    label.color = Color.black;

                    //TODO: wat
                    var tgl = btn.GetComponent<Toggle>();
                    tgl.group = _objParent.GetComponent<ToggleGroup>();
                    tgl.enabled = false;
                    tgl.enabled = true;

                    btn.GetComponent<SceneAssist.PointerAction>().listClickAction.Add(() =>
                    {
                        __instance.OnChangePlaySelect(btn);
                    });

                    btn.SetActive(true);
                    if (__instance.flags.nowAnimationInfo == anim)
                    {
                        btn.GetComponent<Toggle>().isOn = true;
                    }

                    swapAnimationMapping.TryGetValue(anim, out var swap);
                    if (swap != null)
                    {
                        btn.transform
                            .FindLoop("Background").GetComponent<Image>().color = buttonColor;
                        label.text = swap.AnimationName;
                    }
                }

                // order all buttons by name
                var allButtons = buttonParent.Cast<Transform>()
                    .OrderBy(x => x.GetComponentInChildren<TextMeshProUGUI>().text).ToList();
                foreach (var t in allButtons)
                {
                    // disable New text
                    var newT = t.FindLoop("New");
                    if (newT)
                    {
                        newT.gameObject.SetActive(false);
                    }

                    if (SortPositions.Value)
                    {
                        t.SetAsLastSibling();
                    }

                    var textMeshGo = t.FindLoop("TextMeshPro Text").gameObject;
                    var textMesh = textMeshGo.GetComponent<TextMeshProUGUI>();
                    textMesh.enableWordWrapping = false;
                    textMesh.overflowMode = TextOverflowModes.Overflow; // disable ... after text

                    // add scrolling text if text is long enough
                    var rectT = (RectTransform)t;
                    if (rectT.sizeDelta.x < textMesh.preferredWidth)
                    {
                        textMesh.alignment = TextAlignmentOptions.CaplineLeft;

                        var txtScroll = textMeshGo.AddComponent<TextScroll>();
                        txtScroll.textMesh = textMesh;
                        txtScroll.transBase = rectT;
                    }
                }

                if (scrollT != null && allButtons.Count > 8)
                {
                    scrollT.SetRect(0f, 0f, 1f, 1f, -5f, -100f, -5f, 100f);
                }
            }
        }

        private static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            var type = original.GetType();
            var dst = destination.GetComponent(type) as T;
            if (!dst)
            {
                dst = destination.AddComponent(type) as T;
            }

            var fields = type.GetFields();
            foreach (var field in fields)
            {
                if (field.IsStatic)
                {
                    continue;
                }

                field.SetValue(dst, field.GetValue(original));
            }
            var props = type.GetProperties();
            foreach (var prop in props)
            {
                if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name")
                {
                    continue;
                }

                prop.SetValue(dst, prop.GetValue(original, null), null);
            }
            return dst;
        }
    }
}
