using UnityEngine;

public class Henchman : Enemy
{
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask whatIsGround;

    [SerializeField] private float obstacleCheckDistance = 0.6f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float jumpForce = 8f;

    void Start()
    {
        rb.gravityScale = 3f;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();

        if (!isRecoiling)
        {
            float direction = Mathf.Sign(
                PlayerController.Instance.transform.position.x - transform.position.x
            );

            rb.linearVelocity = new Vector2(
                direction * speed,
                rb.linearVelocity.y
            );

            // Flip sprite to face player
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
        }
    }

    private void FixedUpdate()
    {
        if (isRecoiling)
            return;

        float direction = Mathf.Sign(
            PlayerController.Instance.transform.position.x - transform.position.x
        );

        // Check if grounded
        bool grounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            whatIsGround
        );

        // Check for obstacle in front
        RaycastHit2D obstacle = Physics2D.Raycast(
            transform.position,
            Vector2.right * direction,
            obstacleCheckDistance,
            obstacleLayer
        );

        // Jump if grounded and obstacle is in the way
        if (grounded && obstacle.collider != null)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }
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