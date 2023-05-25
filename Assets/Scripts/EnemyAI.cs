using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
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
    [SerializeField] private bool targetBird;
    [SerializeField] private MultiAimConstraint multiAimConstraint;

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
    private lb_BirdController _birdController;
    private Transform _target;
    private lb_Bird _targetLBBird;
    private List<ParticleSystem> _effects = new List<ParticleSystem>();
    private RigBuilder _rigBuilder;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.SetTrigger(RUN_TRIGGER);
        _agent = GetComponent<NavMeshAgent>();
        _health = startingHealth;
        _weapon = GetComponentInChildren<Weapon>();
        _audioSource = GetComponent<AudioSource>();
        _isDead = false;
        _rigBuilder = GetComponent<RigBuilder>();
    }
    
    private void Update()
    {
        if (!_agent.isStopped)
        {
            if (_isDead)
            {
                _agent.isStopped = true;
            } 
            else if ((transform.position - _occupiedCoverSpot.position).sqrMagnitude <= 0.1f)
            {
                // make the AI stop walking to cover and shoot
                _agent.isStopped = true;
                StartCoroutine(InitializeShooting());
            }
        }
        else if (_isShooting)
        {
            if (targetBird)
            {
                RotateTowardsRandomBird();
            }
            else
            {
                RotateTowardsPlayer();
            }
        }

        
    }
    
    /**
     * called on SendMessage function call
     */
    void SetController(EnemySpawner cont){
        _enemySpawner = cont;
    }

    /**
     * Rotate AI towards player
     */
    private void RotateTowardsPlayer()
    {
        RotateTo(_player.GetHeadPosition());
    }

    /**
     * Rotate AI towards random bird in lb_BirdController.myBirds
     */
    private void RotateTowardsRandomBird()
    {
        GameObject[] birds = _birdController.GetBirds();
        // Point at random bird if the current target is null 
        // or when current target is not active
        if (_target == null || !_target.gameObject.activeSelf || _targetLBBird.IsDead())
        {
            // make so that we only points to active and alive bird
            // TODO: Check infinity loop
            while (true)
            {
                GameObject currBird = birds[Random.Range(0, birds.Length)];
                lb_Bird currLbBird = currBird.GetComponent<lb_Bird>();
                if (currBird.activeSelf && !currLbBird.IsDead())
                {
                    _target = currBird.transform;
                    _targetLBBird = currLbBird;
                    // set the direction so that the AI facing towards the bird
                    multiAimConstraint.data.sourceObjects =  new WeightedTransformArray{new WeightedTransform(_target, 1f)};
                    _rigBuilder.Build();
                    break;
                }
            }
        }

        var position = _target.position;
        RotateTo(position);
    }

    private void RotateTo(Vector3 target)
    {
        Vector3 direction = target - transform.position;
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
        var hitPlayer = Random.Range(0, 100) <= shootingAccuracy;

        Vector3 direction;
        
        if (targetBird)
        {
            direction = _target.position - shootingPosition.localPosition;
        }
        else
        {
            direction = _player.GetHeadPosition() - shootingPosition.localPosition;
        }

        // 100% accuracy when AI targeting the bird
        if (!hitPlayer && !targetBird)
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
        foreach (var effect in _effects)
        {
            Destroy(effect);
        }
    }
    
    public void Init(Player player, Transform coverSpot, lb_BirdController birdController)
    {
        _occupiedCoverSpot = coverSpot;
        _player = player;
        GetToCover();
        _birdController = birdController;
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
                multiAimConstraint.weight = 0f;
            }
        }
        else
        {
            _audioSource.PlayOneShot(hitSound);
        }
        
        // Show splatter of blood everytime AI takes damage
        ParticleSystem effect = Instantiate(bloodSplatterFX, contactPoint,
            Quaternion.LookRotation(weapon.transform.position - contactPoint));
        _effects.Add(effect);
        effect.Stop();
        effect.Play();
    }
}
