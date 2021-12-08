using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AnimationLoader
{
    public class TextScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform transBase;
        public TextMeshProUGUI textMesh;
        public float speed = 70f;
        private bool move; 

        public void OnPointerEnter(PointerEventData eventData)
        {
            move = true;
            StartCoroutine(MoveText());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            move = false;
            MarginSet(0f);
        }

        private IEnumerator MoveText()
        {
            while(move)
            {
                if(textMesh.preferredWidth - Math.Abs(textMesh.margin.x) < transBase.sizeDelta.x * 0.5f)
                    MarginSet(transBase.sizeDelta.x * 0.5f);
                else
                    MarginAdd(-speed * Time.deltaTime);
                
                yield return null;
            }
        }

        private void MarginAdd(float value)
        {
            var margin = textMesh.margin;
            margin.x += value;
            textMesh.margin = margin;
        }
        
        private void MarginSet(float value)
        {
            var margin = textMesh.margin;
            margin.x = value;
            textMesh.margin = margin;
        }
    }
}