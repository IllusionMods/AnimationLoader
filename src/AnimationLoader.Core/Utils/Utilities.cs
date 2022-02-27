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
            static internal byte _alpha = 255;

#pragma warning disable IDE1006 // Naming Styles

#pragma warning disable IDE0025 // Use block body for properties
            static public Color red => new Color32(255, 0, 0, _alpha);
            static public Color darkred => new Color32(139, 0, 0, _alpha);

            static public Color magenta => new Color32(255, 0, 255, _alpha);

/* Unmerged change from project 'AnimationLoader.Koikatu'
Before:
            public static Color darkmagenta => new Color32(139, 0, 139, _alpha);
After:
            static public Color darkmagenta => new Color32(139, 0, 139, _alpha);
*/
            static public Color darkmagenta => new Color32(139, 0, 139, _alpha);


/* Unmerged change from project 'AnimationLoader.Koikatu'
Before:
            public static Color yellow => new Color32(255, 255, 0, _alpha);
            public static Color lightyellow => new Color32(255, 255, 224, _alpha);
After:
            static public Color yellow => new Color32(255, 255, 0, _alpha);
            static public Color lightyellow => new Color32(255, 255, 224, _alpha);
*/
            static public Color yellow => new Color32(255, 255, 0, _alpha);
            static public Color lightyellow => new Color32(255, 255, 224, _alpha);


/* Unmerged change from project 'AnimationLoader.Koikatu'
Before:
            public static Color blue => new Color32(0, 0, 255, _alpha);
            public static Color darkblue => new Color32(0, 0, 139, _alpha);
After:
            static public Color blue => new Color32(0, 0, 255, _alpha);
            static public Color darkblue => new Color32(0, 0, 139, _alpha);
*/
            static public Color blue => new Color32(0, 0, 255, _alpha);
            static public Color darkblue => new Color32(0, 0, 139, _alpha);

            static public Color cyan => new Color32(0, 255, 255, _alpha);
            static public Color darkcyan => new Color32(0, 139, 139, _alpha);

            static public Color orange => new Color32(255, 165, 0, _alpha);
            static public Color darkorange => new Color32(255, 140, 0, _alpha);
            

/* Unmerged change from project 'AnimationLoader.Koikatu'
Before:
            public static Color green => new Color32(0, 128, 0, _alpha);
            public static Color darkgreen => new Color32(0, 100, 0, _alpha);
            public static Color lime => new Color32(0, 255, 0, _alpha);
After:
            static public Color green => new Color32(0, 128, 0, _alpha);
            static public Color darkgreen => new Color32(0, 100, 0, _alpha);
            static public Color lime => new Color32(0, 255, 0, _alpha);
*/
            static public Color green => new Color32(0, 128, 0, _alpha);
            static public Color darkgreen => new Color32(0, 100, 0, _alpha);
            static public Color lime => new Color32(0, 255, 0, _alpha);


/* Unmerged change from project 'AnimationLoader.Koikatu'
Before:
            public static Color violet => new Color32(238, 130, 238, _alpha);
After:
            static public Color violet => new Color32(238, 130, 238, _alpha);
*/
            static public Color violet => new Color32(238, 130, 238, _alpha);
            static public Color darkviolet => new Color32(148, 0, 211, _alpha);


/* Unmerged change from project 'AnimationLoader.Koikatu'
Before:
            public static Color orangered => new Color32(255, 69, 0, _alpha);
            public static Color blueviolet => new Color32(138, 43, 226, _alpha);
            public static Color greenyellow => new Color32(173, 255, 47, _alpha);
            public static Color yellowgreen => new Color32(154, 205, 50, _alpha);
After:
            static public Color orangered => new Color32(255, 69, 0, _alpha);
            static public Color blueviolet => new Color32(138, 43, 226, _alpha);
            static public Color greenyellow => new Color32(173, 255, 47, _alpha);
            static public Color yellowgreen => new Color32(154, 205, 50, _alpha);
*/
            static public Color orangered => new Color32(255, 69, 0, _alpha);
            static public Color blueviolet => new Color32(138, 43, 226, _alpha);
            static public Color greenyellow => new Color32(173, 255, 47, _alpha);
            static public Color yellowgreen => new Color32(154, 205, 50, _alpha);


/* Unmerged change from project 'AnimationLoader.Koikatu'
Before:
            public static Color black => new Color32(0, 0, 0, _alpha);
            public static Color gray => new Color32(128, 128, 128, _alpha);
            public static Color white => new Color32(255, 255, 255, _alpha);
After:
            static public Color black => new Color32(0, 0, 0, _alpha);
            static public Color gray => new Color32(128, 128, 128, _alpha);
            static public Color white => new Color32(255, 255, 255, _alpha);
*/
            static public Color black => new Color32(0, 0, 0, _alpha);
            static public Color gray => new Color32(128, 128, 128, _alpha);
            static public Color white => new Color32(255, 255, 255, _alpha);

#pragma warning restore IDE0025 // Use block body for properties
#pragma warning restore IDE1006 // Naming Styles

            /// <summary>
            /// Save information for template.xml
            /// </summary>

/* Unmerged change from project 'AnimationLoader.Koikatu'
Before:
            internal static void SaveAnimInfo(
After:
            static internal void SaveAnimInfo(
*/
            static internal void SaveAnimInfo(
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

            static internal string Translate(string name)
            {
                if (!TranslationHelper.TryTranslate(name, out var tmp))
                {
                    return name;
                }

                return tmp;
            }

            static internal string TranslateName(string animationName, bool original = false)
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
            static internal string CategoryList(List<HSceneProc.Category> categories, bool names = false, bool quotes = true)
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
            static internal string CategoryList(List<int> categories, bool names = false, bool quotes = true)
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

            static internal int CountAnimations(List<HSceneProc.AnimationListInfo>[] lstAnimInfo)
            {
                var count = 0;

                foreach (var c in lstAnimInfo)
                {
                    count += c.Count;
                }
                return count;
            }

            static internal bool HasMovement(AnimationInfo anim)
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
            static internal void SetOriginalPositionAll()
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
            static internal bool IsNewPosition(ChaControl chaControl)
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
            static internal int GetExpTaii(int mode, int id)
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
