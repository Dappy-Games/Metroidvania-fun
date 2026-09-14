using UnityEngine;

public class Fireball : MonoBehaviour
{
    [SerializeField] float Damage;
    [SerializeField] float HitForce;
    [SerializeField] int Speed;
    [SerializeField] float LifeTime = 2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, LifeTime);
    }

    private void FixedUpdate()
    {
        transform.position += Speed * transform.right;
    }

  void OnTriggerEnter2D(Collider2D _other)
  {
        if (_other.tag == "Enemy")
        {
            _other.GetComponent<Enemy>().EnemyHit(Damage, (_other.transform.position - transform.position).normalized, -HitForce);
        }
    }
}
