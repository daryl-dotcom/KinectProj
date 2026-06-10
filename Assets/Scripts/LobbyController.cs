using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbyController : MonoBehaviourPunCallbacks
{
    void Start()
    {
        Debug.Log("--- PHOTON: Connecting to Server... ---");
        // PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("--- PHOTON: Connected! Joining or creating room 'Colosseum'... ---");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.JoinOrCreateRoom("Colosseum", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("--- PHOTON: Successfully entered room 'Colosseum'! ---");
        Debug.Log("Players in room right now: " + PhotonNetwork.CurrentRoom.PlayerCount);

        #if UNITY_ANDROID || UNITY_IOS
            Debug.Log("Mobile detected! Moving to VR Gladiator view...");
            PhotonNetwork.LoadLevel("Scene_VR_Gladiator");
        #else
            Debug.Log("PC Desktop detected! Moving to AR Titan view...");
            PhotonNetwork.LoadLevel("Scene_AR_Titan");
        #endif
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogWarning("============== NETWORK ALERT ==============");
        Debug.LogWarning("VR Gladiator (Player 2) has successfully connected to the room!");
        Debug.LogWarning("===========================================");
    }
}