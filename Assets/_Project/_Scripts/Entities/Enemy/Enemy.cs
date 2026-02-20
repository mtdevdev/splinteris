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
[RequireComponent(typeof(EnemyGunController))]
public class Enemy : MonoBehaviour
{
    #region Settings

    [Header("General Stats")]
    [SerializeField] private float health = 100f;
    [SerializeField] private string playerTag = "Player";

    [Header("Model Correction")]
    [Range(-180f, 180f)]
    [SerializeField] private float modelRotationYOffset = 0f; 

    [Header("AI Configuration")]
    [SerializeField] private float detectionRadius = 15f;
    [Range(0, 360)]
    [SerializeField] private float fieldOfView = 110f;
    [SerializeField] private float attackRange = 10f; 
    [SerializeField] private float stopChaseDistance = 20f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Movement")]
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float patrolTurnInterval = 4f;

    [Header("Audio/VFX")]
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private AudioClip deathSound;

    [Header("UI")]
    [SerializeField] private MainUI mainUI;

    #endregion

    #region Private Fields

    private NavMeshAgent _agent;
    private Animator _animator;
    private EnemyGunController _gunController;
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
        if (health <= 0) return;

        if (_playerTransform == null)
        {
            SwitchState(EnemyState.Idle);
            return;
        }

        StateMachineLogic();
        UpdateAnimations();
    }

    private void OnDrawGizmos()
    {

        Vector3 correctedForward = Quaternion.Euler(0, modelRotationYOffset, 0) * transform.forward;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.up, correctedForward * 2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (_currentState == EnemyState.Idle || _currentState == EnemyState.Chasing)
        {
            Gizmos.color = Color.green;
            Vector3 viewAngleA = DirFromAngle(-fieldOfView / 2, false);
            Vector3 viewAngleB = DirFromAngle(fieldOfView / 2, false);

            Gizmos.DrawLine(transform.position, transform.position + viewAngleA * detectionRadius);
            Gizmos.DrawLine(transform.position, transform.position + viewAngleB * detectionRadius);
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y + modelRotationYOffset;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    #endregion

    #region AI Logic

    private void InitializeComponents()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _gunController = GetComponent<EnemyGunController>();

        FindPlayer();
        
        _agent.stoppingDistance = attackRange * 0.7f; 
        _agent.updateRotation = false; 
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            _playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"[Enemy] Jogador com a tag '{playerTag}' não encontrado! O inimigo ficará parado.");
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

        if (newState == EnemyState.Idle)
        {
            _agent.isStopped = true;
        }
        else if (newState == EnemyState.Chasing)
        {
            _agent.isStopped = false;
        }
        else if (newState == EnemyState.Attacking)
        {
            _agent.isStopped = true;
        }
    }

    #endregion

    #region State Handlers

    private void HandleIdleState(bool canSeePlayer, float distance)
    {
        _patrolTimer += Time.deltaTime;
        if (_patrolTimer >= patrolTurnInterval)
        {
            float randomY = Random.Range(0f, 360f);
            _idleLookRotation = Quaternion.Euler(0, randomY, 0);
            _patrolTimer = 0;
        }
        
        RotateTowards(_idleLookRotation);

        if (canSeePlayer && distance <= detectionRadius)
        {
            SwitchState(EnemyState.Chasing);
        }
    }

    private void HandleChaseState(bool canSeePlayer, float distance)
    {
        if (_playerTransform == null) return;

        _agent.SetDestination(_playerTransform.position);
        
        if (_agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion moveRot = Quaternion.LookRotation(_agent.velocity.normalized);
            RotateTowards(moveRot);
        }

        if (distance <= attackRange && canSeePlayer)
        {
            SwitchState(EnemyState.Attacking);
        }
        else if (distance > stopChaseDistance)
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

        Player playerScript = _playerTransform.GetComponent<Player>();
        if (playerScript != null && !playerScript.isAlive)
        {
            SwitchState(EnemyState.Idle);
            return;
        }

        _gunController.TryShoot(); 

        if (distance > attackRange * 1.2f || !canSeePlayer)
        {
            SwitchState(EnemyState.Chasing);
        }
    }

    private void RotateTowards(Quaternion targetRotation)
    {
        Quaternion correctedTarget = targetRotation * Quaternion.Euler(0, modelRotationYOffset, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, correctedTarget, Time.deltaTime * rotationSpeed);
    }

    #endregion

    #region Helper Methods

    private bool CheckVisibility()
    {
        if (_playerTransform == null) return false;

        Vector3 dirToPlayer = (_playerTransform.position - transform.position).normalized;
        
        Vector3 currentForward = Quaternion.Euler(0, modelRotationYOffset, 0) * transform.forward;
        
        if (Vector3.Angle(currentForward, dirToPlayer) > fieldOfView / 2f)
        {
            return false;
        }

        float dstToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
        
        return !Physics.Raycast(transform.position + Vector3.up, dirToPlayer, dstToPlayer, obstacleMask);
    }

    private void UpdateAnimations()
    {
        bool isMoving = _agent.velocity.sqrMagnitude > 0.1f;
        _animator.SetBool("isMoving", isMoving);
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        
        if (_currentState == EnemyState.Idle && _playerTransform != null) 
        {
            SwitchState(EnemyState.Chasing);
        }
        
        if (health <= 0) Die();
    }

    private void Die()
    {
        _agent.enabled = false;
        
        mainUI.UpdateRemainingEnemies();
        
        if (deathEffect) Instantiate(deathEffect, transform.position, Quaternion.identity);
        
        if (deathSound)
        {
            GameObject soundObj = new GameObject("EnemyDeathSound");
            soundObj.transform.position = transform.position;
            
            AudioSource src = soundObj.AddComponent<AudioSource>();
            src.clip = deathSound;
            src.spatialBlend = 1f; 
            
            soundObj.AddComponent<AudioTimeScaleHandler>().Init(src);
        }

        Destroy(gameObject);
    }

    #endregion
}

public class AudioTimeScaleHandler : MonoBehaviour
{
    private AudioSource _source;
    private float _basePitch = 1f;

    public void Init(AudioSource src)
    {
        _source = src;
        _basePitch = Random.Range(0.9f, 1.1f);
        _source.playOnAwake = false;
        _source.Play();
        StartCoroutine(ManageAudio());
    }

    private IEnumerator ManageAudio()
    {
        while (_source != null && _source.isPlaying)
        {
            _source.pitch = _basePitch * Mathf.Max(0.01f, Time.timeScale);
            yield return null;
        }
        Destroy(gameObject);
    }
}