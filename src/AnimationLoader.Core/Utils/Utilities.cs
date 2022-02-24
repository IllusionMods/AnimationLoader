using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

using Illusion.Extensions;

using HarmonyLib;

using KKAPI.Utilities;
using System.Xml.Serialization;

namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal class Utilities
        {
            /// <summary>
            /// Save information for template.xml
            /// </summary>
            internal static void SaveAnimInfo(
                object hsceneProc, 
                List<HSceneProc.AnimationListInfo>[] lstAnimInfo)
            {
                var total = 0;

                // id, mode,
                // nameAnimation (Japanese name), path, posture,
                // numCtrl, kindHoshi,
                // hoshiLoopActionS, isFemaleInitiative,
                // {category list}, fileSiruPaste
                // dicExpTaii[mode][id]
                for (var i = 0; i < lstAnimInfo.Length; i++)
                {
                    FileInfo file = new($"lst{i}.csv");

                    if (file.Exists)
                    {
                        continue;
                    }

                    var lines = lstAnimInfo[i].Select(x => $"{x.id}, {x.mode}," +
                         $" {TranslateName(x.nameAnimation, true)}, {x.paramFemale.path.file}, {x.posture}," +
                         $" {x.numCtrl}, {x.kindHoushi}," +
                         $" {x.houshiLoopActionS}, {x.isFemaleInitiative}," +
                         $"{CategoryList(x.lstCategory, true)}," +
#if KKS
                         $" {x.paramFemale.fileSiruPaste}," +
                         $" {GetExpTaii((int)x.mode, x.id)}");
#else
                         $" {x.paramFemale.fileSiruPaste}");
#endif
                    File.WriteAllLines($"lst{i}.csv", lines.ToArray());
                    total += lines.ToArray().Length;
                }
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

            /// <summary>
            /// Return categories in the string form "{ cat 1, cat 2, ,,,}"
            /// </summary>
            /// <param name="categories"></param>
            /// <param name="names"></param>
            /// <param name="quotes"></param>
            /// <returns></returns>
            internal static string CategoryList(List<HSceneProc.Category> categories, bool names = false, bool quotes = true)
            {
                var tmp = "";
                var first = true;

                foreach (var c in categories)
                {
                    if (first)
                    {
                        if (names)
                        {
                            tmp += (PositionCategory)c.category;
                        }
                        else
                        {
                            tmp += c.category.ToString();
                        }
                        first = false;
                    }
                    else
                    {
                        if (names)
                        {
                            tmp += ", " + (PositionCategory)c.category;
                        }
                        else
                        {
                            tmp += ", " + c.category.ToString();
                        }
                    }
                }
                return quotes ? "\" { " + tmp + " }\"" : "{ " + tmp + " }";
            }

            /// <summary>
            /// Ditto
            /// </summary>
            /// <param name="categories"></param>
            /// <param name="names"></param>
            /// <param name="quotes"></param>
            /// <returns></returns>
            internal static string CategoryList(List<int> categories, bool names = false, bool quotes = true)
            {
                var tmp = "";
                var first = true;

                foreach (var c in categories)
                {
                    if (first)
                    {
                        if (names)
                        {
                            tmp += (PositionCategory)c;
                        }
                        else
                        {
                            tmp += c.ToString();
                        }
                        first = false;
                    }
                    else
                    {
                        if (names)
                        {
                            tmp += ", " + (PositionCategory)c;
                        }
                        else
                        {
                            tmp += ", " + c.ToString();
                        }
                    }
                }
                return quotes ? "\" { " + tmp + " }\"" : "{ " + tmp + " }";
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

            internal static bool HasMovement(AnimationInfo anim)
            {
                if (anim?.SwapAnim != null)
                {
                    if (anim.SwapAnim.PositionHeroine != Vector3.zero)
                    {
                        return true;
                    }
                    if (anim.SwapAnim.PositionPlayer != Vector3.zero)
                    {
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Set new original position for characters if there is a move 
            /// from original position saved
            /// </summary>
            /// <param name="message"></param>
            internal static void SetOriginalPositionAll()
            {
                if (_flags == null)
                {
                    return;
                }

                var heroines = _flags.lstHeroine;
                for (var i = 0; i < heroines.Count; i++)
                {
                    if (IsNewPosition(heroines[i].chaCtrl))
                    {
                        GetMoveController(heroines[i].chaCtrl).SetOriginalPosition();
                    }
                }
                if (IsNewPosition(_flags.player.chaCtrl))
                {
                    GetMoveController(_flags.player.chaCtrl).SetOriginalPosition();
                }
            }

            /// <summary>
            /// Determine if there is a change in original position
            /// </summary>
            /// <param name="chaControl"></param>
            /// <returns></returns>
            internal static bool IsNewPosition(ChaControl chaControl)
            {
                var controller = GetMoveController(chaControl);
                var newPosition = chaControl.transform.position;
                var originalPosition = controller._originalPosition;
                var lastMovePosition = controller._lastMovePosition;
                if (newPosition != originalPosition && newPosition != lastMovePosition)
                {
                    return true;
                }
                return false;
            }
#if KKS
            /// <summary>
            /// Returns the experience level needed for the animation to be active using cached
            /// dictionary.  The system dictionary may be altered.
            /// </summary>
            /// <param name="mode"></param>
            /// <param name="id"></param>
            /// <returns></returns>
            internal static int GetExpTaii(int mode, int id)
            {
                if (_dicExpAddTaii != null)
                {
                    if (_dicExpAddTaii.ContainsKey(mode) && _dicExpAddTaii[mode].ContainsKey(id))
                    {
                        return _dicExpAddTaii[mode][id];
                    }
                    return 0;
                }
                return -1;
            }
#endif
        }
    }
}
