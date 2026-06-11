using UnityEngine;
using Photon.Pun;

public class MonsterAI : MonoBehaviourPun
{
    [Header("Target")]
    public string gladiatorTag = "Gladiator";

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float rotationSpeed = 6f;
    public float attackDistance = 1.8f;

    [Header("Attack")]
    public float attackCooldown = 1.5f;

    private Transform target;
    private Animator monsterAnimator; // FIXED: Using modern Animator system
    private float nextAttackTime;
    private bool isDead;
    private bool isStunned;

    void Start()
    {
        monsterAnimator = GetComponent<Animator>();
        FindGladiator();
    }

    void Update()
    {
        // Only let the Master Client calculate AI movement to prevent network stuttering
        if (!PhotonNetwork.IsMasterClient) return;

        if (isDead || isStunned)
            return;

        if (target == null)
        {
            FindGladiator();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > attackDistance)
        {
            MoveTowardTarget();
        }
        else
        {
            AttackTarget();
        }
    }

    void FindGladiator()
    {
        // Try to find via Tag
        GameObject gladiator = GameObject.FindGameObjectWithTag(gladiatorTag);

        // Fallback: search by name clone if tag wasn't set correctly in Inspector
        if (gladiator == null)
        {
            gladiator = GameObject.Find("Net_Gladiator(Clone)");
        }

        if (gladiator != null)
        {
            target = gladiator.transform;
        }
    }

    void MoveTowardTarget()
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        transform.position += transform.forward * moveSpeed * Time.deltaTime;

        // FIXED: Safely triggers standard walking states if parameters exist
        if (monsterAnimator != null)
        {
            monsterAnimator.SetBool("IsWalking", true);
            monsterAnimator.SetBool("Moving", true); // covering common naming asset packs
        }
    }

    void AttackTarget()
    {
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

        if (monsterAnimator != null)
        {
            monsterAnimator.SetBool("IsWalking", false);
            monsterAnimator.SetBool("Moving", false);
        }

        if (Time.time < nextAttackTime)
            return;

        nextAttackTime = Time.time + attackCooldown;
        
        if (monsterAnimator != null)
        {
            monsterAnimator.SetTrigger("Attack");
        }

        Debug.LogWarning("--- CHOP! Monster hits Gladiator Player 2! ---");
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (monsterAnimator != null) monsterAnimator.SetTrigger("Hit");
    }

    public void Stun(float duration)
    {
        if (isDead) return;
        photonView.RPC(nameof(RPC_Stun), RpcTarget.All, duration);
    }

    [PunRPC]
    void RPC_Stun(float duration)
    {
        if (isDead) return;
        StartCoroutine(StunRoutine(duration));
    }

    System.Collections.IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    public void Die()
    {
        if (isDead) return;
        photonView.RPC(nameof(RPC_Die), RpcTarget.All);
    }

    [PunRPC]
    void RPC_Die()
    {
        isDead = true;
        if (monsterAnimator != null) monsterAnimator.SetTrigger("Die");

        Collider monsterCollider = GetComponent<Collider>();
        if (monsterCollider != null) monsterCollider.enabled = false;
    }
}