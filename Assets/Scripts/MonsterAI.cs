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

    [Header("Animation Clips")]
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip attackAnimation;
    public AnimationClip damageAnimation;
    public AnimationClip deathAnimation;

    private Transform target;
    private Animation monsterAnimation;
    private float nextAttackTime;
    private bool isDead;
    private bool isStunned;

    void Start()
    {
        monsterAnimation = GetComponent<Animation>();
        FindGladiator();

        PlayAnimation(idleAnimation);
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

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
        GameObject gladiator = GameObject.FindGameObjectWithTag(gladiatorTag);

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

        PlayAnimation(walkAnimation);
    }

    void AttackTarget()
    {
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

        if (Time.time < nextAttackTime)
        {
            PlayAnimation(idleAnimation);
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        PlayAnimation(attackAnimation);

        // Later: call gladiator health damage here.
        // Example:
        // target.GetComponent<GladiatorHealth>()?.TakeDamage(10);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        PlayAnimation(damageAnimation);

        // Later: subtract health here.
        // currentHealth -= damage;
        // if (currentHealth <= 0) Die();
    }

    public void Stun(float duration)
    {
        if (isDead)
            return;

        photonView.RPC(nameof(RPC_Stun), RpcTarget.All, duration);
    }

    [PunRPC]
    void RPC_Stun(float duration)
    {
        if (isDead)
            return;

        StartCoroutine(StunRoutine(duration));
    }

    System.Collections.IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        PlayAnimation(idleAnimation);

        yield return new WaitForSeconds(duration);

        isStunned = false;
    }

    public void Die()
    {
        if (isDead)
            return;

        photonView.RPC(nameof(RPC_Die), RpcTarget.All);
    }

    [PunRPC]
    void RPC_Die()
    {
        isDead = true;
        PlayAnimation(deathAnimation);

        Collider monsterCollider = GetComponent<Collider>();
        if (monsterCollider != null)
            monsterCollider.enabled = false;
    }

    void PlayAnimation(AnimationClip animationClip)
    {
        if (monsterAnimation == null)
            return;

        if (animationClip == null)
            return;

        if (monsterAnimation.GetClip(animationClip.name) == null)
            return;

        if (!monsterAnimation.IsPlaying(animationClip.name))
        {
            monsterAnimation.CrossFade(animationClip.name, 0.2f);
        }
    }
}