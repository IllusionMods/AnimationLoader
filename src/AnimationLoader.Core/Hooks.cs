using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

using HarmonyLib;

using Illusion.Extensions;

using Manager;

using Sideloader.AutoResolver;

using Studio;

using UnityEngine;

#if DEBUG
using Newtonsoft.Json;
#endif


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal static Harmony _hookInstance;
        //internal static HFlag.EMode _mode;

        internal partial class Hooks
        {
            /// <summary>
            /// Initialize the Hooks patch instance
            /// </summary>
            internal static void Init()
            {
                _hookInstance = Harmony.CreateAndPatchAll(typeof(Hooks), nameof(Hooks));

                if (vrType != null)
                {
                    _hookInstance.Patch(
                        AccessTools.Method(
                            vrType,
                            nameof(HSceneProc.ChangeAnimator)),
                            postfix: new HarmonyMethod(typeof(Hooks),
                            nameof(SwapAnimation)));
                    _hookInstance.Patch(
                        AccessTools.Method(
                            vrType,
                            nameof(HSceneProc.CreateAllAnimationList)),
                            postfix: new HarmonyMethod(typeof(Hooks),
                            nameof(ExtendList)));
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.CreateAllAnimationList))]
            private static void ExtendList(object __instance)
            {
                // add new animations to the complete list
                Utilities.SaveHProcInstance(__instance);
#if KK
                var hlist = Singleton<Game>.Instance.glSaveData.playHList;
#elif KKS
                var hlist = Game.globalData.playHList;
#endif
                var lstAnimInfo = Traverse
                    .Create(__instance)
                    .Field<List<HSceneProc.AnimationListInfo>[]>("lstAnimInfo").Value;
                swapAnimationMapping = 
                    new Dictionary<HSceneProc.AnimationListInfo, SwapAnimationInfo>();
#if DEBUG
                var countGameA = 0;
                var countAL = 0;
                countGameA = Utilities.CountAnimations(lstAnimInfo);
#endif
                foreach (var anim in animationDict.SelectMany(
                    e => e.Value,
                    (e, a) => a
                ))
                {
                    var mode = (int)anim.Mode;
                    if (mode < 0 || mode >= lstAnimInfo.Length)
                    {
                        continue;
                    }
                    var animListInfo = lstAnimInfo[(int)anim.Mode];

                    var donorInfo = animListInfo
                        .FirstOrDefault(x => x.id == anim.DonorPoseId)?.DeepCopy();

                    if (donorInfo == null)
                    {
                        Logger.LogWarning($"0001: No donor: {anim.Mode} {anim.DonorPoseId}");
                        continue;
                    }

                    if (anim.NeckDonorId >= 0 && anim.NeckDonorId != anim.DonorPoseId)
                    {
                        donorInfo.paramFemale.fileMotionNeck = 
                            animListInfo
                                .First(x => x.id == anim.NeckDonorId).paramFemale.fileMotionNeck;
                    }
                    if (anim.FileMotionNeck != null)
                    {
                        donorInfo.paramFemale.fileMotionNeck = anim.FileMotionNeck;
                    }
                    if (anim.IsFemaleInitiative != null)
                    {
                        donorInfo.isFemaleInitiative = anim.IsFemaleInitiative.Value;
                    }
                    
                    /*
                    if (anim.FileSiruPaste != null && SiruPasteFiles
                        .TryGetValue(anim.FileSiruPaste.ToLower(), out var fileSiruPaste))
                    {

                        donorInfo.paramFemale.fileSiruPaste = fileSiruPaste;
                    }
                    */

                    if (!string.IsNullOrEmpty(anim.FileSiruPaste))
                    {
                        // Check if FileSuruPaset is on dictionary first
                        if (SiruPasteFiles.TryGetValue(
                            anim.FileSiruPaste.ToLower(), out var fileSiruPaste))
                        {
                            donorInfo.paramFemale.fileSiruPaste = fileSiruPaste;
                        }
                        else
                        {
                            donorInfo.paramFemale.fileSiruPaste = anim.FileSiruPaste.ToLower();
                        }
                    }

                    donorInfo.lstCategory = anim.categories.Select(c =>
                        {
                            var cat = new HSceneProc.Category
                            {
                                category = (int)c
                            };
                            return cat;
                        }).ToList();
                    Logger.LogDebug($"0002: Adding animation {anim.AnimationName} to EMode " +
                        $"{ anim.Mode} Key {anim}");
#if KKS
                    // Update name so it shows on button text label
                    donorInfo.nameAnimation = anim.AnimationName;
#endif
#if DEBUG
                    countAL++;
#endif
                    animListInfo.Add(donorInfo);
                    swapAnimationMapping[donorInfo] = anim;
                }
#if DEBUG
                Logger.LogWarning($"0003: Added {countAL + countGameA} animations: Game " +
                    $"standard - {countGameA} " +
                    $"AnimationLoader - {countAL}");
                //Logger.LogError($"0004: Added {countAL + countKKS} animations: KKS - {countKKS} " +
                //$"AnimationLoader - {countAL}\n\n{JsonConvert.SerializeObject(lstAnimInfo)}\n\n"
                //$"{JsonConvert.SerializeObject(swapAnimationMapping)}");
                // Saves information used in the templates
                Utilities.SaveAnimInfo();
#endif
            }

            [HarmonyTranspiler]
            [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnChangePlaySelect))]
            private static IEnumerable<CodeInstruction> OnChangePlaySelect(
                IEnumerable<CodeInstruction> instructions)
            {
                // Force position change even if position appears to match.
                // Prevents clicks from being eaten.
                return new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(
                            OpCodes.Ldfld,
                            AccessTools.Field(typeof(HSprite), "flags")),
                        new CodeMatch(
                            OpCodes.Ldfld,
                            AccessTools.Field(typeof(HFlag),
                            "nowAnimationInfo")),
                        new CodeMatch(
                            OpCodes.Ldfld,
                            AccessTools.Field(typeof(HSceneProc.AnimationListInfo), "id"))
                    )
                    .MatchForward(false,
                        new CodeMatch(OpCodes.Ret)
                    )
                    .SetAndAdvance(OpCodes.Nop, null)
                    .InstructionEnumeration();
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.ChangeAnimator))]
            private static void SwapAnimation(
                object __instance,
                HSceneProc.AnimationListInfo _nextAinmInfo)
            {
                if (!swapAnimationMapping.TryGetValue(_nextAinmInfo, out var swapAnimationInfo))
                {
                    return;
                }

                RuntimeAnimatorController femaleCtrl = null;
                RuntimeAnimatorController maleCtrl = null;
                if (!string.IsNullOrEmpty(swapAnimationInfo.PathFemale)
                    || !string.IsNullOrEmpty(swapAnimationInfo.ControllerFemale))
                {
                    femaleCtrl = AssetBundleManager.LoadAsset(
                        swapAnimationInfo.PathFemale,
                        swapAnimationInfo.ControllerFemale,
                        typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
                }
                if (!string.IsNullOrEmpty(swapAnimationInfo.PathMale)
                    || !string.IsNullOrEmpty(swapAnimationInfo.ControllerMale))
                {
                    maleCtrl = AssetBundleManager.LoadAsset(
                        swapAnimationInfo.PathMale,
                        swapAnimationInfo.ControllerMale,
                        typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
                }
                var t_hsp = Traverse.Create(__instance);
                var female = t_hsp.Field<List<ChaControl>>("lstFemale").Value[0];
                var male = t_hsp.Field<ChaControl>("male").Value;
                ////TODO: lstFemale[1], male1

                if (femaleCtrl != null)
                {
                    female.animBody.runtimeAnimatorController = SetupAnimatorOverrideController(
                        female.animBody.runtimeAnimatorController,
                        femaleCtrl);
                }
                if (maleCtrl != null)
                {
                    male.animBody.runtimeAnimatorController = SetupAnimatorOverrideController(
                        male.animBody.runtimeAnimatorController, maleCtrl);
                }

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
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Studio.Info), nameof(Studio.Info.LoadExcelDataCoroutine))]
            private static void LoadStudioAnims(Studio.Info __instance, ref IEnumerator __result)
            {
                __result = __result.AppendCo(() =>
                {
                    foreach (var keyVal in animationDict)
                    {
                        CreateGroup(0);
                        CreateGroup(1);

                        void CreateGroup(byte sex)
                        {
                            var grp = new Info.GroupInfo 
                                { name = $"AL {(sex == 0 ? "M" : "F")} {keyVal.Key}" };
                            var animGrp = 
                                new Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>();

                            var grpKey = $"{keyVal.Key}{sex}";
                            if (!EModeGroups.TryGetValue(grpKey, out var grpId))
                            {
                                return;
                            }

                            foreach (var swapAnimInfo in keyVal.Value.Where(x => x.StudioId >= 0))
                            {
                                var path = 
                                    sex == 0 ? swapAnimInfo.PathMale
                                    : swapAnimInfo.PathFemale;
                                var ctrl = 
                                    sex == 0 ? swapAnimInfo.ControllerMale
                                    : swapAnimInfo.ControllerFemale;

                                if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(ctrl))
                                {
                                    continue;
                                }

                                var controller = AssetBundleManager.LoadAsset(
                                    path, 
                                    ctrl,
                                    typeof(RuntimeAnimatorController))
                                        .GetAsset<RuntimeAnimatorController>();
                                if (controller == null)
                                {
                                    continue;
                                }

                                var animName = string.IsNullOrEmpty(swapAnimInfo.AnimationName) ?
                                    ctrl : swapAnimInfo.AnimationName;
                                grp.dicCategory.Add(swapAnimInfo.StudioId, animName);
                                var animCat = new Dictionary<int, Info.AnimeLoadInfo>();
                                animGrp.Add(swapAnimInfo.StudioId, animCat);

                                var clips = controller.animationClips;
                                for (var i = 0; i < clips.Length; i++)
                                {
                                    var newSlot = UniversalAutoResolver.GetUniqueSlotID();

                                    UniversalAutoResolver.LoadedStudioResolutionInfo
                                        .Add(new StudioResolveInfo
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

                            if (animGrp.Count > 0)
                            {
                                __instance.dicAGroupCategory.Add(grpId, grp);
                                __instance.dicAnimeLoadInfo.Add(grpId, animGrp);
                            }
                        }
                    }
                });
            }
        }
    }
}
