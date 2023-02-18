//
// Load XML animation information
//
using UnityEngine;


namespace AnimationLoader
{
    public partial class SwapAnim
    {
        private static void Override(ref string lhs, string rhs)
        {
            if (rhs != null)
            {
                lhs = rhs;
            }
        }

        private static void Override(ref int lhs, int rhs)
        {
            if (rhs >= 0)
            {
                lhs = rhs;
            }
        }

        private static void DoOverrides(
            ref SwapAnimationInfo data,
            OverrideInfo overrides,
            ref Animation animation,
            bool overrideName)
        {
            if (overrides.PathFemale != null)
            {
                data.PathFemale = string.Copy(overrides.PathFemale);
            }
            if (overrides.ControllerFemale != null)
            {
                data.ControllerFemale = string.Copy(overrides.ControllerFemale);
            }

            Override(ref data.PathFemale1, overrides.PathFemale1);
            Override(ref data.ControllerFemale1, overrides.ControllerFemale1);

            if (overrides.PathMale != null)
            {
                data.PathMale = string.Copy(overrides.PathMale);
            }
            if (overrides.ControllerMale != null)
            {
                data.ControllerMale = string.Copy(overrides.ControllerMale);
            }
            if ((overrides.AnimationName != null) && !overrideName)
            {
                data.AnimationName = string.Copy(overrides.AnimationName);
                if (UserOverrides.Value)
                {
                    animation.KoikatsuSunshine = string.Copy(overrides.AnimationName);
                    animation.KoikatsuSunshineReference = string
                        .Copy(overrides.AnimationName);
                }
            }
            if (overrides.Mode >= 0)
            {
                data.Mode = overrides.Mode;
            }
            if (overrides.kindHoushi >= 0)
            {
                data.kindHoushi = overrides.kindHoushi;
            }

            overrides.categories?.CopyTo(data.categories, 0);

            if (overrides.DonorPoseId >= 0)
            {
                data.DonorPoseId = overrides.DonorPoseId;
            }

            if (overrides.NeckDonorId >= -1)
            {
                data.NeckDonorId = overrides.NeckDonorId;
            }

            Override(ref data.NeckDonorIdFemale, overrides.NeckDonorIdFemale);
            Override(ref data.NeckDonorIdFemale1, overrides.NeckDonorIdFemale1);

            if (overrides.NeckDonorIdMale >= 0)
            {
                data.NeckDonorIdMale = overrides.NeckDonorIdMale;
            }

            if (overrides.FileMotionNeck != null)
            {
                data.FileMotionNeck = string.Copy(overrides.FileMotionNeck);
            }
            if (overrides.FileMotionNeckMale != null)
            {
                data.FileMotionNeckMale = string.Copy(overrides.FileMotionNeckMale);
            }
            if (overrides.IsFemaleInitiative != null)
            {
                data.IsFemaleInitiative = overrides.IsFemaleInitiative;
            }
            if (overrides.FileSiruPaste != null)
            {
                data.FileSiruPaste = string.Copy(overrides.FileSiruPaste);
            }
            if (overrides.MotionIKDonor != null)
            {
                data.MotionIKDonor = overrides.MotionIKDonor;
            }
            if (overrides.MotionIKDonorFemale != null)
            {
                data.MotionIKDonorFemale = overrides.MotionIKDonorFemale;
            }
            if (overrides.MotionIKDonorFemale1 != null)
            {
                data.MotionIKDonorFemale1 = overrides.MotionIKDonorFemale1;
            }
            if (overrides.MotionIKDonorMale != null)
            {
                data.MotionIKDonorMale = overrides.MotionIKDonorMale;
            }
            if (overrides.ExpTaii >= 0)
            {
                data.ExpTaii = overrides.ExpTaii;
            }
            if (overrides.PositionHeroine != Vector3.zero)
            {
                data.PositionHeroine = overrides.PositionHeroine;
            }
            if (overrides.PositionPlayer != Vector3.zero)
            {
                data.PositionPlayer = overrides.PositionPlayer;
            }
        }
    }
}
