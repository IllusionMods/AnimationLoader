
using KKAPI;

using static HFlag;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        /// <summary>
        /// Class to help uniquely identified the animations
        /// </summary>
        public class AnimationInfo
        {
            internal SwapAnimationInfo _anim;

            internal string _Guid;
            internal EMode _mode;
            internal int _id;
            internal string _controller;

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
                get { return $"{_Guid}-{_mode}-{_controller}-{_id:D3}"; }
            }

            public AnimationInfo()
            {
                _Guid = string.Empty;
                _mode = EMode.none;
                _controller = string.Empty;
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
                    // Some manifest don't define a StudioId using this to make sure
                    // key is one-to-one (so far so god)
                    _controller = anim.ControllerFemale;
                    _anim = anim;
                }
                else
                {
                    // Game animation
                    _anim = null;
                    _Guid = KoikatuAPI.GameProcessName;
                    _controller = animation.paramFemale.path.file;
                    _id = animation.id;
                }
            }
            public AnimationInfo(HSceneProc.AnimationListInfo animation)
            {
                AnimationInfoHelper(animation);
            }

            public static string GetKey(HSceneProc.AnimationListInfo animation, bool withguid = true)
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
                    Guid = "com.illusion";
                    id = animation.id;
                }
                return withguid ? $"{Guid}-{mode}-{id:D3}" : $"{mode}-{id:D3}";
            }

            public static string GetKey(SwapAnimationInfo animation)
            {
                return $"{animation.Guid}-{animation.Mode}-{animation.ControllerFemale}-{animation.StudioId:D3}";
            }

            public void SetAnimation(object animation)
            {
                AnimationInfoHelper((HSceneProc.AnimationListInfo)animation);
            }

            public static bool IsAnimationLoader(SwapAnimationInfo animation)
            {
                return true;
            }

            public static bool IsAnimationLoader(HSceneProc.AnimationListInfo animation)
            {
                swapAnimationMapping.TryGetValue(animation, out var anim);
                if (anim != null)
                {
                    return true;
                }
                return false;
            }

            public static string TranslateName(HSceneProc.AnimationListInfo animation)
            {
                swapAnimationMapping.TryGetValue(animation, out var anim);
                if (anim != null)
                {
                    return Utilities.TranslateName(anim.AnimationName);
                }
                return Utilities.TranslateName(animation.nameAnimation);
            }
        }
    }
}
