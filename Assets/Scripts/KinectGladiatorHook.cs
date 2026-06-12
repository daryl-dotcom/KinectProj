using UnityEngine;
using Photon.Pun;

public class KinectGladiatorHook : MonoBehaviour
{
    void Start()
    {
        // 1. Find the AR Laptop's Kinect Manager
        KinectManager kinect = Object.FindFirstObjectByType<KinectManager>();
        
        // 2. Grab the AvatarController on this Gladiator
        AvatarController avatar = GetComponent<AvatarController>();
        
        if (kinect != null)
        {
            // 3. Automatically inject this Gladiator into the Player 2 slot
            if (!kinect.Player2Avatars.Contains(gameObject))
            {
                kinect.Player2Avatars.Add(gameObject);
                
                // 4. Wake the AvatarController up NOW that it is safely inside the Player 2 list!
                if (avatar != null) 
                {
                    avatar.enabled = true;
                }

                kinect.ResetAvatarControllers(); 
                
                Debug.Log("--- Gladiator successfully hooked to Kinect Player 2! ---");
            }
        }
    }
}