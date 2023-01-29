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
            Log.Warning($"Load time for LoadStudioAnims {elapsedTime}");
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

        /// <summary>
        /// String format Vector3 variables for easier logging
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string FormatVector(this Vector3 self)
        {
            var formatString = $"( {self.x, 11:F6} , {self.y, 11:F6}, {self.z, 11:F6} )";

            return formatString;
        }

        /// <summary>
        /// Adjust move vector to transform vector in Unity when using transform it looks
        /// that moving forward sometimes is in the X axis (Why?)
        /// </summary>
        /// <param name="self">object self reference</param>
        /// <param name="transform">character transform</param>
        /// <returns></returns>
        public static Vector3 MovementTransform(this Vector3 self, Transform transform)
        {
            var result = new Vector3(0, 0, 0);

            result += transform.right * self.x;
            result += transform.up * self.y;
            result += transform.forward * self.z;

            return result;
        }
    }
}
