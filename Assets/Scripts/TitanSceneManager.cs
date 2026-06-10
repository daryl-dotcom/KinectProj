using UnityEngine;
using Photon.Pun;

public class TitanSceneManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("--- Titan Scene Loaded. Spawning Titan Avatar... ---");

        // Spawns the Titan into the AR scene 
        // Photon handles syncing this object across the network automatically
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Instantiate("Net_Titan", Vector3.zero, Quaternion.identity);
        }
    }
}