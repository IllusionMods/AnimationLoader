//
// Class to help identify the animations
//
using System.Collections.Generic;

using UnityEngine;

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
            internal HSceneProc.AnimationListInfo _animGA;
            internal bool _isAnimationLoader;

            internal string _Guid;
            internal EMode _mode;
            internal int _id;
            internal string _controller;
            internal string _name;
            internal int _donor;

            internal string Controller => _controller;
            public int Donor => _donor;
            public string Guid => _Guid;
            public int Id => _id;
            public string Key => $"{_Guid}-{_mode}-{_controller}-{_id:D3}";
            public EMode Mode => _mode;
            public string Name => _name;
            public bool IsAnimationLoader => _isAnimationLoader;
            public SwapAnimationInfo SwapAnim => _anim;
            public HSceneProc.AnimationListInfo Animation => _animGA;

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
                _name = string.Empty;
                _donor = -1;
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
                    _anim = anim;
                    _animGA = null;
                    _controller = anim.ControllerFemale;
                    _Guid = anim.Guid;
                    _id = anim.StudioId;
                    _isAnimationLoader = true;
                    _name = anim.AnimationName;
                    _donor = anim.DonorPoseId;
                }
                else
                {
                    // Game animation
                    _anim = null;
                    _animGA = animation;
                    _controller = animation.paramFemale.path.file;
                    _Guid = KoikatuAPI.GameProcessName;
                    _id = animation.id;
                    _isAnimationLoader = false;
                    _name = animation.nameAnimation;
                    _donor = -1;
                }
            }

            public void SetAnimation(object animation)
            {
                AnimationInfoHelper((HSceneProc.AnimationListInfo)animation);
            }

            public string TranslateName()
            {
                return Utilities.TranslateName(_name);
            }

            public object Anim()
            {
                if (_anim == null)
                {
                    return _animGA;
                }
                return _anim;
            }

        }

        /// <summary>
        /// Static function to get the key for any animation 
        /// </summary>
        /// <param name="animation"></param>
        /// <param name="withguid"></param>
        /// <returns></returns>
        public static string GetAnimationKey(HSceneProc.AnimationListInfo animation)
        {
            string Guid;
            EMode mode;
            int id;
            string controller;

            Guid = "com.illusion";
            mode = animation.mode;
            id = animation.id;
            controller = animation.paramFemale.path.file;

            if (swapAnimationMapping != null)
            {
                swapAnimationMapping.TryGetValue(animation, out var anim);
                if (anim != null)
                {
                    Guid = anim.Guid;
                    id = anim.StudioId;
                    controller = anim.ControllerFemale;
                }
            }

            return $"{Guid}-{mode}-{controller}-{id:D3}";
        }

        /// <summary>
        /// GetKey overload
        /// </summary>
        /// <param name="animation"></param>
        /// <returns></returns>
        public static string GetAnimationKey(SwapAnimationInfo animation)
        {
            return $"{animation.Guid}-{animation.Mode}-{animation.ControllerFemale}" +
                $"-{animation.StudioId:D3}";
        }

        public static List<string> Controllers(HSceneProc.AnimationListInfo animation)
        {
            var result = new List<string>();

            result.Clear();


            if (swapAnimationMapping != null)
            {
                swapAnimationMapping.TryGetValue(animation, out var anim);
                if (anim != null)
                {
                    return Controllers(anim);
                }
            }

            if (animation.paramFemale.path.file != null)
            {
                result.Add(animation.paramFemale.path.file);
            }
            if (animation.paramMale.path.file != null)
            {
                result.Add(animation.paramMale.path.file);
            }
            if (animation.paramFemale1.path.file != null)
            {
                result.Add(animation.paramFemale1.path.file);
            }

            return result;
        }


        public static List<string> Controllers(SwapAnimationInfo animation)
        {
            var result = new List<string>();

            result.Clear();

            if (animation.ControllerFemale != null)
            {
                result.Add(animation.ControllerFemale);
            }
            if (animation.ControllerMale != null)
            {
                result.Add(animation.ControllerMale);
            }
            if (animation.ControllerFemale1 != null)
            {
                result.Add(animation.ControllerFemale1);
            }

            return result;
        }

        /// <summary>
        /// Get a list with the positions adjustment in the manifest. If it is a regular
        /// animation it will return zero vectors
        /// </summary>
        /// <param name="animation">animation to search</param>
        /// <returns></returns>
        public static List<Vector3> GetAnimationMovement(HSceneProc.AnimationListInfo animation)
        {
            List<Vector3> result = new() { new Vector3(0, 0, 0), new Vector3(0, 0, 0) };

            if (swapAnimationMapping != null)
            {
                swapAnimationMapping.TryGetValue(animation, out var anim);
                if (anim != null)
                {
                    result[0] = anim.PositionPlayer;
                    result[1] = anim.PositionHeroine;
                }
            }
            return result;
        }

    }
}
