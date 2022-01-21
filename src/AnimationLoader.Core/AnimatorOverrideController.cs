using System.Linq;

using UnityEngine;

namespace AnimationLoader
{
    public partial class SwapAnim
    {
        private static AnimatorOverrideController SetupAnimatorOverrideController(
            RuntimeAnimatorController src,
            RuntimeAnimatorController over)
        {
            if (src == null || over == null)
            {
                return null;
            }

            var aoc = new AnimatorOverrideController(src);
            var target = new AnimatorOverrideController(over);
            foreach (var ac in src.animationClips.Where(x => x != null)) //thanks omega/katarsys
            {
                aoc[ac.name] = ac;
            }

            foreach (var ac in target.animationClips.Where(x => x != null)) //thanks omega/katarsys
            {
                aoc[ac.name] = ac;
            }

            aoc.name = over.name;
            return aoc;
        }
    }
}
