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

    [Header("Animation Names")]
    public string idleAnimation = "idle01";
    public string walkAnimation = "walk";
    public string attackAnimation = "attack01";
    public string damageAnimation = "damage";
    public string deathAnimation = "dead";

    [Header("Damage")]
    public int attackDamage = 10;

    private Transform target;
    private Animation monsterAnimation;
    private string currentAnimation;
    private float nextAttackTime;
    private bool isDead;
    private bool isStunned;
    private bool isAttacking;

    void Start()
    {
        monsterAnimation = GetComponent<Animation>();
        FindGladiator();
        PlayAnimationNetworked(idleAnimation);
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
            PlayAnimationNetworked(idleAnimation);
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
    if (isAttacking)
        return;

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

    PlayAnimationNetworked(walkAnimation);
}

void AttackTarget()
{
    transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

    if (isAttacking)
        return;

    if (Time.time < nextAttackTime)
    {
        PlayAnimationNetworked(idleAnimation);
        return;
    }

    StartCoroutine(AttackRoutine());
}

    System.Collections.IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        PlayAnimationNetworked(attackAnimation, true);

        yield return new WaitForSeconds(0.4f);

        GladiatorHealth health = target.GetComponent<GladiatorHealth>();
        if (health != null)
        {
            health.TakeDamage(attackDamage);
        }

        Debug.LogWarning("--- CHOP! Monster hits Gladiator Player 2! ---");

        yield return new WaitForSeconds(0.6f);

        isAttacking = false;
        PlayAnimationNetworked(idleAnimation);
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
        PlayAnimationNetworked(idleAnimation);

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

        PlayAnimationNetworked(deathAnimation, true);

        Collider monsterCollider = GetComponent<Collider>();
        if (monsterCollider != null)
            monsterCollider.enabled = false;

        // Later, when you want the wave manager to detect death:
        // StartCoroutine(DestroyAfterDeathAnimation());
    }

    System.Collections.IEnumerator DestroyAfterDeathAnimation()
    {
        yield return new WaitForSeconds(2f);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void PlayAnimationNetworked(string animationName, bool forceReplay = false)
    {
        if (string.IsNullOrEmpty(animationName))
            return;

        if (!forceReplay && currentAnimation == animationName)
            return;

        photonView.RPC(nameof(RPC_PlayAnimation), RpcTarget.All, animationName, forceReplay);
    }

    [PunRPC]
    void RPC_PlayAnimation(string animationName, bool forceReplay)
    {
        if (monsterAnimation == null)
            monsterAnimation = GetComponent<Animation>();

        if (monsterAnimation == null)
            return;

        if (monsterAnimation.GetClip(animationName) == null)
        {
            Debug.LogWarning("Missing animation clip: " + animationName + " on " + gameObject.name);
            return;
        }

        currentAnimation = animationName;

        if (forceReplay)
        {
            monsterAnimation.Stop(animationName);
            monsterAnimation.Play(animationName);
        }
        else
        {
            monsterAnimation.CrossFade(animationName, 0.2f);
        }
    }
}