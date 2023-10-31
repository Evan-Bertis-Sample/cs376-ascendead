using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ascendead.Components
{
    public class Bob : MonoBehaviour
    {
        [SerializeField] private float _bobSpeed = 1f;
        [SerializeField] private float _bobHeight = 1f;
        [SerializeField] private float _bobOffset = 0f;

        private float _time = 0f;
        private Vector3 _startPosition;

        private void Start()
        {
            _startPosition = transform.position;
        }

        private void Update()
        {
            _time += Time.deltaTime * _bobSpeed;
            float y = Mathf.Sin(_time) * _bobHeight + _bobOffset;
            transform.position = _startPosition + new Vector3(0, y, 0);
        }
    }
}
