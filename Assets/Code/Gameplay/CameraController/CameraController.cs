using System.Collections;
using System.Collections.Generic;
using CurlyCore;
using CurlyCore.Input;
using UnityEngine;

namespace Ascendead.Components
{
    public class CameraController : MonoBehaviour
    {
        [field: Header("Grid Settings")]
        [field: SerializeField] public Vector2 GridSize { get; private set; } = new Vector2(30f, 17f);
        [field: SerializeField] public Vector2 GridOrigin { get; private set; } = new Vector2(0f, 0.4735f);
        [field: SerializeField] public Transform Target { get; private set; }

        [field: Header("Camera Peeking Settings")]
        [field: SerializeField] public float CameraPeekDistance { get; private set; } = 1f;
        [field: SerializeField] public float CameraPeekSpeed { get; private set; } = 1f;
        [field: SerializeField] public AnimationCurve CameraPeekCurve { get; private set; } = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [field: SerializeField, InputPath] public string CameraPeekInput { get; private set; }

        [GlobalDefault] private InputManager _inputManager;
        [SerializeField] private Vector2Int _gridIndex;

        private void Start()
        {
            if (Target == null) throw new System.Exception("CameraController requires a target to function.");
            DependencyInjector.InjectDependencies(this);
        }

        private void Update()
        {
            if (Target == null) return;
            _gridIndex = GetGridIndex(Target.position);
            Vector2 position = GetGridPosition(_gridIndex);

            transform.position = position;
        }

        private Vector2Int GetGridIndex(Vector2 position)
        {
            Vector2 offset = position - GridOrigin;
            Vector2Int index = new Vector2Int(Mathf.FloorToInt(offset.x / GridSize.x), Mathf.FloorToInt(offset.y / GridSize.y));
            return index;
        }
        
        private Vector2 GetGridPosition(Vector2Int index)
        {
            Vector2 position = GridOrigin + new Vector2(index.x * GridSize.x, index.y * GridSize.y);
            return position;
        }

    }
}
