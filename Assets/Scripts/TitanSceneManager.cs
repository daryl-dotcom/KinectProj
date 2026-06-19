using UnityEngine;
using Photon.Pun;
using Vuforia;

public class TitanSceneManager : MonoBehaviour
{
    public Transform spawnPoint;

    private ObserverBehaviour observerBehaviour;
    private bool hasSpawned;

    void Start()
    {
        // Get the ObserverBehaviour from the ImageTarget this is parented under
        observerBehaviour = GetComponentInParent<ObserverBehaviour>();

        if (observerBehaviour != null)
        {
            observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }
        else
        {
            Debug.LogWarning("TitanSceneManager could not find an ObserverBehaviour in parent.");
        }
    }

    void OnDestroy()
    {
        if (observerBehaviour != null)
            observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        bool isTracked = status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED;

        if (isTracked && !hasSpawned)
        {
            SpawnTitan();
        }
    }

    void SpawnTitan()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        GameObject myTitan = PhotonNetwork.Instantiate("Net_Titan", spawnPos, spawnRot);

        myTitan.transform.SetParent(spawnPoint, false);
        myTitan.transform.localPosition = Vector3.zero;
        myTitan.transform.localRotation = Quaternion.identity;

        KinectManager kinect = FindFirstObjectByType<KinectManager>();
        if (kinect != null)
        {
            kinect.Player1Avatars.Add(myTitan);
            kinect.ResetAvatarControllers();
            Debug.Log("--- Titan successfully registered to Kinect Player 1 and controllers refreshed! ---");
        }

        hasSpawned = true;
        Debug.Log("--- Titan spawned after marker tracked! ---");
    }
}