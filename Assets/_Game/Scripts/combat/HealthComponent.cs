using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public int health;
    public bool flashColorOnHit;
    public Color flashColor;
    public int flashAmount;
    public float healthDamageFlashSpeed;
    public float destroyTime = 1.5f;
    [HideInInspector]
    public Rigidbody2D body;
    public float dieTimer = 1;
    private bool dying = false;
    private SpriteRenderer spriterender;

    // subcribe to ondamage
    private void OnEnable()
    {
        EventManager.onDamageActor += TakeDamage;
        EventManager.onObjectDied += Die;
    }
    // unsubcribe 
    private void OnDisable()
    {
        EventManager.onDamageActor -= TakeDamage;
        EventManager.onObjectDied -= Die;
    }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        spriterender = GetComponent<SpriteRenderer>();
    }

    void TakeDamage(GameObject _target, GameObject _attacker, int _damage, float _knockback)
    {
        if (_target == this.gameObject)
        {
            StartCoroutine(FlashDamage());
            health -= _damage;
            // knockback
            Vector3 diff = transform.position - _attacker.transform.position;
            body.velocity += (new Vector2(diff.x, diff.y).normalized * _knockback);
            // if die
            if (health - _damage <= 0)
                // call an event for loot drop or something
                EventManager.ObjectDied(gameObject);
        }
    }

    void Die(GameObject _obj)
    {
        if (_obj == this.gameObject)
        {
            var anim = _obj.GetComponent<Animator>();
            if(anim != null)
            {
                anim.SetTrigger("isDead");
                if(_obj.tag == "Player")
                {
                    EventManager.ControlsEnabled(false);
                }
            }
            // destroy gameObject
            Destroy(gameObject, destroyTime);
        }
    }

    void Update()
    {
        if (dying)
        {
            dieTimer -= Time.deltaTime;
            if (dieTimer <= 0)
                EventManager.ObjectDied(gameObject);
        }
    }

    IEnumerator FlashDamage()
    {
        if (flashColorOnHit)
        {
            if (spriterender != null)
            {
                for (int i = 0; i < flashAmount; i++)
                {
                    spriterender.color = flashColor;
                    yield return new WaitForSeconds(healthDamageFlashSpeed);
                    spriterender.color = Color.white;
                    yield return new WaitForSeconds(healthDamageFlashSpeed);
                }

            }
        }
        
    }
}
