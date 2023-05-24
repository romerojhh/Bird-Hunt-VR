using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Object = System.Object;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour, ITakeDamage
{
    const string RUN_TRIGGER = "Run";
    const string CROUCH_TRIGGER = "Crouch";
    const string SHOOT_TRIGGER = "Shoot";
    const string DIE_TRIGGER = "Die";

    [SerializeField] private float startingHealth;
    // minimum and maximum time for enemy to stay in the cover
    [SerializeField] private float minTimeUnderCover;
    [SerializeField] private float maxTimeUnderCover;
    [SerializeField] private int minShootsToTake;
    [SerializeField] private int maxShootsToTake;
    [SerializeField] private float rotationSpeed;
    [Range(0, 100)] [SerializeField] private float shootingAccuracy;
    [SerializeField] private ParticleSystem bloodSplatterFX;
    [SerializeField] private Transform shootingPosition;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;

    private bool _isShooting;
    private int _currentShotsTaken;
    private int _currentMaxShotsToTake;
    private NavMeshAgent _agent;
    private Player _player;
    // A transform for the AI to go for a cover
    private Transform _occupiedCoverSpot;
    private Animator _animator;
    private float _health;
    public float health
    {
        get => _health;
        set => _health = Mathf.Clamp(value, 0, startingHealth);
    }

    private Weapon _weapon;
    private AudioSource _audioSource;
    private EnemySpawner _enemySpawner;
    private bool _isDead;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.SetTrigger(RUN_TRIGGER);
        _agent = GetComponent<NavMeshAgent>();
        _health = startingHealth;
        _weapon = GetComponentInChildren<Weapon>();
        _audioSource = GetComponent<AudioSource>();
        _isDead = false;
    }
    
    private void Update()
    {
        // if AI is not stopped and close to the cover spot
        if (!_agent.isStopped && (transform.position - _occupiedCoverSpot.position).sqrMagnitude <= 0.1f)
        {
            // make the AI stop walking to cover and shoot
            _agent.isStopped = true;
            StartCoroutine(InitializeShooting());
        } 
        else if (_isShooting)
        {
            RotateTowardsPlayer();
        }
    }
    
    void SetController(EnemySpawner cont){
        _enemySpawner = cont;
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction = _player.GetHeadPosition() - transform.position;
        direction.y = 0;
        Quaternion rotation = Quaternion.LookRotation(direction);
        rotation = Quaternion.RotateTowards(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        transform.rotation = rotation;
    }

    /**
     * Coroutine method that makes the AI to hide behind cover at specified time and shoot
     */
    private IEnumerator<WaitForSeconds> InitializeShooting()
    {
        HideBehindCover();
        yield return new WaitForSeconds(Random.Range(minTimeUnderCover, maxTimeUnderCover));
        StartShooting();
    }

    private void HideBehindCover()
    {
        _animator.SetTrigger(CROUCH_TRIGGER);
    }
    
    private void StartShooting()
    {
        _isShooting = true;
        _currentMaxShotsToTake = Random.Range(minShootsToTake, maxShootsToTake);
        _currentShotsTaken = 0;
        _animator.SetTrigger(SHOOT_TRIGGER);
    }

    /**
     * This function is called on animation event
     * Shoot at the player
     * If accuracy is closer to 100, the probability of hitting the player is higher
     */
    public void Shoot()
    {
        Debug.Log("Shoot called by AI");
        var hitPlayer = Random.Range(0, 100) <= shootingAccuracy;
        Vector3 direction = _player.GetHeadPosition() - shootingPosition.localPosition;

        if (!hitPlayer)
        {
            // "Fake" attempt to miss the player
            direction += new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1));
        }
        
        _weapon.transform.LookAt(direction);
        _weapon.Shoot();

        _currentShotsTaken++;
        if (_currentShotsTaken >= _currentMaxShotsToTake)
        {
            StartCoroutine(InitializeShooting());
        }
    }

    /**
     * This function is called on animation event
     */
    public void RemoveCorpse()
    {
        _enemySpawner.SendMessage("RemoveEnemy", _occupiedCoverSpot);
        Destroy(gameObject);
    }
    
    public void Init(Player player, Transform coverSpot)
    {
        _occupiedCoverSpot = coverSpot;
        _player = player;
        GetToCover();
    }

    /**
     * Make AI to walk into the cover
     */
    private void GetToCover()
    {
        _agent.isStopped = false;
        _agent.SetDestination(_occupiedCoverSpot.position);
    }
    
    public void TakeDamage(Weapon weapon, Projectile projectile, Vector3 contactPoint)
    {
        health -= weapon.GetDamage();
        
        if (health <= 0)
        {
            if (!_isDead)
            {
                _audioSource.PlayOneShot(deathSound);
                _animator.SetTrigger(DIE_TRIGGER);
                _isDead = true;
            }
        }
        else
        {
            _audioSource.PlayOneShot(hitSound);
        }
        
        // Show splatter of blood everytime AI takes damage
        ParticleSystem effect = Instantiate(bloodSplatterFX, contactPoint,
            Quaternion.LookRotation(weapon.transform.position - contactPoint));
        effect.Stop();
        effect.Play();
    }
}
