using System;

using UnityEngine;

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
            override protected void OnCardBeingSaved(GameMode currentGameMode)
            {
            }

            internal void Init(CharacterType chaType)
            {
                _chaType = chaType;
                SetOriginalPosition();
            }

            /// <summary>
            /// Save original position
            /// </summary>
            internal void SetOriginalPosition() =>
                _originalPosition = ChaControl.transform.position;

            /// <summary>
            /// Restore original position
            /// </summary>
            public void ResetPosition()
            {
                ChaControl.transform.position = _originalPosition;
#if DEBUG
                Log.Warning($"0032: Resetting character position for {_chaType}.");
#endif
            }

            public void Move(Vector3 move)
            {
                try
                {
#if DEBUG
                    Log.Warning($"0031: Adjusting character position for {_chaType}.");
#else
                    Log.Debug($"0031: Adjusting character position for {_chaType}.");
#endif
                    //if (_originalPosition == Vector3.zero)
                    //{
                    //    originalPosition = character.transform.position;
                    //}
                    var xAxis = ChaControl.transform.right * move.x;
                    var yAxis = new Vector3(0, move.y, 0);
                    var zAxis = ChaControl.transform.forward * move.z;

                    ChaControl.transform.position += xAxis;
                    ChaControl.transform.position += yAxis;
                    ChaControl.transform.position += zAxis;
                    _lastMovePosition = ChaControl.transform.position;
                }
                catch (Exception e)
                {
                    Log.Error($"0010: Cannot adjust {_chaType} - {ChaControl.name} - {e}.");
                }
            }
        }
    }
}
