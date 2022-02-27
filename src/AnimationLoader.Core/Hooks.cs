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
        static private ChaControl _heroine;
        static private ChaControl _heroine3P;
        static private List<ChaControl> _lstHeroines;
        static private ChaControl _player;
        static private HFlag _flags;
        static private Harmony _hookInstance;

        internal partial class Hooks
        {
            /// <summary>
            /// Initialize the Hooks patch instance
            /// </summary>
            static internal void Init()
            {
                //_hookInstance = Harmony.CreateAndPatchAll(typeof(Hooks), nameof(Hooks));
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
            static private void SetShortcutKeyPrefix(
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

                _flags = Traverse
                    .Create(__instance)
                    .Field<HFlag>("flags").Value;
            }

            [HarmonyTranspiler]
            [HarmonyPatch(typeof(HSprite), nameof(HSprite.OnChangePlaySelect))]
            static private IEnumerable<CodeInstruction> OnChangePlaySelect(
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
