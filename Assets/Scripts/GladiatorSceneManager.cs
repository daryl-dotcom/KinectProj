using UnityEngine;
using Photon.Pun;

public class GladiatorSceneManager : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public Camera vrCamera; // We will plug your Main Camera into this slot

    void Start()
    {
        Debug.Log("--- Gladiator VR Scene Loaded. Spawning Gladiator Avatar... ---");

        if (PhotonNetwork.IsConnected)
        {
            // 1. Spawn Player 2
            GameObject myGladiator = PhotonNetwork.Instantiate("Net_Gladiator", new Vector3(0f, 0f, 3f), Quaternion.identity);

            // 2. Find the character's Head bone using Unity's built-in Animator map
            Animator gladiatorAnim = myGladiator.GetComponentInChildren<Animator>();
            
            if (gladiatorAnim != null && vrCamera != null)
            {
                Transform headBone = gladiatorAnim.GetBoneTransform(HumanBodyBones.Head);

                if (headBone != null)
                {
                    // 3. Snap the camera inside the head
                    vrCamera.transform.SetParent(headBone);
                    
                    // Move it slightly forward so you don't see the inside of your own face mesh
                    vrCamera.transform.localPosition = new Vector3(0f, 0.1f, 0.1f); 
                    vrCamera.transform.localRotation = Quaternion.identity;
                }
            }
        }
    }
}