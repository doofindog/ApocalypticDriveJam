using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform player;

    [Header("Spawn Settings")] 
    [SerializeField] private Vector3 bounds;
    [SerializeField] private int enemyLoadCount;

    [Header("Chase Settings")] 
    [SerializeField] private float updateRate;
    [SerializeField] private float chaseRange;
    
    private List<NavMeshAgent> agents;

    private IEnumerator Start()
    {
        agents ??= new List<NavMeshAgent>();

        for (int i = 0; i < enemyLoadCount; i++)
        {
            var enemyObj = Instantiate(enemyPrefab);
            NavMeshAgent navAgent = enemyObj.GetComponent<NavMeshAgent>();
            
            agents.Add(navAgent);

            bool isValidSpawn = false;

            while (!isValidSpawn)
            {
                Vector3 spawnPosition = new Vector3()
                {
                    x = Random.Range(-bounds.x, bounds.x),
                    y = 0,
                    z = Random.Range(-bounds.z, bounds.z)
                };
                
                Collider[] results = new Collider[5]; 
                int hitCount = Physics.OverlapSphereNonAlloc(spawnPosition, 2.5f, results, LayerMask.GetMask("FrequencyIgnore"));
                if (hitCount == 0)
                {
                    isValidSpawn = true;
                    
                    if (enemyObj.TryGetComponent(out NavMeshAgent agent))
                    {
                        // Sometimes agents misbehave if enabled before a valid position
                        bool wasEnabled = agent.enabled;
                        agent.enabled = false;

                        enemyObj.transform.position = spawnPosition;

                        agent.enabled = wasEnabled;

                        // Warp ensures internal navmesh state is correct
                        agent.Warp(spawnPosition);
                    }
                }
                
               
            }

            yield return null;
        }        
    }

    private void Update()
    {
        updateRate += Time.deltaTime;
        if(updateRate < 0.1f)
            return;
        
        foreach (var agent in agents)
        {
            Transform agentTransform = agent.transform;
            float distance = Vector3.Distance(player.position, agentTransform.position);
            if (distance < chaseRange)
                agent.SetDestination(player.position);
        }

        updateRate = 0;
    }
}
