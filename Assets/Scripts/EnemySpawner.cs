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

    private void Start()
    {
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
            if (_available.Count != 0 && spawnPoints.Length - _available.Count < maxEnemiesNumber)
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
        var transform1 = transform;
        EnemyAI enemy = Instantiate(enemyPrefab, transform1.position, transform1.rotation);
        enemy.SendMessage("SetController", this);

        if (_available.Count == 0)
        {
            Debug.LogError("Can't spawn new enemy, all space occupied");
            return;
        }
        enemy.Init(player, _available.Pop(), birdController);
    }

    /**
     * 1st param type = EnemyAI
     * 2nd param type = Transform
     */
    private void RemoveEnemy(Transform unusedCover)
    {
        _available.Push(unusedCover);
    }
}
