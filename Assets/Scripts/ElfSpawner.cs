using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElfSpawner : MonoBehaviour
{
    [Tooltip("Specify the color of the elves to be spawned by this spawner")]
    public Color ElfColor;

    private float timeBetweenSpawns = 5.0f;
    private float timeUntilNextSpawn = 5.0f;
    private bool isInitialized = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInitialized)
        {
            return;
        }
        timeUntilNextSpawn -= Time.deltaTime;
        if (timeUntilNextSpawn < 0)
        {
            SpawnElf();
            timeUntilNextSpawn = timeBetweenSpawns;
        }
    }

    private void SpawnElf()
    {
        
    }

    public void InitializeSpawner(GameObject elfPrefab, float initialTimeBetweenSpawns)
    {
        timeBetweenSpawns = initialTimeBetweenSpawns;
        isInitialized = true;
    }

    public void UpdateTimeBetweenSpawns(float newSpawnRate)
    {
        timeBetweenSpawns = newSpawnRate;
        // TODO: Maybe we can spawn an elf and reset the count, or just reset the count
    }
}
