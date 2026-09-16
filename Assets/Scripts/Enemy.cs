using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float RecoilLength;
    [SerializeField] protected float RecoilFactor;
    [SerializeField] protected bool isRecoiling = false;

    [SerializeField] protected PlayerController player;
    [SerializeField] protected float speed;

    [SerializeField] protected float Damage;

    protected float RecoilTimer;
    protected Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {     
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instance;
    }

    // Update is called once per frame
    protected virtual void Update()
{
    if (health <= 0)
    {
        Destroy(gameObject);
    }
    if (isRecoiling)
    {
        if (RecoilTimer < RecoilLength)
        {
            RecoilTimer += Time.deltaTime;
        }
        else
        {
            isRecoiling = false;
            RecoilTimer = 0;
            rb.linearVelocity = Vector2.zero;
        }
    }
}
    
    public virtual void EnemyHit(float _DamageDone, Vector2 _HitDirection, float _HitForce)
    {
        health -= _DamageDone;
        if(!isRecoiling)
        {
            rb.AddForce(-_HitForce * RecoilFactor * _HitDirection);
            isRecoiling = true;
        }
    }
    protected void OnTriggerStay2D(Collider2D _Other)
    {
        if (_Other.CompareTag("Player") && !PlayerController.Instance.pState.Invincible)
        {
            Attack();
            PlayerController.Instance.HitStopTime(0, 5, 0.5f);
        }
    }
    protected virtual void Attack()
        {
        PlayerController.Instance.TakeDamage(Damage);
    }

}
