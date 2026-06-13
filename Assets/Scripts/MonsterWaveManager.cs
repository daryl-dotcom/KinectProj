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
    public float delayBetweenMonsters = 1.5f;
    public float delayBetweenWaves = 5f;

    private bool wavesStarted = false;
    private List<GameObject> aliveMonsters = new List<GameObject>();

    void Start()
    {
        TryStartWaves();
    }

    void Update()
    {
        TryStartWaves();
    }

    void TryStartWaves()
    {
        if (wavesStarted)
            return;

        if (!PhotonNetwork.InRoom)
            return;

        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
            return;

        wavesStarted = true;
        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        Debug.Log("Both players are in the room. Monster waves will start.");

        yield return new WaitForSeconds(firstWaveDelay);

        Debug.Log("=== WAVE 1 START ===");
        yield return StartCoroutine(SpawnMonstersSequentially("GoblinSmall", 5));
        yield return StartCoroutine(WaitUntilAllMonstersDead());

        yield return new WaitForSeconds(delayBetweenWaves);

        Debug.Log("=== WAVE 2 START ===");
        yield return StartCoroutine(SpawnMonstersSequentially("Hobgoblin", 3));
        yield return StartCoroutine(WaitUntilAllMonstersDead());

        yield return new WaitForSeconds(delayBetweenWaves);

        Debug.Log("=== WAVE 3 START ===");
        SpawnMonster("Troll", 0);
        yield return StartCoroutine(WaitUntilAllMonstersDead());

        Debug.Log("=== ARENA CLEARED ===");
    }

    IEnumerator SpawnMonstersSequentially(string monsterPrefab, int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnMonster(monsterPrefab, i);
            yield return new WaitForSeconds(delayBetweenMonsters);
        }
    }

    IEnumerator WaitUntilAllMonstersDead()
    {
        while (true)
        {
            aliveMonsters.RemoveAll(monster => monster == null);

            if (aliveMonsters.Count == 0)
                yield break;

            yield return new WaitForSeconds(0.5f);
        }
    }

    void SpawnMonster(string monsterPrefabName, int spawnIndex)
    {
        if (monsterSpawnPoints == null || monsterSpawnPoints.Length == 0)
        {
            Debug.LogError("No monster spawn points assigned.");
            return;
        }

        Transform spawnPoint = monsterSpawnPoints[spawnIndex % monsterSpawnPoints.Length];

        GameObject monster = PhotonNetwork.Instantiate(
            monsterPrefabName,
            spawnPoint.position,
            spawnPoint.rotation
        );

        aliveMonsters.Add(monster);
        Debug.Log("Spawned monster: " + monsterPrefabName);
    }
}