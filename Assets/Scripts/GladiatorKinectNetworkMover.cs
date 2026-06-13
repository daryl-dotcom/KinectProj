using UnityEngine;
using Photon.Pun;

public class GladiatorKinectNetworkMover : MonoBehaviourPun
{
    public float smoothSpeed = 8f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool hasTarget;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition;
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        if (!hasTarget)
            return;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }

    [PunRPC]
    public void RPC_SetKinectMoveOffset(Vector3 moveOffset)
    {
        if (!photonView.IsMine)
            return;

        targetPosition = startPosition + moveOffset;
        hasTarget = true;
    }
}