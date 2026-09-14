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
            transform.position = Vector2.MoveTowards
                (transform.position, new Vector2(PlayerController.Instance.transform.position.x, transform.position.y),
                speed * Time.deltaTime);
        }
    }
    public override void EnemyHit(float _DamageDone, Vector2 _HitDirection, float _HitForce)
    {
        base.EnemyHit(_DamageDone, _HitDirection, _HitForce);
    }
}
