using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

using Illusion.Extensions;

using KKAPI;
using KKAPI.Utilities;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal class Utilities
        {
            private static readonly byte _alpha = 255;
            private static readonly bool _isSunshine = (KoikatuAPI.GameProcessName == "KoikatsuSunshine")
                || (KoikatuAPI.VRProcessName == "KoikatsuSunshine_VR");

            private static readonly bool _isSunshineEx = _isSunshine &&
                (KoikatuAPI.GetGameVersion().CompareTo(new Version("1.0.8")) > 0);

            #region Color properties
#pragma warning disable IDE1006 // Naming Styles

            public static Color red => new Color32(255, 0, 0, _alpha);
            public static Color darkred => new Color32(139, 0, 0, _alpha);

            public static Color magenta => new Color32(255, 0, 255, _alpha);
            public static Color darkmagenta => new Color32(139, 0, 139, _alpha);
            public static Color pink => new Color32(255, 192, 203, _alpha);

            public static Color yellow => new Color32(255, 255, 0, _alpha);
            public static Color lightyellow => new Color32(255, 255, 224, _alpha);

            public static Color blue => new Color32(0, 0, 255, _alpha);
            public static Color darkblue => new Color32(0, 0, 139, _alpha);

            public static Color cyan => new Color32(0, 255, 255, _alpha);
            public static Color darkcyan => new Color32(0, 139, 139, _alpha);

            public static Color orange => new Color32(255, 165, 0, _alpha);
            public static Color darkorange => new Color32(255, 140, 0, _alpha);
            public static Color gold => new Color32(255, 215, 0, _alpha);

            public static Color green => new Color32(0, 128, 0, _alpha);
            public static Color darkgreen => new Color32(0, 100, 0, _alpha);
            public static Color lime => new Color32(0, 255, 0, _alpha);

            public static Color violet => new Color32(238, 130, 238, _alpha);
            public static Color darkviolet => new Color32(148, 0, 211, _alpha);

            public static Color orangered => new Color32(255, 69, 0, _alpha);
            public static Color blueviolet => new Color32(138, 43, 226, _alpha);
            public static Color greenyellow => new Color32(173, 255, 47, _alpha);
            public static Color yellowgreen => new Color32(154, 205, 50, _alpha);

            public static Color black => new Color32(0, 0, 0, _alpha);
            public static Color gray => new Color32(128, 128, 128, _alpha);
            public static Color white => new Color32(255, 255, 255, _alpha);

#pragma warning restore IDE1006 // Naming Styles
            #endregion

            public static bool IsSunshine => _isSunshine;
            public static bool IsSunshineEx => _isSunshineEx;

            /// <summary>
            /// Save information for template.xml
            /// </summary>
            internal static void SaveAnimInfo(List<HSceneProc.AnimationListInfo>[] lstAnimInfo)
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
                         $" {TranslateName(x.nameAnimation, true)}, {x.paramFemale.path.file}," +
                         $" {x.paramFemale.fileMotionNeck}, {x.posture}," +
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
            internal static void SetOriginalPositionAll(Vector3 position)
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
                        GetMoveController(heroines[i].chaCtrl).SetOriginalPosition(position);
                    }
                }
                if (IsNewPosition(_flags.player.chaCtrl))
                {
                    GetMoveController(_flags.player.chaCtrl).SetOriginalPosition(position);
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

            /// <summary>
            /// Save ExpTaii in dictionary
            /// </summary>
            internal static void AlDicExpAddTaii()
            {
                _alDicExpAddTaii.Clear();

                foreach (var mode in animationDict.Keys)
                {
                    foreach(var anim in animationDict[mode])
                    {
                        if(!_alDicExpAddTaii.ContainsKey(anim.Guid))
                        {
                            _alDicExpAddTaii.Add(anim.Guid,
                                []);
                            _alDicExpAddTaii[anim.Guid].Clear();
                        }
                        if(!_alDicExpAddTaii[anim.Guid].ContainsKey((int)anim.Mode))
                        {
                            _alDicExpAddTaii[anim.Guid].Add((int)anim.Mode,
                                []);
                            _alDicExpAddTaii[anim.Guid][(int)anim.Mode].Clear();
                        }
                        _alDicExpAddTaii[anim.Guid][(int)anim.Mode]
                            .Add($"{anim.ControllerFemale}{anim.StudioId}", anim.ExpTaii);
                    }
                }
            }
#endif
        }
    }
}
