using UnityEngine;

public class Henchman : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.gravityScale = 3f;
    }
    protected override void Awake()
    {
        base.Awake();
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (!isRecoiling)
        {
            float direction = Mathf.Sign(
                PlayerController.Instance.transform.position.x - transform.position.x
            );

            rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
        }
    }
    public override void EnemyHit(float _DamageDone, Vector2 _HitDirection, float _HitForce)
    {
        base.EnemyHit(_DamageDone, _HitDirection, _HitForce);
    }
}
