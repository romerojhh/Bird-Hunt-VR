using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private EnemyAI enemyPrefab;
    [SerializeField] private float spawnInterval;
    [SerializeField] private int maxEnemiesNumber;
    [SerializeField] private Player player;
    [SerializeField] private lb_BirdController birdController;
    
    private float _timeSinceLastSpawn;
    private Stack<Transform> _available = new Stack<Transform>();
    private List<EnemyAI> _activeEnemies = new List<EnemyAI>();
    private bool _currentlyDancing;
    private bool _targetPlayer;

    private void Start()
    {
        _currentlyDancing = false;
        _targetPlayer = false;
        _timeSinceLastSpawn = spawnInterval;
        foreach (var stuff in spawnPoints)
        {
            _available.Push(stuff);
        }
    }

    private void Update()
    {
        // Time that it took from last Update() call to current call
        _timeSinceLastSpawn += Time.deltaTime;
        if (_timeSinceLastSpawn > spawnInterval)
        {
            _timeSinceLastSpawn = 0f;
            // 3 - 3 < 1 T
            // 3 - 2 < 1 F
            // 3 - 1 < 1 F
            // 3 - 0 < 1 F
            if (_available.Count != 0 && spawnPoints.Length - _available.Count < maxEnemiesNumber && !_currentlyDancing)
            {
                // Spawn enemy
                SpawnEnemy();
            }
        }
    }

    /**
     * Method to spawn enemy in the specified spawn points
     */
    private void SpawnEnemy()
    {
        // don't spawn new enemy when dancing is on progress
        if (_currentlyDancing) return;
        
        if (_available.Count == 0)
        {
            Debug.LogError("Can't spawn new enemy, all space occupied");
            return;
        }
        
        var transform1 = transform;
        EnemyAI enemy = Instantiate(enemyPrefab, transform1.position, transform1.rotation);
        _activeEnemies.Add(enemy);
        enemy.SendMessage("SetController", this);
        enemy.SendMessage(_targetPlayer ? "TargetPlayer" : "TargetBird");
        enemy.Init(player, _available.Pop(), birdController);
    }

    /**
     * Called on SendMessage
     */
    private void RemoveEnemy(Transform unusedCover)
    {
        _available.Push(unusedCover);
    }
    
    /**
     * Called on SendMessage
     */
    private void RemoveEnemy(EnemyAI enemy)
    {
        _activeEnemies.Remove(enemy);
    }

    /**
     * This function called on red button on game
     */
    public void MakeEnemyDance()
    {
        if (_currentlyDancing)
        {
            _currentlyDancing = false;
            foreach (var enemy in _activeEnemies)
            {
                enemy.SendMessage("EndDance");
            }
        }
        else
        {
            _currentlyDancing = true;
            foreach (var enemy in _activeEnemies)
            {
                enemy.SendMessage("Dance");
            }
        }
    }

    public void TargetPlayer()
    {
        _targetPlayer = !_targetPlayer;
        if (_targetPlayer)
        {
            foreach (var enemy in _activeEnemies)
            {
                enemy.SendMessage("TargetPlayer");
            }
        }
        else
        {
            foreach (var enemy in _activeEnemies)
            {
                enemy.SendMessage("TargetBird");
            }
        }
    }
}
