using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.AppUI.Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings:")]  
    [SerializeField] private float walkSpeed = 1;
    [Space(5)]

    [Header("Jump Settings:")]
    [SerializeField] private float jumpForce = 30;
    private float JumpBufferCounter = 0;
    [SerializeField] private float JumpBufferFrames;
    private float CoyoteTimeCounter = 0;
    [SerializeField] private float CoyoteTime;
    private int AirJumpCounter = 0;
    [SerializeField] private int MaxAirJumps;
    [Space(5)]

    [Header("Ground Check Settings:")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask WhatIsGround;
    [SerializeField] private LayerMask WhatIsWall;
    [Space(5)]

    [Header("Dash Settings:")]
    [SerializeField] private float DashSpeed;
    [SerializeField] private float DashTime;
    [SerializeField] private float DashCooldown;
    [SerializeField] GameObject dashEffect;
    [Space(5)]

    [Header("Attack Settings:")]
    bool attack = false;
    [SerializeField] private float TimeBetweenAttack;
    private float TimeSinceAttack;
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask AttackableLayer;
    [SerializeField] float Damage;
    [SerializeField] GameObject SlashEffect;
    [SerializeField] GameObject Slash;
    [SerializeField] GameObject Shockwave;
    bool restoreTime;
    float restoreTimeSpeed;
    [Space(5)]

    [Header("Recoil Settings:")]
    [SerializeField] int RecoilXSteps = 5;
    [SerializeField] int RecoilYSteps = 5;
    [SerializeField] float RecoilXSpeed = 100;
    [SerializeField] float RecoilYSpeed = 100;
    private int StepsXRecoiled, StepsYRecoiled;
    [SerializeField] private float WallRecoilForce = 10f;
    [Space(5)]

    [Header("Health Settings:")]
    public int health;
    public int MaxHealth;
    [SerializeField] GameObject BloodSpurt;
    [SerializeField] float HitFlashSpeed;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallBack;


    float HealTimer;
    [SerializeField] float TimeToHeal;
    [Space(5)]

    [Header("Mana Settings")]
    [SerializeField] UnityEngine.UI.Image ManaStorage;
    [SerializeField] float Mana;
    [SerializeField] float ManaDrainSpeed;
    [SerializeField] float ManaGain;
    [Space(5)]

    [Header("Spell Settings")]
    [SerializeField] float ManaSpellCost = 0.3f;
    [SerializeField] float TimeBetweenCasts = 0.1f;
    float TimeSinceCast;
    [SerializeField] float SpellDamage;
    [SerializeField] float DownSpellForce;

    [SerializeField] GameObject SideSpellFireball;
    [SerializeField] GameObject UpSpellExplosion;
    [SerializeField] GameObject DownSpellFireball;
    [Space(5)]


    [HideInInspector] public PlayerStateList pState;
    private Rigidbody2D rb;
    private float xAxis, yAxis;
    private float gravity;
    private Animator anim;
    private bool canDash = true;
    private bool Dashed;
    private SpriteRenderer sr;

    public static PlayerController Instance;

    private void Awake()
    {
       if(Instance != null && Instance != this)
            {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
       Health = MaxHealth;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pState = GetComponent<PlayerStateList>();

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        anim = GetComponent<Animator>();

        gravity = rb.gravityScale;

        mana = Mana;
        ManaStorage.fillAmount = Mana;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);

        Gizmos.color = Color.green;
        if (SideAttackTransform != null)
            Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F KEY PRESSED!");
        }
        GetInputs();
        UpdateJumpVariables();
        StartDash();
        Flip();
        Move();
        Jump();
        Attack();
        RestoreTimeScale();
        FlashWhileInvincible();
        Heal();
        CastSpell();
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.GetComponent<Enemy>() != null && pState.Casting)
        {
            _other.GetComponent<Enemy>().EnemyHit(SpellDamage, (_other.transform.position - transform.position).normalized, RecoilYSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (pState.Dashing) return;
        Recoil();
    }
    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Attack1");
    }

    void Flip()
    {
        if(xAxis < 0)
        {
            transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            //transform.eulerAngles = new Vector2(0, 180);
            pState.LookingRight = false;
        }
        else if(xAxis > 0)
        {
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            //transform.eulerAngles = new Vector2(0, 0);
            pState.LookingRight = true;
        }
    }
    private void Move()
    {
        if (!pState.Dashing)  // <-- Add this check
        {
            rb.linearVelocity = new Vector2(walkSpeed * xAxis, rb.linearVelocity.y);
            anim.SetBool("Walking", rb.linearVelocity.x != 0 && Grounded());
        }
    }

    void StartDash()
    {
        if (Input.GetButtonDown("Dash") && canDash && !Dashed)
        {
            StartCoroutine(Dash());
            Dashed = true;
        }

        if (Grounded())
        {
            Dashed = false;
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        pState.Dashing = true;
        anim.SetTrigger("Dashing");
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(transform.localScale.x * DashSpeed, 0);
        if (Grounded()) Instantiate(dashEffect, transform);
        yield return new WaitForSeconds(DashTime);
        rb.gravityScale = gravity;
        pState.Dashing = false;
        yield return new WaitForSeconds(DashCooldown);
        canDash = true;
    }
    void Attack()
    {
        TimeSinceAttack += Time.deltaTime;
        if(attack && TimeSinceAttack >= TimeBetweenAttack)
        {
            TimeSinceAttack = 0;
            anim.SetTrigger("Attacking");

            if(yAxis == 0 || yAxis < 0 && Grounded())
            {
                Hit(SideAttackTransform, SideAttackArea, ref pState.RecoilingX, RecoilXSpeed);
                Instantiate(SlashEffect, SideAttackTransform);      
            }
            else if(yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.RecoilingY, RecoilYSpeed);
                SlashEffectAtAngle(SlashEffect, 80, UpAttackTransform);
            }
            else if(yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.RecoilingY, RecoilYSpeed);
                SlashEffectAtAngle(SlashEffect, -80, DownAttackTransform);
            }
        }
    }

    void SpawnParticleEffect(GameObject particleEffect, Vector3 position, float rotationZ)
    {
        GameObject instance = Instantiate(particleEffect, position, Quaternion.identity);

        ParticleSystem ps = instance.GetComponent<ParticleSystem>();
        if (ps != null && particleEffect.name != "Shockwave")
        {
            StartCoroutine(DestroyParticleEffect(instance, ps));
        }
        else if (particleEffect.name != "Shockwave")
        {
            Destroy(instance, 2f);
        }
    }

    IEnumerator DestroyParticleEffect(GameObject instance, ParticleSystem ps)
    {
        while (ps != null && ps.isPlaying)
        {
            yield return null;
        }
        
        if (instance != null)
        {
            yield return new WaitForSeconds(0.2f);
            Destroy(instance);
        }
    }

    private void Hit(Transform _AttackTransform, Vector2 _AttackArea, ref bool _RecoilDir, float _RecoilStrength)
    {
        Collider2D[] ObjectsToHit = Physics2D.OverlapBoxAll(_AttackTransform.position, _AttackArea, 0, AttackableLayer);
        Collider2D[] FloorToHit = Physics2D.OverlapBoxAll(_AttackTransform.position, _AttackArea, 0, WhatIsGround);
        Collider2D[] WallToHit = Physics2D.OverlapBoxAll(_AttackTransform.position, _AttackArea, 0, WhatIsWall);
        List<Enemy> HitEnemies = new List<Enemy>();

        if (ObjectsToHit.Length > 0)
        {
            _RecoilDir = true;
        }
        
        // Check if we Cast
        if (pState.Casting)
        {
            Debug.Log("You are casting");
        }
        
        for (int i = 0; i < ObjectsToHit.Length; i++)
        {
          Enemy E = ObjectsToHit[i].GetComponent<Enemy>();
            if(E && !HitEnemies.Contains(E))
            {
                E.EnemyHit(Damage, (transform.position - ObjectsToHit[i].transform.position).normalized, _RecoilStrength);
                HitEnemies.Add(E);
                SpawnParticleEffect(Slash, ObjectsToHit[i].transform.position, 0);

                if (ObjectsToHit[i].CompareTag("Enemy"))
                {
                    mana += ManaGain;
                }
            }
        }

        if (FloorToHit.Length > 0)
        {
            if (Shockwave != null)
            {
                Vector2 closestPoint = FloorToHit[0].ClosestPoint(_AttackTransform.position);
                SpawnParticleEffect(Shockwave, closestPoint, 0);
            }
        }
        if (WallToHit.Length > 0)
        {
            Vector2 closestPoint = WallToHit[0].ClosestPoint(_AttackTransform.position);
            SpawnParticleEffect(Shockwave, closestPoint, 0);
        }
    }
    void SlashEffectAtAngle(GameObject _SlashEffect, int _EffectAngle, Transform _AttackTransform)
    {
        _SlashEffect = Instantiate(_SlashEffect, _AttackTransform);
        _SlashEffect.transform.eulerAngles = new Vector3(0, 0, _EffectAngle);
        _SlashEffect.transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y);
    }

    void Recoil()
    {
        if (pState.RecoilingX)
        {
            if (pState.LookingRight)
            {
                rb.linearVelocity = new Vector2(-RecoilXSpeed, 0);
            }
            else
            {
                rb.linearVelocity = new Vector2(RecoilXSpeed, 0);
            }
        }

        if (pState.RecoilingY)
        {
            rb.gravityScale = 0;
            if (yAxis < 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, RecoilYSpeed);
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -RecoilYSpeed);
            }
            AirJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        if(pState.RecoilingX && StepsXRecoiled < RecoilXSteps)
        {
            StepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }
        if (pState.RecoilingY && StepsYRecoiled < RecoilYSteps)
        {
            StepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }
        
        if(Grounded())
        {
            StopRecoilY();
        }
    }

    void StopRecoilX()
    { 
        StepsXRecoiled = 0;
        pState.RecoilingX = false;
    }
    void StopRecoilY()
    {
        StepsYRecoiled = 0;
        pState.RecoilingY = false;
    }

    public void TakeDamage(float _Damage)
    {
        Health -= Mathf.RoundToInt(_Damage);
        StartCoroutine(StopTakingDamage());
    }
    IEnumerator StopTakingDamage()
    {
        pState.Invincible = true;
        GameObject _bloodSpurtParticles = Instantiate(BloodSpurt, transform.position, Quaternion.identity);
        Destroy(_bloodSpurtParticles, 1.5f);
        anim.SetTrigger("TakeDamage");
        yield return new WaitForSeconds(1f);
        pState.Invincible = false;
    }
    void FlashWhileInvincible()
    {
        sr.material.color = pState.Invincible ?
            Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * HitFlashSpeed, 1.0f)) :
            Color.white;
    }
    void RestoreTimeScale()
    {         if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.deltaTime * restoreTimeSpeed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }
    public void HitStopTime(float _NewTimeScale, int _RestoreSpeed, float _delay)
        {
        restoreTimeSpeed = _RestoreSpeed;
        Time.timeScale = _NewTimeScale;

        if(_delay > 0)
        {
            StopCoroutine(StartTimeAgain(_delay));
            StartCoroutine(StartTimeAgain(_delay));
        }
        else
        {
            restoreTime = true;
        }
    }
    IEnumerator StartTimeAgain(float _delay)
    {
        restoreTime = true;
        yield return new WaitForSeconds(_delay);
    }

    public int Health
    {
        get { return health; }
        set
        {
            if(health != value)
            {
                health = Mathf.Clamp(value, 0, MaxHealth);

                if(onHealthChangedCallBack != null)
                {
                    onHealthChangedCallBack.Invoke();
                }
            }
        }
    }

    void Heal()
    {
        if (Input.GetButton("Healing") && Health < MaxHealth && mana > 0 && !pState.jumping && !pState.Dashing)
        {            
            pState.Healing = true;
            anim.SetBool("Healing", true);

            HealTimer += Time.deltaTime;
            if (HealTimer >= TimeToHeal)
            {
                Health ++;
                HealTimer = 0;
            }

            mana -= Time.deltaTime * ManaDrainSpeed;
        }
        else
        {
            pState.Healing = false;
            anim.SetBool("Healing", false);
            HealTimer = 0;
        }
    }

    float mana
    {
        get { return Mana; }
        set
        {
            if (Mana != value)
            {
                Mana = Mathf.Clamp(value, 0, 1);
                ManaStorage.fillAmount = mana;
            }
        }
    }

    void CastSpell()
    {
        // Check for both F key AND R2 button (joystick button 7)
        bool castInput = Input.GetKeyDown(KeyCode.F) || Input.GetButtonDown("CastSpell");

        if (castInput)
        {
            Debug.Log($"CAST PRESSED! Mana: {mana}/{ManaSpellCost}, Time: {TimeSinceCast}/{TimeBetweenCasts}");
        }

        if (castInput && TimeSinceCast >= TimeBetweenCasts && mana >= ManaSpellCost)
        {
            Debug.Log("SPELL CASTING!");
            pState.Casting = true;
            TimeSinceCast = 0;
            StartCoroutine(CastCoroutine());
        }
        else
        {
            TimeSinceCast += Time.deltaTime;
        }

        if (Grounded())
        {
            DownSpellFireball.SetActive(false);
        }

        if (DownSpellFireball.activeInHierarchy)
        {
            rb.linearVelocity += DownSpellForce * Vector2.down;
        }
    }

    IEnumerator CastCoroutine()
    {
        anim.SetBool("Casting", true);

        // Store yAxis and grounded state BEFORE the wait
        float storedYAxis = yAxis;
        bool storedGrounded = Grounded();

        yield return new WaitForSeconds(0.15f);  // <-- Changed from 0.5f to 0.15f (or even 0.05f for instant)

        if (storedYAxis < 0 && !storedGrounded)
        {
            // Cast Down Spell
            DownSpellFireball.SetActive(true);
        }
        else if (storedYAxis > 0)
        {
            // Cast Up Spell
            Instantiate(UpSpellExplosion, transform);
            rb.linearVelocity = Vector2.zero;
        }
        else if (storedYAxis == 0 || (storedYAxis < 0 && storedGrounded))
        {
            // Cast Side Spell
            GameObject _Fireball = Instantiate(SideSpellFireball, SideAttackTransform.position, Quaternion.identity);

            if (pState.LookingRight)
            {
                _Fireball.transform.rotation = Quaternion.identity;
            }
            else
            {
                _Fireball.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            pState.RecoilingX = true;
        }

        mana -= ManaSpellCost;
        yield return new WaitForSeconds(0.35f);
        anim.SetBool("Casting", false);
        pState.Casting = false;
    }

    public bool Grounded()
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, WhatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, WhatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, WhatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
        void Jump()
    {
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x,0);
            pState.jumping = false;
        }

        if (!pState.jumping)
        {
            if (JumpBufferCounter > 0 && CoyoteTimeCounter > 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
                pState.jumping = true;
            }
            else if(!Grounded() && AirJumpCounter < MaxAirJumps && Input.GetButtonDown("Jump"))
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
                pState.jumping = true;
                AirJumpCounter ++;
            }
        }

        anim.SetBool("Jumping", !Grounded());
    }
    void UpdateJumpVariables()
        {
        if (Grounded())
            {
            pState.jumping = false;
            CoyoteTimeCounter = CoyoteTime;
            AirJumpCounter = 0;
        }
        else
        {
            CoyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            JumpBufferCounter = JumpBufferFrames;
        }
        else
        {
            //JumpBufferCounter--;
            JumpBufferCounter = JumpBufferCounter - Time.deltaTime * 10;
        }
    }

}
