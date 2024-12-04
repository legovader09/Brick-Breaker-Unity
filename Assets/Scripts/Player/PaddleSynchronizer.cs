using System;
using UnityEngine;

namespace Player
{
    public class PaddleSynchronizer : MonoBehaviour
    {
        public GameObject leftEdge;
        public GameObject rightEdge;
        public GameObject leftAnchor;
        public GameObject rightAnchor;
        private float _size;
        private float _originalScale;
    
        public void SetPaddleSize(float size)
        {
            _size = size;
            var newSize = _originalScale * _size;
            transform.localScale = new(newSize, 1, 1);
        
            if (Mathf.Approximately(_size, 1f))
            {
                leftEdge.transform.localPosition = Vector3.zero;
                rightEdge.transform.localPosition = Vector3.zero;
            }
            else
            {
                leftEdge.transform.localPosition = new(leftAnchor.transform.localPosition.x * (_size - 1), 0, 0);
                rightEdge.transform.localPosition = new(rightAnchor.transform.localPosition.x * (_size - 1), 0, 0);
            }
        }

        public void ModifyPaddleSize(float size)
        {
            SetPaddleSize(Math.Min(3f, Math.Max(.5f, _size * size)));
        }
    
        private void Start()
        {
            _originalScale = transform.localScale.x;
        }
    }
}
