using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterWaveManager : MonoBehaviourPun
{
    [Header("Spawn Points")]
    public Transform[] monsterSpawnPoints;

    [Header("Wave Settings")]
    public float firstWaveDelay = 8f;
    public float delayBetweenMonsters = 5f; // Spaced out time between entry
    public float delayBetweenWaves = 5f;

    private List<GameObject> aliveMonsters = new List<GameObject>();

    void Start()
    {
        // Network protection: Only run waves on the Master Client (AR laptop)
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(RunWaves());
        }
    }

    IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(firstWaveDelay);

        // --- WAVE 1 ---
        Debug.Log("=== WAVE 1 START ===");
        yield return StartCoroutine(SpawnMonstersSequentially("GoblinSmall", 5));
        
        // Wait until Player 2 wipes out all 5 before moving on
        yield return StartCoroutine(WaitUntilAllMonstersDead());
        Debug.Log("=== WAVE 1 CLEARED ===");

        yield return new WaitForSeconds(delayBetweenWaves);

        // --- WAVE 2 ---
        Debug.Log("=== WAVE 2 START ===");
        yield return StartCoroutine(SpawnMonstersSequentially("Hobgoblin", 3));
        
        yield return StartCoroutine(WaitUntilAllMonstersDead());
        Debug.Log("=== WAVE 2 CLEARED ===");

        yield return new WaitForSeconds(delayBetweenWaves);

        // --- WAVE 3 (BOSS) ---
        Debug.Log("=== WAVE 3 START (BOSS) ===");
        SpawnMonster("Troll", 0);

        yield return StartCoroutine(WaitUntilAllMonstersDead());
        Debug.Log("=== TROLL DEFEATED ===");
        Debug.Log("=== ARENA CLEARED ===");
    }

    IEnumerator SpawnMonstersSequentially(string monsterPrefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnMonster(monsterPrefab, i);
            // FIXED: Just wait for the delay between entries, don't halt on dead check inside loop!
            yield return new WaitForSeconds(delayBetweenMonsters);
        }
    }

    IEnumerator WaitUntilAllMonstersDead()
    {
        while (true)
        {
            aliveMonsters.RemoveAll(monster => monster == null);

            if (aliveMonsters.Count == 0)
            {
                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void SpawnMonster(string monsterPrefabName, int spawnIndex)
    {
        if (monsterSpawnPoints == null || monsterSpawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned in Wave Manager component!");
            return;
        }

        Transform spawnPoint = monsterSpawnPoints[spawnIndex % monsterSpawnPoints.Length];

        GameObject monster = PhotonNetwork.Instantiate(
            monsterPrefabName,
            spawnPoint.position,
            spawnPoint.rotation
        );

        aliveMonsters.Add(monster);
        Debug.Log("Successfully Spawned Network Entity: " + monsterPrefabName);
    }
}