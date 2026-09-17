using UnityEngine;

public class Henchman : Enemy
{
    [SerializeField] private float obstacleCheckDistance = 0.6f;
    [SerializeField] private LayerMask obstacleLayer;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float jumpForce = 8f;

    [SerializeField] private float sightRange = 8f;
    [SerializeField] private float giveUpTime = 3f;

    [SerializeField] private Transform playerFeet;
    [SerializeField] private Transform playerHead;

    private Animator anim;

    [SerializeField] private float attackCooldown = 1f;
    private bool canAttack = true;

    private bool canSeePlayer;
    private bool chasingPlayer;
    private float losePlayerTimer;
    private Vector2 lastKnownPlayerPosition;

    void Start()
    {
        rb.gravityScale = 3f;
    }

    protected override void Awake()
    {
        base.Awake();

        anim = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        CheckPlayerSight();

        if (!isRecoiling && chasingPlayer)
        {
            float direction = Mathf.Sign(
                lastKnownPlayerPosition.x - transform.position.x
            );

            rb.linearVelocity = new Vector2(
                direction * speed,
                rb.linearVelocity.y
            );

            anim.SetBool("Walking", Mathf.Abs(rb.linearVelocity.x) > 0.1f);
            anim.SetBool("Jumping", rb.linearVelocity.y > 0.1f);

            if (direction > 0)
            {
                transform.localScale = new Vector2(
                    Mathf.Abs(transform.localScale.x),
                    transform.localScale.y
                );
            }
            else if (direction < 0)
            {
                transform.localScale = new Vector2(
                    -Mathf.Abs(transform.localScale.x),
                    transform.localScale.y
                );
            }

            if (Mathf.Abs(transform.position.x - lastKnownPlayerPosition.x) < 0.2f)
            {
                rb.linearVelocity = new Vector2(
                    0,
                    rb.linearVelocity.y
                );
            }
        }
        else if (!isRecoiling)
        {
            rb.linearVelocity = new Vector2(
                0,
                rb.linearVelocity.y
            );

            anim.SetBool("Walking", false);
        }
    }

    private void FixedUpdate()
    {
        if (isRecoiling || !chasingPlayer)
            return;

        float direction = Mathf.Sign(
            lastKnownPlayerPosition.x - transform.position.x
        );

        RaycastHit2D obstacle = Physics2D.Raycast(
            transform.position,
            Vector2.right * direction,
            obstacleCheckDistance,
            obstacleLayer
        );

        // Obstacle detection ray
        Debug.DrawRay(
            transform.position,
            Vector2.right * direction * obstacleCheckDistance,
            Color.red
        );

        bool grounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            whatIsGround
        );

        if (grounded && obstacle.collider != null)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }
    }

    private void CheckPlayerSight()
    {
        if (playerFeet == null || playerHead == null)
            return;

        Vector2 feetDirection =
            playerFeet.position - transform.position;

        Vector2 headDirection =
            playerHead.position - transform.position;

        float feetDistance = feetDirection.magnitude;
        float headDistance = headDirection.magnitude;

        bool feetCanBeSeen = false;
        bool headCanBeSeen = false;

        // FEET RAY
        if (feetDistance <= sightRange)
        {
            RaycastHit2D feetHit = Physics2D.Raycast(
                transform.position,
                feetDirection.normalized,
                feetDistance,
                obstacleLayer
            );

            feetCanBeSeen = feetHit.collider == null;

            Debug.DrawRay(
                transform.position,
                feetDirection.normalized * feetDistance,
                feetCanBeSeen ? Color.green : Color.red
            );
        }

        // HEAD RAY
        if (headDistance <= sightRange)
        {
            RaycastHit2D headHit = Physics2D.Raycast(
                transform.position,
                headDirection.normalized,
                headDistance,
                obstacleLayer
            );

            headCanBeSeen = headHit.collider == null;

            Debug.DrawRay(
                transform.position,
                headDirection.normalized * headDistance,
                headCanBeSeen ? Color.green : Color.red
            );
        }

        // If either ray can see the player, chase
        if (feetCanBeSeen || headCanBeSeen)
        {
            canSeePlayer = true;
            chasingPlayer = true;

            lastKnownPlayerPosition =
                PlayerController.Instance.transform.position;

            losePlayerTimer = giveUpTime;

            return;
        }

        // Player cannot currently be seen
        canSeePlayer = false;

        if (chasingPlayer)
        {
            losePlayerTimer -= Time.deltaTime;

            if (losePlayerTimer <= 0)
            {
                chasingPlayer = false;
            }
        }
    }

    // PERMANENTLY SHOW SIGHT RANGE IN SCENE VIEW
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            sightRange
        );
    }

    protected override void Attack()
    {
        if (!canAttack)
            return;

        canAttack = false;

        anim.SetTrigger("Attacking");

        base.Attack();

        StartCoroutine(AttackCooldown());
    }

    private System.Collections.IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    public override void EnemyHit(
        float _DamageDone,
        Vector2 _HitDirection,
        float _HitForce
    )
    {
        base.EnemyHit(_DamageDone, _HitDirection, _HitForce);
    }
}