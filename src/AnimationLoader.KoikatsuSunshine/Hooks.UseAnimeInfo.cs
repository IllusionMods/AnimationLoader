using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Manager;

using HarmonyLib;

using KKAPI;
using Newtonsoft.Json;



namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal partial class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.CreateListAnimationFileName))]
            private static void CreateListAnimationFileNamePostfix(
                object __instance, bool _isAnimListCreate = true)
            {
                PPrintUseAnimInfo(__instance, _isAnimListCreate);

                //return;

                var hsceneTraverse = Traverse.Create(__instance);
                var lines = new StringBuilder();
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
 
                var strInSet = string.Empty;

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
                    strInSet = JsonConvert.SerializeObject(intSet);

                    // if not Free-h or inset.count != 0 or in range1 or in range2
                    if (!flags.isFreeH || intSet.Count != 0 || flagRange1 || flagRange2)
                    {
                        // index2 loop through animations in category index1
                        for (var index2 = 0; index2 < lstAnimInfo[index1].Count; ++index2)
                        {
                            var anim = lstAnimInfo[index1][index2];
                            var prefix = AnimationInfo.IsAnimationLoader(anim) ? "AL" : "GA";
                            lines.Append($"{prefix}-{index1}-{anim.mode,-6}-{anim.id:D2}-{anim.sysTaii:D2}-" +
                                $"{anim.stateRestriction:D2} {Utilities.TranslateName(anim.nameAnimation)} - " +
                                $"{Utilities.CategoryList(anim.lstCategory, true, false)} ");
                            // Not clear: is animation in range1 continue
                            /*lines.Append(
                                $"Element=[{index1},{index2}]\n" +
                                $"inSet={strInSet}\n" +
                                $"flagRange1={flagRange1}\n" +
                                $"flagRange2={flagRange2}\n" +
                                $"isRelease={lstAnimInfo[index1][index2].isRelease}\n" +
                                $"checkExpAddTaii={checkExpAddTaii.GetValue<bool>(index1, lstAnimInfo[index1][index2].id, flags.lstHeroine[0].hExp)}\n" +
                                $"ceckShopAdd={checkShopAdd.GetValue<bool>(new HashSet<int>((IEnumerable<int>)saveData.player.buyNumTable.Keys), index1, lstAnimInfo[index1][index2].id)}\n" +
                                $"isExperience={lstAnimInfo[index1][index2].isExperience}\n" +
                                $"stateRestriction={lstAnimInfo[index1][index2].stateRestriction}\n" +
                                $"inSet.Contains({lstAnimInfo[index1][index2].id})={intSet.Contains(lstAnimInfo[index1][index2].id)}\n" +
                                $"HExperience={flags.lstHeroine[0].HExperience}\n" +
                                $"flags.experience {flags.experience}.\n");*/
                            if (flagRange1)
                            {
                                if (!lstAnimInfo[index1][index2].lstCategory
                                    .Any<HSceneProc.Category>(
                                        (Func<HSceneProc.Category, bool>)
                                        (c => categorys.Contains(c.category))))
                                {
                                    lines.Append($"Continue in first categorys range test.\n");
                                    continue;
                                }
                            }
                            // Animation 
                            else if (!lstAnimInfo[index1][index2].lstCategory
                                .Any<HSceneProc.Category>(
                                    (Func<HSceneProc.Category, bool>)
                                    (c => useCategorys.Contains(c.category))))
                            {
                                lines.Append($"Continue in useCategorys test.\n");
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
                                    lines.Append($"Continue not Free-H\n");
                                    continue;
                                }
                            }
                            else if ((SaveData.Heroine.HExperienceKind)lstAnimInfo[index1][index2].stateRestriction 
                                        > flags.lstHeroine[0].HExperience 
                                     || !intSet.Contains(lstAnimInfo[index1][index2].id) 
                                     && !flagRange1 
                                     && !flagRange2)
                            {
                                lines.Append($"Continue State sateRestriction > HExperience...\n");
                                continue;
                            }
                            lines.Append($"Added UseAnimation said {UseAnimation(lstAnimInfo[index1][index2])}\n");
                            lstUseAnimInfo[index1].Add(lstAnimInfo[index1][index2]);
                        }
                    }
                }
                lines.Append('\n');
                var countGameA = Utilities.CountAnimations(lstUseAnimInfo);
                lines.Append($"Used Animations - {countGameA}\n");
                Log.Info($"0025: [CreateListAnimationFileName] Selected animations more detail:\n\n{lines}\n");
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
                lines.Append($"{Scene.ActiveScene.name} Heroine: {flags.lstHeroine[0].Name} with Experience {hExp}\n");

                var lstUseAnimInfo = hsceneTraverse
                    .Field<List<HSceneProc.AnimationListInfo>[]>("lstUseAnimInfo").Value;
                var dictTaii = JsonConvert.SerializeObject(dicExpAddTaii);
                var categorys = hsceneTraverse.Field<List<int>>("categorys").Value;
                var useCategorys = hsceneTraverse.Field<List<int>>("useCategorys").Value;
                var strUseCategories = Utilities.CategoryList(useCategorys, true, false);
                var strCategories = Utilities.CategoryList(categorys, true, false);
                var playHlist = JsonConvert.SerializeObject(Game.globalData.playHList);

                if (_isAnimListCreate)
                {
                    lines.Append("Asked to create animation list\n");
                }

                lines.Append($"category: {strCategories}\n");
                lines.Append($"useCategorys: {strUseCategories}\n");
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

            private static bool UseAnimation(
                HSceneProc.AnimationListInfo anim)
            {
                if (_hprocEarlyObjInstance == null)
                {
                    return false;
                }

                var hsceneTraverse = Traverse.Create(_hprocEarlyObjInstance);
                var flags = hsceneTraverse
                    .Field<HFlag>("flags").Value;
                var hExp = flags.lstHeroine[0].hExp;

                var categorys = hsceneTraverse
                    .Field<List<int>>("categorys").Value;
                var useCategorys = hsceneTraverse
                    .Field<List<int>>("useCategorys").Value;
                var lstAnimInfo = hsceneTraverse
                    .Field<List<HSceneProc.AnimationListInfo>[]>("lstAnimInfo").Value;
                var lstUseAnimInfo = new List<HSceneProc.AnimationListInfo>[8];
                var checkExpAddTaii = hsceneTraverse
                    .Method("CheckExpAddTaii",
                        new Type[] { typeof(int), typeof(int), typeof(float) });
                var checkShopAdd = hsceneTraverse
                    .Method("CheckShopAdd",
                        new Type[] { typeof(HashSet<int>), typeof(int), typeof(int) });
                var saveData = Game.saveData;
                var playHlist = Game.globalData.playHList;

                // get list of already used animations for category index1
                if (!playHlist.TryGetValue((int)anim.mode, out HashSet<int> intSet))
                {
                    intSet = new HashSet<int>();
                }

                if (!flags.isFreeH || intSet.Count != 0)
                {
                    if (!anim.lstCategory
                        .Any<HSceneProc.Category>(
                            (Func<HSceneProc.Category, bool>)
                            (c => useCategorys.Contains(c.category))))
                    {
                        return false;
                    }

                    if (!flags.isFreeH)
                    {
                        if (((!anim.isRelease ?
                            0 : (!checkExpAddTaii.GetValue<bool>(
                                    (int)anim.mode,
                                    anim.id,
                                    flags.lstHeroine[0].hExp) ? 1 : 0))
                                | (!checkShopAdd.GetValue<bool>(
                                    new HashSet<int>((IEnumerable<int>)saveData.player.buyNumTable.Keys),
                                    (int)anim.mode,
                                    anim.id) ? 1 : 0)) != 0
                                    || anim.isExperience != 2
                                    && (HSceneProc.EExperience)anim.isExperience > flags.experience)
                        {
                            return false;
                        }
                    }
                    else if ((SaveData.Heroine.HExperienceKind)anim.stateRestriction
                                > flags.lstHeroine[0].HExperience
                                || !_usedAnimations.Keys.Contains(AnimationInfo.GetKey(anim)))
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }

            //private Dictionary<int, Dictionary<int, int>> dicExpAddTaii = new Dictionary<int, Dictionary<int, int>>();

            //private bool CheckExpAddTaii(int mode, int id, float exp) => 
            //    !this.dicExpAddTaii.ContainsKey(mode) 
            //    || !this.dicExpAddTaii[mode].ContainsKey(id) 
            //    || (double)exp >= (double)  this.dicExpAddTaii[mode][id];

            private static bool UseAnimationOriginal(
                HSceneProc.AnimationListInfo anim)
            {
                if (_hprocEarlyObjInstance == null)
                {
                    return false;
                }

                var hsceneTraverse = Traverse.Create(_hprocEarlyObjInstance);
                var flags = hsceneTraverse
                    .Field<HFlag>("flags").Value;
                var hExp = flags.lstHeroine[0].hExp;

                var categorys = hsceneTraverse
                    .Field<List<int>>("categorys").Value;
                var useCategorys = hsceneTraverse
                    .Field<List<int>>("useCategorys").Value;
                var lstAnimInfo = hsceneTraverse
                    .Field<List<HSceneProc.AnimationListInfo>[]>("lstAnimInfo").Value;
                var lstUseAnimInfo = new List<HSceneProc.AnimationListInfo>[8];
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

                //HashSet<int> intSet;

                // get list of already used animations for category index1
                if (!playHlist.TryGetValue((int)anim.mode, out HashSet<int> intSet))
                {
                    intSet = new HashSet<int>();
                }

                if (!flags.isFreeH || intSet.Count != 0 || flagRange1 || flagRange2)
                {
                    if (flagRange1)
                    {
                        if (!anim.lstCategory
                            .Any<HSceneProc.Category>(
                                (Func<HSceneProc.Category, bool>)
                                (c => categorys.Contains(c.category))))
                        {
                            return false;
                        }
                    }
                    else if (!anim.lstCategory
                        .Any<HSceneProc.Category>(
                            (Func<HSceneProc.Category, bool>)
                            (c => useCategorys.Contains(c.category))))
                    {
                        return false;
                    }

                    if (!flags.isFreeH)
                    {
                        if (((!anim.isRelease ?
                            0 : (!checkExpAddTaii.GetValue<bool>(
                                    (int)anim.mode,
                                    anim.id,
                                    flags.lstHeroine[0].hExp) ? 1 : 0))
                                | (!checkShopAdd.GetValue<bool>(
                                    new HashSet<int>((IEnumerable<int>)saveData.player.buyNumTable.Keys),
                                    (int)anim.mode,
                                    anim.id) ? 1 : 0)) != 0
                                    || anim.isExperience != 2
                                    && (HSceneProc.EExperience)anim.isExperience > flags.experience)
                        {
                            return false;
                        }
                    }
                    else if ((SaveData.Heroine.HExperienceKind)anim.stateRestriction
                                > flags.lstHeroine[0].HExperience
                                || !intSet.Contains(anim.id)
                                && !flagRange1
                                && !flagRange2)
                    {
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }
    }
}
