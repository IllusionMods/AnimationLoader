using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

using HarmonyLib;
using Illusion.Extensions;

using KKAPI.Utilities;

using static HFlag;
using static Illusion.Component.UI.MouseButtonCheck;

namespace AnimationLoader
{
    public partial class SwapAnim
    {
        internal class MoveCharacter
        {
            public static void Move(ChaControl character, Vector3 move)
            {
                try
                {
                    var xAxis = character.transform.right * move.x;
                    var yAxis = new Vector3(0, move.y, 0);
                    var zAxis = character.transform.forward * move.z;

                    character.transform.position += xAxis;
                    character.transform.position += yAxis;
                    character.transform.position += zAxis;
                }
                catch (Exception e)
                {
                    Logger.LogError($"0015: Cannot adjust {character.name}.");
                }
            }
        }
    }
}
