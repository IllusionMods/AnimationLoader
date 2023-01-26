//
// AnimationLoader Hooks
//
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using H;

using HarmonyLib;

using static AnimationLoader.SwapAnim.MoveController;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        private static ChaControl _heroine;
        private static ChaControl _heroine3P;
        private static List<ChaControl> _lstHeroines;
        private static ChaControl _player;
        private static HFlag _flags;
        private static Harmony _hookInstance;
        private static bool _specialAnimation = false;

        internal partial class Hooks
        {
            /// <summary>
            /// Initialize the Hooks patch instance
            /// </summary>
            internal static void Init()
            {
                _hookInstance = Harmony.CreateAndPatchAll(typeof(Hooks));

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
                _lstHeroines = ___lstFemale;
                _heroine = _lstHeroines[0];
                GetMoveController(_heroine).Init(CharacterType.Heroine);

                if (___lstFemale.Count > 1)
                {
                    _heroine3P = _lstHeroines[1];
                    GetMoveController(_heroine3P).Init(CharacterType.Heroine3P);
                }

                _player = ___male;
                GetMoveController(_player).Init(CharacterType.Player);

                var hsceneTraverse = Traverse.Create(__instance);
                var categorys = hsceneTraverse.Field<List<int>>("categorys").Value;
                _specialAnimation = (categorys[0] is 12 or > 1000);

                _flags = hsceneTraverse
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
