using System;
using System.Collections.Generic;
using System.Reflection.Emit;

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
#if DEBUG && KKS
                _hookInstance.Patch(
                    AccessTools.Method(
                        Type.GetType("HSceneProc, Assembly-CSharp"),
                            nameof(HSceneProc.CreateListAnimationFileName)),
                        postfix: new HarmonyMethod(typeof(Hooks),
                            nameof(CreateListAnimationFileNamePostfix)));

                _hookInstance.Patch(
                    AccessTools.Method(
                        Type.GetType("HSceneProc, Assembly-CSharp"),
                            nameof(HSceneProc.ChangeCategory)),
                        postfix: new HarmonyMethod(typeof(Hooks),
                            nameof(ChangeCategoryPostfix)));


                // This is for DEBUG tests only
                _hookInstance.Patch(
                    AccessTools.Method(
                        Type.GetType("HSceneProc, Assembly-CSharp"),
                            nameof(HSceneProc.LoadAddTaii),
                            new Type[] { typeof(List<AddTaiiData.Param>) }),
                        postfix: new HarmonyMethod(typeof(Hooks),
                            nameof(LoadAddTaiiPostfix)));

                /*
                 * Why this does not work...when method runs does not find stuff
                 * 
                 * var HSceneProcType = Type.GetType("HSceneProc, Assembly-CSharp");
                 * _hsHookInstance.Patch(
                 *     HSceneProcType.GetMethod("SetShortcutKey", AccessTools.all),
                 *     postfix: new HarmonyMethod(typeof(HSHooks), nameof(HSHooks.SetShortcutKeyPostfix)));
                 * _hookInstance.Patch(
                 *     HSceneProcType.GetMethod("CreateListAnimationFileName", AccessTools.all),
                 *     prefix: new HarmonyMethod(typeof(Hooks), nameof(Hooks.CreateListAnimationFileNamePostfix)));
                 */
#endif
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
