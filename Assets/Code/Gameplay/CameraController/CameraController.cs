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
        [field: SerializeField] public float CameraPeekDistance { get; private set; } = 5f;
        [field: SerializeField] public float CameraLerpSpeed { get; private set; } = 10f;
        [field: SerializeField, InputPath] public string CameraPeekInput { get; private set; }

        [GlobalDefault] private InputManager _inputManager;
        [SerializeField] private Vector2Int _gridIndex;
        private float _cameraZ;

        private void Start()
        {
            if (Target == null) throw new System.Exception("CameraController requires a target to function.");
            DependencyInjector.InjectDependencies(this);
            _cameraZ = transform.position.z;
        }

        private void Update()
        {
            if (Target == null) return;
            _gridIndex = GetGridIndex(Target.position);
            Vector3 position = GetGridPosition(_gridIndex);

            Vector2 peekInput = _inputManager.ReadInput<Vector2>(CameraPeekInput);
            int peakDirection = Mathf.RoundToInt(peekInput.y);

            position.y += CameraPeekDistance * peakDirection;
            
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * CameraLerpSpeed);
        }

        private Vector2Int GetGridIndex(Vector2 position)
        {
            Vector2 offset = position - GridOrigin + new Vector2(GridSize.x / 2f, GridSize.y / 2f);
            Vector2Int index = new Vector2Int(Mathf.FloorToInt(offset.x / GridSize.x), Mathf.FloorToInt(offset.y / GridSize.y));
            return index;
        }
        
        private Vector3 GetGridPosition(Vector2Int index)
        {
            Vector3 position = GridOrigin + new Vector2(index.x * GridSize.x, index.y * GridSize.y);
            position.z = _cameraZ;
            return position;
        }

    }
}
