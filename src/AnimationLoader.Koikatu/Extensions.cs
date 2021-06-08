using System;
using System.Collections;
using UnityEngine;

namespace AnimationLoader.Koikatu
{
    public static class Extensions
    {
        /// <summary>
        /// Create a coroutine that calls the appendCoroutine after base coroutine finishes
        /// </summary>
        public static IEnumerator AppendCo(this IEnumerator baseCoroutine, IEnumerator appendCoroutine)
        {
            return new[] { baseCoroutine, appendCoroutine }.GetEnumerator();
        }

        /// <summary>
        /// Create a coroutine that calls the yieldInstruction after base coroutine finishes.
        /// Append further coroutines tu run after this.
        /// </summary>
        public static IEnumerator AppendCo(this IEnumerator baseCoroutine, YieldInstruction yieldInstruction)
        {
            return new object[] { baseCoroutine, yieldInstruction }.GetEnumerator();
        }

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
            foreach (var action in actions)
            {
                action();
                yield return null;
            }
        }

        /// <summary>
        /// Create a coroutine that calls each of the supplied coroutines in order
        /// </summary>
        public static IEnumerator ComposeCoroutine(params IEnumerator[] coroutine)
        {
            return coroutine.GetEnumerator();
        }
        
        public static void SetRect(this RectTransform self, Vector2 anchorMin)
        {
            SetRect(self, anchorMin, Vector2.one, Vector2.zero, Vector2.zero);
        }
        public static void SetRect(this RectTransform self, Vector2 anchorMin, Vector2 anchorMax)
        {
            SetRect(self, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        }
        public static void SetRect(this RectTransform self, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin)
        {
            SetRect(self, anchorMin, anchorMax, offsetMin, Vector2.zero);
        }
        public static void SetRect(this RectTransform self, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            self.anchorMin = anchorMin;
            self.anchorMax = anchorMax;
            self.offsetMin = offsetMin;
            self.offsetMax = offsetMax;
        }

        public static void SetRect(this RectTransform self, RectTransform other)
        {
            self.anchorMin = other.anchorMin;
            self.anchorMax = other.anchorMax;
            self.offsetMin = other.offsetMin;
            self.offsetMax = other.offsetMax;
        }

        public static void SetRect(this RectTransform self, float anchorLeft = 0f, float anchorBottom = 0f, float anchorRight = 1f, float anchorTop = 1f, float offsetLeft = 0f, float offsetBottom = 0f, float offsetRight = 0f, float offsetTop = 0f)
        {
            self.anchorMin = new Vector2(anchorLeft, anchorBottom);
            self.anchorMax = new Vector2(anchorRight, anchorTop);
            self.offsetMin = new Vector2(offsetLeft, offsetBottom);
            self.offsetMax = new Vector2(offsetRight, offsetTop);
        }

        public static void SetRect(this Transform self, Transform other)
        {
            SetRect(self as RectTransform, other as RectTransform);
        }

        public static void SetRect(this Transform self, Vector2 anchorMin)
        {
            SetRect(self as RectTransform, anchorMin, Vector2.one, Vector2.zero, Vector2.zero);
        }
        public static void SetRect(this Transform self, Vector2 anchorMin, Vector2 anchorMax)
        {
            SetRect(self as RectTransform, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        }
        public static void SetRect(this Transform self, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin)
        {
            SetRect(self as RectTransform, anchorMin, anchorMax, offsetMin, Vector2.zero);
        }
        public static void SetRect(this Transform self, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rt = self as RectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
        }

        public static void SetRect(this Transform self, float anchorLeft = 0f, float anchorBottom = 0f, float anchorRight = 1f, float anchorTop = 1f, float offsetLeft = 0f, float offsetBottom = 0f, float offsetRight = 0f, float offsetTop = 0f)
        {
            RectTransform rt = self as RectTransform;
            rt.anchorMin = new Vector2(anchorLeft, anchorBottom);
            rt.anchorMax = new Vector2(anchorRight, anchorTop);
            rt.offsetMin = new Vector2(offsetLeft, offsetBottom);
            rt.offsetMax = new Vector2(offsetRight, offsetTop);
        }
    }
}