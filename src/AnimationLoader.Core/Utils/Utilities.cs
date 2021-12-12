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
            static internal void SaveAnimInfo()
            {
                if (_hprocInstance == null)
                {
#if DEBUG
                    Logger.LogWarning($"[ShowAnimInfo] Instance? {_hprocInstance != null}");
#endif
                    return;
                }

                int total = 0;
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
                         $" {Translate(x.nameAnimation)} ({x.nameAnimation}), {x.posture}," +
                         $" {x.numCtrl}, {x.kindHoushi}," +
                         $" {x.houshiLoopActionS}, {x.isFemaleInitiative}," +
                         $" {CategoryList(x.lstCategory)}," +
                         $" {x.paramFemale.fileSiruPaste}");

                    File.WriteAllLines($"lst{i}.csv", (string[])lines);
                    total += lines.ToArray().Length;
                }
#if DEBUG
                Logger.LogWarning($"Total animations {total}");
#endif
            }

            static internal string Translate(string name)
            {
                if (!TranslationHelper.TryTranslate(name, out string tmp))
                {
                    return name;
                }

                return tmp;
            }

            // TODO: Tried a few ways to make it work with/without casting
            // did not work check why later
            static internal string CategoryList(List<HSceneProc.Category> categories)
            {
                string tmp = "";
                bool first = true;

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

            static internal int CountAnimations(List<HSceneProc.AnimationListInfo>[] lstAnimInfo)
            {
                int count = 0;

                foreach (var c in lstAnimInfo)
                {
                    count += c.Count;
                }
                return count;
            }

            static internal void DisplayData(object data)
            {
                var ddata = (SwapAnimationInfo)data;
                Logger.LogWarning($"\n{ddata.AnimationName}");
            }
        }
    }
}
