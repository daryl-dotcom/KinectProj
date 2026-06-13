using UnityEngine;
using Photon.Pun;

public class KinectPlayer2ArmSender : MonoBehaviour
{
    public string gladiatorTag = "Gladiator";
    public float sendRate = 30f;

    private KinectManager kinect;
    private PhotonView gladiatorView;
    private float nextSendTime;

    void Start()
    {
        kinect = KinectManager.Instance;
    }

    void Update()
    {
        if (kinect == null)
            kinect = KinectManager.Instance;

        if (kinect == null || !PhotonNetwork.InRoom)
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

        int shoulderCenter = (int)KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter;
        int elbowLeft = (int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft;
        int elbowRight = (int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight;
        int handLeft = (int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft;
        int handRight = (int)KinectWrapper.NuiSkeletonPositionIndex.HandRight;

        if (!kinect.IsJointTracked(player2Id, shoulderCenter) ||
            !kinect.IsJointTracked(player2Id, elbowLeft) ||
            !kinect.IsJointTracked(player2Id, elbowRight) ||
            !kinect.IsJointTracked(player2Id, handLeft) ||
            !kinect.IsJointTracked(player2Id, handRight))
        {
            return;
        }

        Vector3 shoulderPos = kinect.GetJointPosition(player2Id, shoulderCenter);

        Vector3 leftElbowOffset = kinect.GetJointPosition(player2Id, elbowLeft) - shoulderPos;
        Vector3 rightElbowOffset = kinect.GetJointPosition(player2Id, elbowRight) - shoulderPos;
        Vector3 leftHandOffset = kinect.GetJointPosition(player2Id, handLeft) - shoulderPos;
        Vector3 rightHandOffset = kinect.GetJointPosition(player2Id, handRight) - shoulderPos;

        gladiatorView.RPC(
            nameof(GladiatorArmIKReceiver.RPC_SetKinectArms),
            RpcTarget.All,
            leftElbowOffset,
            rightElbowOffset,
            leftHandOffset,
            rightHandOffset
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
}