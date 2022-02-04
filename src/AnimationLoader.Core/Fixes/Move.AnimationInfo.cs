//
// Class to help identify the animations
//
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
            internal bool _isAnimationLoader;

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

            internal string Controller {
                get { return _controller; }
            }

            public string Key {
                get { return $"{_Guid}-{_mode}-{_controller}-{_id:D3}"; }
            }

            public SwapAnimationInfo SwapAnim {
                get { return _anim; }
            }

            /// <summary>
            /// These are the fields for the key
            ///     _Guid - The guid of the zipmod
            ///     _mode - Animator mode (aibu, hoshi, ..)
            ///     _controller - Controller for the animation some zipmod don't define StudioId
            ///         this is added to make key unique
            ///     _id = Studio ID
            /// </summary>
            public AnimationInfo()
            {
                _Guid = string.Empty;
                _mode = EMode.none;
                _controller = string.Empty;
                _id = -1;
            }

            public AnimationInfo(HSceneProc.AnimationListInfo animation)
            {
                AnimationInfoHelper(animation);
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
                    _controller = anim.ControllerFemale;
                    _anim = anim;
                    _isAnimationLoader = true;
                }
                else
                {
                    // Game animation
                    _anim = null;
                    _Guid = KoikatuAPI.GameProcessName;
                    _controller = animation.paramFemale.path.file;
                    _id = animation.id;
                    _isAnimationLoader = false;
                }
            }

            /// <summary>
            /// Static function to get the key for any animation 
            /// no need for and instance of the class
            /// </summary>
            /// <param name="animation"></param>
            /// <param name="withguid"></param>
            /// <returns></returns>
            public static string GetKey(
                HSceneProc.AnimationListInfo animation,
                bool withguid = true)
            {
                string Guid;
                EMode mode;
                int id;
                string controller;

                mode = animation.mode;
                swapAnimationMapping.TryGetValue(animation, out var anim);
                if (anim != null)
                {
                    Guid = anim.Guid;
                    id = anim.StudioId;
                    controller = anim.ControllerFemale;
                }
                else
                {
                    Guid = "com.illusion";
                    id = animation.id;
                    controller = animation.paramFemale.path.file;
                }
                return withguid ? 
                    $"{Guid}-{mode}-{controller}-{id:D3}" : $"{mode}-{controller}-{id:D3}";
            }

            /// <summary>
            /// GetKey overload
            /// </summary>
            /// <param name="animation"></param>
            /// <returns></returns>
            public static string GetKey(SwapAnimationInfo animation)
            {
                return $"{animation.Guid}-{animation.Mode}-{animation.ControllerFemale}" +
                    $"-{animation.StudioId:D3}";
            }

            public void SetAnimation(object animation)
            {
                AnimationInfoHelper((HSceneProc.AnimationListInfo)animation);
            }

            // IsAnimationLoader no parameters refers to instance
            public bool IsAnimationLoader()
            {
                return _isAnimationLoader;
            }

            // IsAnimationLoader static check if it parameter is an AnimationLoader animation
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
