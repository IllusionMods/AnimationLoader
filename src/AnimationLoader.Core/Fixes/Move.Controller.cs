using System;

using UnityEngine;

using KKAPI;
using KKAPI.Chara;
using Studio;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        public partial class MoveController : CharaCustomFunctionController
        {
            internal Vector3 _originalPosition = new(0, 0, 0);
            internal Vector3 _lastMovePosition = new(0, 0, 0);

            /// <summary>
            /// Required definition.
            /// </summary>
            /// <param name="currentGameMode"></param>
            protected override void OnCardBeingSaved(GameMode currentGameMode)
            {
            }

            internal void Init()
            {
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
                Log.Warning($"0032: Resetting character positions.");
#endif
            }

            public void Move(Vector3 move)
            {
                try
                {
#if DEBUG
                    Log.Warning($"0031: Adjusting character positions.");
#else
                    Log.Debug($"0031: Adjusting character positions.");
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
                    Log.Error($"0015: Cannot adjust {ChaControl.name} - {e}.");
                }
            }
        }
    }
}
