using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElfSpawnerManager : MonoBehaviour
{
    [Tooltip("Time (in seconds) between spawns")]
    [Range(1, 10)]
    public float TimeBetweenSpawns = 5;
    
    [Tooltip("GameObject to be Spawned by the Spawners")]
    public GameObject elfPrefab; 

    private ElfSpawner[] elfSpawners = {};
    private float lastTimeBetweenSpawns;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");
        lastTimeBetweenSpawns = TimeBetweenSpawns;

        elfSpawners = FindObjectsOfType<ElfSpawner>();
        foreach (var elfSpawner in elfSpawners)
        {
            elfSpawner.InitializeSpawner(elfPrefab, TimeBetweenSpawns);
        }
    }

    private void OnValidate()
    {
        if (lastTimeBetweenSpawns != TimeBetweenSpawns)
        {
            foreach (var elfSpawner in elfSpawners)
            {
                elfSpawner.UpdateTimeBetweenSpawns(TimeBetweenSpawns);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
