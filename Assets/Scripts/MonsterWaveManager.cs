using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MonsterWaveManager : MonoBehaviourPun
{
    [Header("Spawn Points")]
    public Transform[] monsterSpawnPoints;

    [Header("Wave Settings")]
    public float firstWaveDelay = 2f;
    public float delayBetweenMonsters = 1f;
    public float delayBetweenWaves = 3f;

    private List<GameObject> aliveMonsters = new List<GameObject>();

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(RunWaves());
        }
    }

    IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(firstWaveDelay);

        Debug.Log("=== WAVE 1 START ===");

        yield return StartCoroutine(
            SpawnMonstersSequentially("GoblinSmall", 5)
        );

        Debug.Log("=== WAVE 1 CLEARED ===");

        yield return new WaitForSeconds(delayBetweenWaves);

        Debug.Log("=== WAVE 2 START ===");

        yield return StartCoroutine(
            SpawnMonstersSequentially("Hobgoblin", 3)
        );

        Debug.Log("=== WAVE 2 CLEARED ===");

        yield return new WaitForSeconds(delayBetweenWaves);

        Debug.Log("=== WAVE 3 START ===");

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

            yield return StartCoroutine(WaitUntilAllMonstersDead());

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
            Debug.LogError("No spawn points assigned!");
            return;
        }

        Transform spawnPoint =
            monsterSpawnPoints[spawnIndex % monsterSpawnPoints.Length];

        GameObject monster = PhotonNetwork.Instantiate(
            monsterPrefabName,
            spawnPoint.position,
            spawnPoint.rotation
        );

        aliveMonsters.Add(monster);

        Debug.Log("Spawned: " + monsterPrefabName);
    }
}