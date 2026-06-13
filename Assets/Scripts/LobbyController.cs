using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class LobbyController : MonoBehaviourPunCallbacks
{
    private const string RoomName = "Colosseum";
    private bool isVrPlayer;

    void Start()
    {
        isVrPlayer = IsVrBuild();

        Debug.Log("--- PHOTON: Connecting to Server... ---");
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        if (isVrPlayer)
        {
            Debug.Log("--- VR Player: Trying to join AR room... ---");
            PhotonNetwork.JoinRoom(RoomName);
        }
        else
        {
            Debug.Log("--- AR Player: Creating or joining room as Player 1... ---");

            RoomOptions roomOptions = new RoomOptions
            {
                MaxPlayers = 2
            };

            PhotonNetwork.JoinOrCreateRoom(RoomName, roomOptions, TypedLobby.Default);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("--- Joined room: " + RoomName + " ---");
        Debug.Log("Players in room: " + PhotonNetwork.CurrentRoom.PlayerCount);
        Debug.Log("Is Master Client: " + PhotonNetwork.IsMasterClient);

        if (isVrPlayer)
        {
            Debug.Log("--- Loading VR Gladiator scene... ---");
            PhotonNetwork.LoadLevel("Scene_VR_Gladiator");
        }
        else
        {
            Debug.Log("--- Loading AR Titan scene... ---");
            PhotonNetwork.LoadLevel("Scene_AR_Titan");
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (isVrPlayer)
        {
            Debug.LogWarning("--- VR Player: AR room not ready yet. Retrying... ---");
            StartCoroutine(RetryJoinRoom());
        }
    }

    IEnumerator RetryJoinRoom()
    {
        yield return new WaitForSeconds(2f);

        if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom)
        {
            PhotonNetwork.JoinRoom(RoomName);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("--- Player joined. Current player count: " + PhotonNetwork.CurrentRoom.PlayerCount + " ---");
    }

    bool IsVrBuild()
    {
#if UNITY_ANDROID || UNITY_IOS
        return true;
#else
        return false;
#endif
    }
}