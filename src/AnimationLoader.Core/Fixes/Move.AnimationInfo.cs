using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using HarmonyLib;
using Illusion.Extensions;

using KKAPI.Utilities;

using static HFlag;
using static Illusion.Component.UI.MouseButtonCheck;

namespace AnimationLoader
{
    public partial class SwapAnim
    {
        public class AnimationInfo
        {
            internal string _Guid;
            internal EMode _mode;
            internal int _id;
            internal SwapAnimationInfo _anim;
            public string Guid {
                get { return _Guid; }
            }
            public EMode Mode {
                get { return _mode; }
            }
            public int Id {
                get { return _id; }
            }

            public string Key {
                get { return $"{_Guid}-{_mode}-{_id:D3}"; }
            }

            public AnimationInfo()
            {
                _Guid = string.Empty;
                _mode = EMode.none;
                _id = -1;
            }

            public SwapAnimationInfo SwapAnim {
                get { return _anim; }
            }

            internal void AnimationInfoHelper(HSceneProc.AnimationListInfo animation)
            {
                _mode = animation.mode;
                swapAnimationMapping.TryGetValue(animation, out var anim);
                if (anim != null)
                {
                    // AnimationLoader animation
                    _Guid = anim.Guid;
                    _id = anim.StudioId;
                    _anim = anim;
                }
                else
                {
                    // Game animation
                    _anim = null;
                    _Guid = PInfo.GUID + ".move";
                    _id = animation.id;
                }
            }
            public AnimationInfo(HSceneProc.AnimationListInfo animation)
            {
                AnimationInfoHelper(animation);
            }

            public static string GetKey(HSceneProc.AnimationListInfo animation)
            {
                string Guid;
                EMode mode;
                int id;

                mode = animation.mode;
                swapAnimationMapping.TryGetValue(animation, out var anim);
                if (anim != null)
                {
                    Guid = anim.Guid;
                    id = anim.StudioId;
                }
                else
                {
                    Guid = PInfo.GUID + ".move";
                    id = animation.id;
                }
                return $"{Guid}-{mode}-{id:D3}";
            }

            public static string GetKey(SwapAnimationInfo animation)
            {
                return $"{animation.Guid}-{animation.Mode}-{animation.StudioId:D3}";
            }

            public void SetAnimation(object animation)
            {
                AnimationInfoHelper((HSceneProc.AnimationListInfo)animation);
            }
        }
    }
}
