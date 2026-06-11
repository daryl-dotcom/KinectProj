using UnityEngine;
using Photon.Pun;
using System.Collections.Generic; // Required to talk to the Kinect lists

public class GladiatorSceneManager : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Camera vrCamera; 
    public Transform spawnPoint; 

    void Start()
    {
        Debug.Log("--- Gladiator VR Scene Loaded. Spawning Gladiator Avatar... ---");

        if (PhotonNetwork.IsConnected)
        {
            // 1. Spawn Player 2
            GameObject myGladiator = PhotonNetwork.Instantiate("Net_Gladiator", spawnPoint.position, spawnPoint.rotation);

            // 2. Camera Setup (Stable root follow)
            if (vrCamera != null)
            {
                vrCamera.transform.SetParent(myGladiator.transform);
                vrCamera.transform.localPosition = new Vector3(0f, 1.6f, 0.2f); 
                vrCamera.transform.localRotation = Quaternion.identity;
            }

            // 3. Automatically register this Gladiator as Player 2 in the Kinect Manager
            KinectManager kinect = FindFirstObjectByType<KinectManager>();
            if (kinect != null)
            {
                kinect.Player2Avatars.Add(myGladiator);
                
                // THE MISSING LINK: Tell the Kinect to refresh its internal tracking lists!
                kinect.ResetAvatarControllers(); 
                
                Debug.Log("--- Gladiator successfully registered to Kinect Player 2 and controllers refreshed! ---");
            }
        }
    }
}