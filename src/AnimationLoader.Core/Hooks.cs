using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

using Illusion.Extensions;
using Manager;
using Studio;
using UnityEngine;

using Sideloader.AutoResolver;
using HarmonyLib;
using BepInEx.Logging;
using System.Text;

namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal static Harmony _hookInstance;

        internal partial class Hooks
        {
            /// <summary>
            /// Initialize the Hooks patch instance
            /// </summary>
            internal static void Init()
            {
                _hookInstance = Harmony.CreateAndPatchAll(typeof(Hooks), nameof(Hooks));

                if (VRHSceneType != null)
                {
                    _hookInstance.Patch(
                        AccessTools.Method(
                            VRHSceneType,
                            nameof(HSceneProc.ChangeAnimator)),
                            postfix: new HarmonyMethod(typeof(Hooks),
                            nameof(SwapAnimation)));
                    _hookInstance.Patch(
                        AccessTools.Method(
                            VRHSceneType,
                            nameof(HSceneProc.CreateAllAnimationList)),
                            postfix: new HarmonyMethod(typeof(Hooks),
                            nameof(ExtendList)));
                }
            }

            /// <summary>
            /// Initialize MoveController for characters
            /// </summary>
            /// <param name="__instance"></param>
            /// <param name="___lstFemale"></param>
            /// <param name="___male"></param>
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HSceneProc), nameof(HSceneProc.SetShortcutKey))]
            private static void SetShortcutKeyPrefix(
                object __instance,
                List<ChaControl> ___lstFemale,
                ChaControl ___male)
            {
                _hprocObjInstance = __instance;
                _lstHeroines = ___lstFemale;
                _heroine = _lstHeroines[0];
                GetMoveController(_heroine).Init();

                if (___lstFemale.Count > 1)
                {
                    _heroine3P = _lstHeroines[1];
                    GetMoveController(_heroine3P).Init();
                }

                _player = ___male;
                GetMoveController(_player).Init();

                _flags = Traverse
                    .Create(__instance)
                    .Field<HFlag>("flags").Value;
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
        }
    }
}
