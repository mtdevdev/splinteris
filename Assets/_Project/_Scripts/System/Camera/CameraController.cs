using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    [Header("Offset Settings")]
    [SerializeField] private Vector3 _defaultOffset = new Vector3(0, 6, 0);
    [SerializeField] private float _maxDynamicShift = 3f;
    [SerializeField] private float _maxMouseInfluenceDistance = 10f;

    [Header("Smoothing")]
    [SerializeField] private float _positionSmoothTime = 0.3f;
    [SerializeField] private float _mouseInputSmoothTime = 0.1f;

    [Header("Collision Management")]
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private float _collisionCheckRadius = 0.3f;
    [SerializeField] private LayerMask _groundMask;

    private CinemachineCamera _vcam;
    private CinemachineFollow _vcamFollow;

    private Vector3 _offsetVelocity;
    private Vector3 _smoothedMouseWorldPos;
    private Vector3 _mouseMovementVelocity;

    private void Awake()
    {
        _vcam = GetComponent<CinemachineCamera>();
        _vcamFollow = _vcam.GetComponent<CinemachineFollow>();

        if (_playerTransform != null) 
            _smoothedMouseWorldPos = _playerTransform.position;
    }

    private void LateUpdate()
    {
        if (_playerTransform == null || _vcamFollow == null) return;

        // 1. Process Mouse Input
        Vector3 rawMouseWorldPos = _playerTransform.position;
        Ray mouseRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(mouseRay, out RaycastHit hit, 100f, _groundMask))
        {
            rawMouseWorldPos = hit.point;
        }

        _smoothedMouseWorldPos = Vector3.SmoothDamp(_smoothedMouseWorldPos, rawMouseWorldPos, ref _mouseMovementVelocity, _mouseInputSmoothTime);

        // 2. Calculate Dynamic Offset Based on Mouse
        Vector3 playerToMouseDir = _smoothedMouseWorldPos - _playerTransform.position;
        playerToMouseDir.y = 0f;

        float distanceToMouse = playerToMouseDir.magnitude;
        float influenceFactor = Mathf.Clamp01(distanceToMouse / _maxMouseInfluenceDistance);

        Vector3 dynamicShift = Vector3.zero;
        if (distanceToMouse > 0.01f)
        {
            dynamicShift = playerToMouseDir.normalized * (_maxDynamicShift * influenceFactor);
        }

        // 3. Prevent Wall Clipping (Collision)
        Vector3 desiredOffset = _defaultOffset + dynamicShift;
        Vector3 targetCameraPos = _playerTransform.position + desiredOffset;
        Vector3 finalCalculatedOffset = desiredOffset;

        Vector3 castDirection = targetCameraPos - _playerTransform.position;
        float castDistance = castDirection.magnitude;

        if (Physics.SphereCast(_playerTransform.position, _collisionCheckRadius, castDirection.normalized, out RaycastHit collisionHit, castDistance, _collisionMask))
        {
            float safeDistance = Mathf.Max(0.5f, collisionHit.distance - _collisionCheckRadius);
            finalCalculatedOffset = castDirection.normalized * safeDistance;
            finalCalculatedOffset.y = _defaultOffset.y;
        }

        // 4. Apply Final Smooth Result
        _vcamFollow.FollowOffset = Vector3.SmoothDamp(_vcamFollow.FollowOffset, finalCalculatedOffset, ref _offsetVelocity, _positionSmoothTime);
    }
}