using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace AnimationLoader
{
    public static class Extensions
    {
        /// <summary>
        /// Create a coroutine that calls each of the actions in order after base coroutine finishes
        /// </summary>
        public static IEnumerator AppendCo(this IEnumerator baseCoroutine, params Action[] actions)
        {
            return new object[] { baseCoroutine, CreateCoroutine(actions) }.GetEnumerator();
        }

        /// <summary>
        /// Create a coroutine that calls each of the action delegates on consecutive frames
        /// (yield return null is returned after each one of the actions)
        /// </summary>
        public static IEnumerator CreateCoroutine(params Action[] actions)
        {
#if DEBUG
            var stopWatch = new Stopwatch();

            stopWatch.Start();
#endif
            foreach (var action in actions)
            {
                action();
                yield return null;
            }
#if DEBUG
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;

            var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:0000}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds);
            Log.Warning($"LoadStudioAnims {elapsedTime}");
#endif
        }

        public static void SetRect(
            this Transform self,
            float anchorLeft = 0f,
            float anchorBottom = 0f,
            float anchorRight = 1f,
            float anchorTop = 1f,
            float offsetLeft = 0f,
            float offsetBottom = 0f,
            float offsetRight = 0f,
            float offsetTop = 0f)
        {
            var rt = (RectTransform)self;
            rt.anchorMin = new Vector2(anchorLeft, anchorBottom);
            rt.anchorMax = new Vector2(anchorRight, anchorTop);
            rt.offsetMin = new Vector2(offsetLeft, offsetBottom);
            rt.offsetMax = new Vector2(offsetRight, offsetTop);
        }
    }
}
