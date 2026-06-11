using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class TitanSceneManager : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Transform spawnPoint; // Where the giant Titan will stand

    void Start()
    {
        Debug.Log("--- Titan AR Scene Loaded. Spawning Titan Avatar... ---");

        if (PhotonNetwork.IsConnected)
        {
            // 1. Spawn Player 1 (Titan) exactly at the spawn point
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            GameObject myTitan = PhotonNetwork.Instantiate("Net_Titan", spawnPos, spawnRot);

           // 2. Automatically register this Titan as Player 1 in the Kinect Manager
            KinectManager kinect = FindObjectOfType<KinectManager>();
            if (kinect != null)
            {
                // Notice the capital P, and we are passing "myTitan" directly!
                kinect.Player1Avatars.Add(myTitan);
                Debug.Log("--- Titan successfully registered to Kinect Player 1! ---");
            }
        }
    }
}