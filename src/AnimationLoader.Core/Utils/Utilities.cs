using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using HarmonyLib;
using Illusion.Extensions;

using KKAPI.Utilities;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal class Utilities
        {
            internal static void SaveAnimInfo()
            {
                if (_hprocInstance == null)
                {
#if DEBUG
                    Logger.LogWarning($"0016: [ShowAnimInfo] Instance? {_hprocInstance is not null}");
#endif
                    return;
                }

                var total = 0;
                var lstAnimInfo = Traverse
                    .Create(_hprocInstance)
                    .Field<List<HSceneProc.AnimationListInfo>[]>("lstAnimInfo").Value;

                // id, mode,
                // nameAnimation (Japanese name), posture,
                // numCtrl, kindHoshi,
                // hoshiLoopActionS, isFemaleInitiative,
                // {category list},
                // fileSiruPaste
                for (var i = 0; i < lstAnimInfo.Length; i++)
                {
                    var lines = lstAnimInfo[i].Select(x => $"{x.id}, {x.mode}," +
                         $" {TranslateName(x.nameAnimation)}, {x.posture}," +
                         $" {x.numCtrl}, {x.kindHoushi}," +
                         $" {x.houshiLoopActionS}, {x.isFemaleInitiative}," +
                         $"{CategoryList(x.lstCategory)}," +
                         $" {x.paramFemale.fileSiruPaste}");

                    File.WriteAllLines($"lst{i}.csv", lines.ToArray());
                    total += lines.ToArray().Length;
                }
#if DEBUG
                Logger.LogWarning($"0017: Total animations {total}");
#endif
            }

            internal static string Translate(string name)
            {
                if (!TranslationHelper.TryTranslate(name, out var tmp))
                {
                    return name;
                }

                return tmp;
            }

            internal static string TranslateName(string animationName, bool original = false)
            {
                var tmp = Translate(animationName);
                if ((tmp == animationName) || !original)
                {
                    return tmp;
                }
                return $"{tmp} ({animationName})";
            }

            // TODO: Tried a few ways to make it work with/without casting
            // did not work check why later
            internal static string CategoryList(List<HSceneProc.Category> categories)
            {
                var tmp = "";
                var first = true;

                foreach (var c in categories)
                {
                    if (first)
                    {
                        tmp += c.category.ToString();
                        first = false;
                    }
                    else
                    {
                        tmp += ", " + c.category.ToString();
                    }
                }
                return "\" { " + tmp + " }\"";
            }

            internal static int CountAnimations(List<HSceneProc.AnimationListInfo>[] lstAnimInfo)
            {
                var count = 0;

                foreach (var c in lstAnimInfo)
                {
                    count += c.Count;
                }
                return count;
            }

            internal static void SaveHProcInstance(object instance)
            {
                if (_hprocInstance == null)
                {
                    _hprocInstance = (HSceneProc)instance;
                    if (_hprocInstance == null)
                    {
                        Logger.LogDebug($"0009: Failed to save _hprocInstance");
                    }
                    else
                    {
                        Logger.LogDebug($"0010: _hprocInstance saved.");
                    }
                }
            }
        }
    }
}
