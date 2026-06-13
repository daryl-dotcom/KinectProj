using UnityEngine;
using Photon.Pun;

public class KinectPlayer2NetworkSender : MonoBehaviour
{
    [Header("Target")]
    public string gladiatorTag = "Gladiator";

    [Header("Movement")]
    public float sendRate = 20f;
    public float xScale = 1.5f;
    public float zScale = 1.5f;
    public bool invertX = false;
    public bool invertZ = false;

    private KinectManager kinect;
    private PhotonView gladiatorView;
    private Vector3 kinectOrigin;
    private bool hasOrigin;
    private float nextSendTime;

    void Start()
    {
        kinect = KinectManager.Instance;
    }

    void Update()
    {
        if (kinect == null)
            kinect = KinectManager.Instance;

        if (kinect == null)
            return;

        if (!PhotonNetwork.InRoom)
            return;

        if (Time.time < nextSendTime)
            return;

        nextSendTime = Time.time + (1f / sendRate);

        FindGladiatorView();

        if (gladiatorView == null)
            return;

        uint player2Id = kinect.GetPlayer2ID();

        if (player2Id == 0)
            return;

        Vector3 kinectPos = kinect.GetUserPosition(player2Id);

        if (!hasOrigin)
        {
            kinectOrigin = kinectPos;
            hasOrigin = true;
        }

        Vector3 delta = kinectPos - kinectOrigin;

        float x = delta.x * xScale;
        float z = delta.z * zScale;

        if (invertX)
            x = -x;

        if (invertZ)
            z = -z;

        Vector3 moveOffset = new Vector3(x, 0f, z);

        gladiatorView.RPC(
            nameof(GladiatorKinectNetworkMover.RPC_SetKinectMoveOffset),
            RpcTarget.All,
            moveOffset
        );
    }

    void FindGladiatorView()
    {
        if (gladiatorView != null)
            return;

        GameObject gladiator = GameObject.FindGameObjectWithTag(gladiatorTag);

        if (gladiator == null)
            gladiator = GameObject.Find("Net_Gladiator(Clone)");

        if (gladiator != null)
            gladiatorView = gladiator.GetComponent<PhotonView>();
    }

    public void ResetKinectOrigin()
    {
        hasOrigin = false;
    }
}