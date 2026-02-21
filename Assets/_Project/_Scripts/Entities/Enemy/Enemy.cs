using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(GunController))]
public class Enemy : MonoBehaviour
{
    #region Settings

    [Header("General Stats")]
    [SerializeField] private float _health = 100f;
    [SerializeField] private string _playerTag = "Player";

    [Header("Model Correction")]
    [Range(-180f, 180f)]
    [SerializeField] private float _modelRotationYOffset = 0f; 

    [Header("AI Configuration")]
    [SerializeField] private float _detectionRadius = 15f;
    [Range(0, 360)]
    [SerializeField] private float _fieldOfView = 110f;
    [SerializeField] private float _attackRange = 10f; 
    [SerializeField] private float _stopChaseDistance = 20f;
    [SerializeField] private LayerMask _obstacleMask;

    [Header("Movement")]
    [SerializeField] private float _rotationSpeed = 8f;
    [SerializeField] private float _patrolTurnInterval = 4f;

    [Header("Audio/VFX")]
    [SerializeField] private GameObject _deathEffectPrefab;
    [SerializeField] private AudioClip _deathSound;

    #endregion

    #region Private Fields

    private NavMeshAgent _agent;
    private Animator _animator;
    private GunController _gunController;
    private Transform _playerTransform;
    
    private EnemyState _currentState;
    private float _patrolTimer;
    private Quaternion _idleLookRotation;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        InitializeComponents();
        _currentState = EnemyState.Idle;
        _idleLookRotation = transform.rotation;
    }

    private void Update()
    {
        if (_health <= 0) return;

        if (_playerTransform == null)
        {
            SwitchState(EnemyState.Idle);
            return;
        }

        StateMachineLogic();
        UpdateAnimations();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 correctedForward = Quaternion.Euler(0, _modelRotationYOffset, 0) * transform.forward;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.up, correctedForward * 2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        if (_currentState == EnemyState.Idle || _currentState == EnemyState.Chasing)
        {
            Gizmos.color = Color.green;
            Vector3 viewAngleA = DirFromAngle(-_fieldOfView / 2, false);
            Vector3 viewAngleB = DirFromAngle(_fieldOfView / 2, false);

            Gizmos.DrawLine(transform.position, transform.position + viewAngleA * _detectionRadius);
            Gizmos.DrawLine(transform.position, transform.position + viewAngleB * _detectionRadius);
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y + _modelRotationYOffset;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    #endregion

    #region AI Logic

    private void InitializeComponents()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _gunController = GetComponent<GunController>();

        FindPlayer();
        
        _agent.stoppingDistance = _attackRange * 0.7f; 
        _agent.updateRotation = false; 
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(_playerTag);
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"[Enemy] Player with tag '{_playerTag}' not found! Enemy will remain idle.");
        }
    }

    private void StateMachineLogic()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        bool canSeePlayer = CheckVisibility();

        switch (_currentState)
        {
            case EnemyState.Idle:
                HandleIdleState(canSeePlayer, distanceToPlayer);
                break;

            case EnemyState.Chasing:
                HandleChaseState(canSeePlayer, distanceToPlayer);
                break;

            case EnemyState.Attacking:
                HandleAttackState(canSeePlayer, distanceToPlayer);
                break;
        }
    }

    private void SwitchState(EnemyState newState)
    {
        _currentState = newState;

        if (newState == EnemyState.Idle || newState == EnemyState.Attacking)
        {
            if (_agent.isOnNavMesh) _agent.isStopped = true;
        }
        else if (newState == EnemyState.Chasing)
        {
            if (_agent.isOnNavMesh) _agent.isStopped = false;
        }
    }

    #endregion

    #region State Handlers

    private void HandleIdleState(bool canSeePlayer, float distance)
    {
        _patrolTimer += Time.deltaTime;
        if (_patrolTimer >= _patrolTurnInterval)
        {
            float randomY = Random.Range(0f, 360f);
            _idleLookRotation = Quaternion.Euler(0, randomY, 0);
            _patrolTimer = 0;
        }
        
        RotateTowards(_idleLookRotation);

        if (canSeePlayer && distance <= _detectionRadius)
        {
            SwitchState(EnemyState.Chasing);
        }
    }

    private void HandleChaseState(bool canSeePlayer, float distance)
    {
        if (_playerTransform == null) return;

        if (_agent.isOnNavMesh)
        {
            _agent.SetDestination(_playerTransform.position);
        }
        
        if (_agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion moveRot = Quaternion.LookRotation(_agent.velocity.normalized);
            RotateTowards(moveRot);
        }

        if (distance <= _attackRange && canSeePlayer)
        {
            SwitchState(EnemyState.Attacking);
        }
        else if (distance > _stopChaseDistance)
        {
            SwitchState(EnemyState.Idle);
        }
    }

    private void HandleAttackState(bool canSeePlayer, float distance)
    {
        if (_playerTransform == null) return;

        Vector3 dirToPlayer = (_playerTransform.position - transform.position).normalized;
        dirToPlayer.y = 0;
        
        if (dirToPlayer != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dirToPlayer);
            RotateTowards(targetRot);
        }

        if (_playerTransform.TryGetComponent(out Player playerScript) && !playerScript.IsAlive)
        {
            SwitchState(EnemyState.Idle);
            return;
        }

        // Use the unified GunController to shoot
        if (_gunController != null)
        {
            _gunController.TryShoot(); 
        }

        if (distance > _attackRange * 1.2f || !canSeePlayer)
        {
            SwitchState(EnemyState.Chasing);
        }
    }

    private void RotateTowards(Quaternion targetRotation)
    {
        Quaternion correctedTarget = targetRotation * Quaternion.Euler(0, _modelRotationYOffset, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, correctedTarget, Time.deltaTime * _rotationSpeed);
    }

    #endregion

    #region Helper Methods

    private bool CheckVisibility()
    {
        if (_playerTransform == null) return false;

        Vector3 dirToPlayer = (_playerTransform.position - transform.position).normalized;
        Vector3 currentForward = Quaternion.Euler(0, _modelRotationYOffset, 0) * transform.forward;
        
        if (Vector3.Angle(currentForward, dirToPlayer) > _fieldOfView / 2f)
        {
            return false;
        }

        float dstToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        return !Physics.Raycast(transform.position + Vector3.up, dirToPlayer, dstToPlayer, _obstacleMask);
    }

    private void UpdateAnimations()
    {
        if (_animator != null)
        {
            bool isMoving = _agent.velocity.sqrMagnitude > 0.1f;
            _animator.SetBool("isMoving", isMoving);
        }
    }

    public void TakeDamage(float amount)
    {
        _health -= amount;
        
        if (_currentState == EnemyState.Idle && _playerTransform != null) 
        {
            SwitchState(EnemyState.Chasing);
        }
        
        if (_health <= 0) Die();
    }

    private void Die()
    {
        _agent.enabled = false;
        
        if (_deathEffectPrefab != null)
        {
            Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (_deathSound != null)
        {
            GameObject soundObj = new GameObject("EnemyDeathSound");
            soundObj.transform.position = transform.position;
            
            AudioSource src = soundObj.AddComponent<AudioSource>();
            src.clip = _deathSound;
            src.spatialBlend = 1f; 
            
            soundObj.AddComponent<TimeHandler>().Init(src);
        }

        Destroy(gameObject);
    }

    #endregion
}