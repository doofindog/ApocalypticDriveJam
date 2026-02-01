using System;
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

    private void Start()
    {
        agents ??= new List<NavMeshAgent>();

        for (int i = 0; i < enemyLoadCount; i++)
        {
            var enemyObj = Instantiate(enemyPrefab);
            NavMeshAgent navAgent = enemyObj.GetComponent<NavMeshAgent>();
            
            agents.Add(navAgent);

            enemyObj.transform.position = new Vector3()
            {
                x = Random.Range(-bounds.x, bounds.x),
                y = 1.5f,
                z = Random.Range(-bounds.z, bounds.z)
            };
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
