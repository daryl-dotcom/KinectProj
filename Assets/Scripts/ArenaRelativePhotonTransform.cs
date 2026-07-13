using UnityEngine;
using Photon.Pun;

public class ArenaRelativePhotonTransform : MonoBehaviourPun, IPunObservable
{
    public string arenaRootName = "ArenaRoot";
    public float smoothSpeed = 12f;

    private Transform arenaRoot;
    private Vector3 targetWorldPosition;
    private Quaternion targetWorldRotation;
    private bool hasTarget;

    void Start()
    {
        FindArenaRoot();
        targetWorldPosition = transform.position;
        targetWorldRotation = transform.rotation;
    }

    void Update()
    {
        if (arenaRoot == null)
            FindArenaRoot();

        if (photonView.IsMine || !hasTarget)
            return;

        transform.position = Vector3.Lerp(transform.position, targetWorldPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetWorldRotation, smoothSpeed * Time.deltaTime);
    }

    void FindArenaRoot()
    {
        GameObject root = GameObject.Find(arenaRootName);
        if (root != null)
            arenaRoot = root.transform;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (arenaRoot == null)
            FindArenaRoot();

        if (arenaRoot == null)
            return;

        if (stream.IsWriting)
        {
            Vector3 localPosition = arenaRoot.InverseTransformPoint(transform.position);
            Quaternion localRotation = Quaternion.Inverse(arenaRoot.rotation) * transform.rotation;

            stream.SendNext(localPosition);
            stream.SendNext(localRotation);
        }
        else
        {
            Vector3 localPosition = (Vector3)stream.ReceiveNext();
            Quaternion localRotation = (Quaternion)stream.ReceiveNext();

            targetWorldPosition = arenaRoot.TransformPoint(localPosition);
            targetWorldRotation = arenaRoot.rotation * localRotation;
            hasTarget = true;
        }
    }
}