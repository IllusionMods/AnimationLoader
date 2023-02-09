using System;

using UnityEngine;

using BepInEx.Logging;

using KKAPI;
using KKAPI.Chara;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        /// <summary>
        /// Move characters to a different positions
        /// </summary>
        public partial class MoveController : CharaCustomFunctionController
        {
            internal Vector3 _originalPosition = new(0, 0, 0);
            internal Vector3 _lastMovePosition = new(0, 0, 0);
            internal CharacterType _chaType = CharacterType.Unknown;

            public enum CharacterType { Heroine, Heroine3P, Player, Janitor, Group, Unknown }

            /// <summary>
            /// Required definition.
            /// </summary>
            /// <param name="currentGameMode"></param>
            protected override void OnCardBeingSaved(GameMode currentGameMode)
            {
            }

            internal void Init(CharacterType chaType)
            {
                _chaType = chaType;
            }

            /// <summary>
            /// Save original position
            /// </summary>
            internal void SetOriginalPosition(Vector3 position)
            {
                _originalPosition = position;
            }

            /// <summary>
            /// Restore original position
            /// </summary>
            public void ResetPosition()
            {
                ChaControl.transform.position = _originalPosition;
#if DEBUG
                Log.Warning($"0032: Resetting character position for {_chaType}" +
                    $"to position={ChaControl.transform.position.FormatVector()}.");
#endif
            }

            public void Move(Vector3 move)
            {
                try
                {
                    //if (_originalPosition == Vector3.zero)
                    //{
                    //    originalPosition = character.transform.position;
                    //}
                    var original = ChaControl.transform.position;
                    var rightZAxis = ChaControl.transform.right * move.x;
                    var upYAxis = ChaControl.transform.up * move.y;
                    var upXAxis = ChaControl.transform.forward * move.z;

                    // In manifest x is right z is forward
                    // in game transform x is forward z is right
                    Vector3 gameMove = new(move.z, move.y, move.x);
                    var newPosition = original + gameMove;
                    var gameMove2 = move.MovementTransform(ChaControl.transform);

                    ChaControl.transform.position += upXAxis;
                    ChaControl.transform.position += upYAxis;
                    ChaControl.transform.position += rightZAxis;
                    _lastMovePosition = ChaControl.transform.position;
#if DEBUG
                    Log.Level(LogLevel.Warning,
                        $"[Move] Adjusting character position for {_chaType}\n" +
                        $"       move={gameMove.FormatVector()}\n" +
                        $" trans move={gameMove2.FormatVector()}\n" +
                        $"         up={ChaControl.transform.up.FormatVector()}\n" +
                        $"      right={ChaControl.transform.right.FormatVector()}\n" +
                        $"    forward={ChaControl.transform.forward.FormatVector()}\n" +
                        $"      y(up)={upYAxis.FormatVector()}\n" +
                        $"   z(right)={rightZAxis.FormatVector()}\n" +
                        $" x(forward)={upXAxis.FormatVector()}\n" +
                        $"       From={original.FormatVector()}\n" +
                        $"         to={_lastMovePosition.FormatVector()}\n" +
                        $"    vector+={newPosition.FormatVector()}.");
#else
                    Log.Debug($"0031: Adjusting character position for {_chaType} to " +
                        $"position={_lastMovePosition.Format()}.");
#endif
                }
                catch (Exception e)
                {
                    Log.Level(LogLevel.Error, $"0010: Cannot adjust {_chaType} - " +
                        $"{ChaControl.name} - {e}.");
                }
            }
        }
    }
}
