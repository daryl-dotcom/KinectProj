using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class GladiatorArmIKReceiver : MonoBehaviourPun, IOnEventCallback
{
    [Header("IK Weights")]
    public float handWeight = 1f;
    public float elbowWeight = 1f;

    [Header("Smoothing")]
    public float smoothSpeed = 12f;

    [Header("Kinect Mapping")]
    public float xScale = 1.2f;
    public float yScale = 1.2f;
    public float zScale = 1.2f;
    public bool invertX = false;
    public bool invertZ = true;

    private Animator animator;

    private Vector3 leftHandTarget;
    private Vector3 rightHandTarget;
    private Vector3 leftElbowTarget;
    private Vector3 rightElbowTarget;

    private Vector3 currentLeftHandTarget;
    private Vector3 currentRightHandTarget;
    private Vector3 currentLeftElbowTarget;
    private Vector3 currentRightElbowTarget;

    private bool hasArmData;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.enabled = true;
        }
    }

    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    void Update()
    {
        if (!hasArmData)
            return;

        currentLeftHandTarget = Vector3.Lerp(currentLeftHandTarget, leftHandTarget, smoothSpeed * Time.deltaTime);
        currentRightHandTarget = Vector3.Lerp(currentRightHandTarget, rightHandTarget, smoothSpeed * Time.deltaTime);
        currentLeftElbowTarget = Vector3.Lerp(currentLeftElbowTarget, leftElbowTarget, smoothSpeed * Time.deltaTime);
        currentRightElbowTarget = Vector3.Lerp(currentRightElbowTarget, rightElbowTarget, smoothSpeed * Time.deltaTime);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != KinectPlayer2ArmSender.Player2ArmEventCode)
            return;

        if (!photonView.IsMine)
            return;

        object[] armData = photonEvent.CustomData as object[];

        if (armData == null || armData.Length < 4)
            return;

        SetKinectArms(
            (Vector3)armData[0],
            (Vector3)armData[1],
            (Vector3)armData[2],
            (Vector3)armData[3]
        );
    }

    void SetKinectArms(
        Vector3 leftElbowOffset,
        Vector3 rightElbowOffset,
        Vector3 leftHandOffset,
        Vector3 rightHandOffset)
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            return;

        Transform chest = animator.GetBoneTransform(HumanBodyBones.Chest);

        if (chest == null)
            chest = animator.GetBoneTransform(HumanBodyBones.Spine);

        if (chest == null)
            return;

        leftElbowTarget = ConvertKinectOffsetToWorld(chest, leftElbowOffset);
        rightElbowTarget = ConvertKinectOffsetToWorld(chest, rightElbowOffset);
        leftHandTarget = ConvertKinectOffsetToWorld(chest, leftHandOffset);
        rightHandTarget = ConvertKinectOffsetToWorld(chest, rightHandOffset);

        if (!hasArmData)
        {
            currentLeftElbowTarget = leftElbowTarget;
            currentRightElbowTarget = rightElbowTarget;
            currentLeftHandTarget = leftHandTarget;
            currentRightHandTarget = rightHandTarget;
            hasArmData = true;
        }
    }

    Vector3 ConvertKinectOffsetToWorld(Transform chest, Vector3 offset)
    {
        float x = offset.x * xScale;
        float y = offset.y * yScale;
        float z = offset.z * zScale;

        if (invertX)
            x = -x;

        if (invertZ)
            z = -z;

        return chest.position
            + transform.right * x
            + transform.up * y
            + transform.forward * z;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || !hasArmData)
            return;

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handWeight);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, handWeight);

        animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, elbowWeight);
        animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, elbowWeight);

        animator.SetIKPosition(AvatarIKGoal.LeftHand, currentLeftHandTarget);
        animator.SetIKPosition(AvatarIKGoal.RightHand, currentRightHandTarget);

        animator.SetIKHintPosition(AvatarIKHint.LeftElbow, currentLeftElbowTarget);
        animator.SetIKHintPosition(AvatarIKHint.RightElbow, currentRightElbowTarget);
    }
}
