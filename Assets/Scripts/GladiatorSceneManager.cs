using UnityEngine;
using Photon.Pun;

public class GladiatorSceneManager : MonoBehaviour
{
    void Start()
    {
        Debug.Log("--- Gladiator Scene Loaded. Spawning Gladiator Avatar... ---");

        // Spawns the Gladiator capsule at position (0, 0, 2) so they don't spawn right inside the Titan
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Instantiate("Net_Gladiator", new Vector3(0f, 0f, 2f), Quaternion.identity);
        }
    }
}