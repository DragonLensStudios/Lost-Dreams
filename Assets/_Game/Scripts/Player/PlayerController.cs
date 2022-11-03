using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public InventoryObject inventory;
    [Header("Combat")]
    public int attackDamage = 1;
    public float attackAnimationLength = .3f;
    public float attackCooldown = 1f;
    public float attackBoxDistance = 1f;
    public Vector2 attackBoxSize = new Vector2(1, 1);
    public AudioClip attackSound;

    [Header("Items")]
    public float foodCooldown = 0.5f;
    private bool isOnFoodCoooldown;

    [Tooltip("The offset of the origin of the hitbox")]
    public Vector2 boxOriginOffset = new Vector2(0, 0);
    public float knockback = 3f;
    public bool canAttack = true;
    [Header("Mud")]
    public Tilemap ground;
    public Tile badMud;
    public float mudSlowness;
    
    // movement variable
    [Header("Movement customization")]
    [Tooltip("allow player to move or not, usefull for cutscene")]
    public bool movementEnable;
    [Tooltip("maximum speed when normaly walking")]
    [Range(0, 100)]
    public float maxWalkingSpeed;
    [Tooltip("maximum speed when running")]
    [Range(0, 100)]
    public float maxRunSpeed;
    [Tooltip("time needed before getting to full speed(in second)")]
    [Range(0.1f, 1f)]
    public float accelerationTime;
    [Tooltip("time needed before the player stoped(in second)")]
    [Range(0f, 1f)]
    public float deaccelerationTime;
    [Tooltip("maximum time delay between the first move button press and the second one to triger running")]
    [Range(0.1f, 1)]
    public float doubleTapForRunningCooldown;
    [Tooltip("joystick run trigger range 1 being the max and 0.1 being the min.")]
    [Range(0.1f, 1)]
    public float joystickRunningTriggerDistance = 0.8f;


    [HideInInspector]
    public Rigidbody2D body;
    [HideInInspector]
    public HealthComponent health;

    [SerializeField]
    private Animator playerAnim;
    // private movement variable
    private Joystick joystick;
    private Vector2 input;
    private Vector2 acceleration;
    private Vector2 accelerationMult;
    private Vector2 movement = Vector2.zero;
    private Vector2 directionFacing = Vector2.up;
    private bool[] moveBefore = new bool[2];
    private float maxSpeed;
    [HideInInspector]
    public bool running = false;
    private Vector2 runningTimer = Vector2.zero;
    private Vector2 runningDirections = Vector2.zero;
    private Vector2 lastInput = Vector2.zero;
    private Vector2 lastMove = new Vector2(0, -1);
    private Vector2 DirectionFacing = Vector2.down;
    // combat variable
    private bool currentlyAttacking = false;
    private bool attackOnCooldown = false;
    private AudioSource audioSource;
    private bool onTheMud = false;

    // subcribe to event manager and stuff
    private void OnEnable()
    {
        EventManager.onItemUse += EventManager_onItemUse;
        EventManager.onFoodEat += EventManager_onFoodEat;
        EventManager.onHeartContainerIncrease += EventManager_onHeartContainerIncrease;
        EventManager.onControlsEnabled += EventManager_onControlsEnabled;
        EventManager.onSavePlayerInventory += EventManager_onSavePlayerInventory;
        EventManager.onLoadPlayerInventory += EventManager_onLoadPlayerInventory;
        EventManager.onAttack += EventManager_onAttack;
        EventManager.onPause += EventManager_onPause;
    }

    private void EventManager_onPause(bool value)
    {
        movementEnable = !value;
    }

    private void OnDisable()
    {
        EventManager.onItemUse -= EventManager_onItemUse;
        EventManager.onFoodEat -= EventManager_onFoodEat;
        EventManager.onHeartContainerIncrease -= EventManager_onHeartContainerIncrease;
        EventManager.onControlsEnabled -= EventManager_onControlsEnabled;
        EventManager.onSavePlayerInventory -= EventManager_onSavePlayerInventory;
        EventManager.onLoadPlayerInventory -= EventManager_onLoadPlayerInventory;
        EventManager.onAttack -= EventManager_onAttack;
        EventManager.onPause -= EventManager_onPause;
    }

    // Start is called before the first frame update
    void Awake()
    {
        //setup all the references variable
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<HealthComponent>();
        EventManager.InventoryChanged(inventory);
        audioSource = GetComponent<AudioSource>();
#if UNITY_ANDROID || UNITY_IOS
        joystick = GameObject.FindObjectOfType<Joystick>();
#endif
    }

    // Update is called once per frame
    void Update()
    {
        // if attack

#if UNITY_ANDROID || UNITY_IOS
            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began && !currentlyAttacking && !attackOnCooldown && movementEnable && canAttack)
            {
                if (!EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
                {
                    Attack();
                }
            }
#else
        if (Input.GetAxisRaw("Fire1") == 1 && !currentlyAttacking && !attackOnCooldown && movementEnable && canAttack)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Attack();
            }
        }
#endif


        Mud();

        if (!isOnFoodCoooldown)
        {
            if (Input.GetAxisRaw("Eat Horizontal") == -1 || Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (inventory.container.Count > 0)
                {
                    inventory.container[0].item.Use(this.gameObject);
                    StartCoroutine(EatCooldown());
                }
            }
            if (Input.GetAxisRaw("Eat Vertical") == 1 || Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (inventory.container.Count > 1)
                {
                    inventory.container[1].item.Use(this.gameObject);
                    StartCoroutine(EatCooldown());
                }
            }
            if (Input.GetAxisRaw("Eat Horizontal") == 1 || Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (inventory.container.Count > 2)
                {
                    inventory.container[2].item.Use(this.gameObject);
                    StartCoroutine(EatCooldown());
                }
            }
            if (Input.GetAxisRaw("Eat Vertical") == -1 || Input.GetKeyDown(KeyCode.Alpha4))
            {
                if (inventory.container.Count > 3)
                {
                    inventory.container[3].item.Use(this.gameObject);
                    StartCoroutine(EatCooldown());
                }
            }
        }
    }
    private void FixedUpdate()
    {
        GetInput();
        if (movementEnable && !currentlyAttacking)
        {
            Movement();
        }
        else
            // just making the player idling
            playerAnim.SetBool("isMoving", false);
    }

    private void OnApplicationQuit()
    {
        inventory.container.Clear();
        inventory.Save();
    }
    private void GetInput()
    {
        // getting input for movement
#if UNITY_ANDROID || UNITY_IOS
        if (joystick != null)
        {
            input.x = joystick.Horizontal;
            input.y = joystick.Vertical;
            if(input.x > joystickRunningTriggerDistance || input.y > joystickRunningTriggerDistance || input.x < -joystickRunningTriggerDistance || input.y < -joystickRunningTriggerDistance)
            {
                running = true;
            }
            else
            {
                running = false;
            }
        }
#else
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
#endif

        // check if running
        if (lastInput != input && input != Vector2.zero)
        {
            if (runningDirections == input)
                running = true;
            else
                for (int i = 0; i < 2; i++)
                    runningTimer[i] = doubleTapForRunningCooldown;
            runningDirections = input;
        }
        if(Input.GetAxisRaw("Fire2") == 1)
        {
            running = true;
        }

        for (int i = 0; i < 2; i++)
        {
            if (runningTimer[i] <= 0)
            {
                runningTimer[i] = 0;
                runningDirections[i] = 0;
            }
            else
                runningTimer[i] -= Time.deltaTime;
        }

        if (input == Vector2.zero)
            running = false;

        lastInput = input;
    }
    private void Movement()
    {
        // movement script, no need to understand it :I, but if you need dm me maybe? @Shaladdin
        maxSpeed = (running && !onTheMud) ? maxRunSpeed : maxWalkingSpeed;
        if(onTheMud)
        maxSpeed -= mudSlowness;
        for (int i = 0; i < 2; i++)
        {
            if (input[i] != 0)
            {
                acceleration[i] += Time.deltaTime;
                if (acceleration[i] > accelerationTime)
                    acceleration[i] = accelerationTime;
                moveBefore[i] = true;
            }
            else
            {
                if (moveBefore[i])
                {
                    moveBefore[i] = false;
                    acceleration[i] = deaccelerationTime;
                }
                else
                    acceleration[i] -= Time.deltaTime;
                if (acceleration[i] < 0)
                    acceleration[i] = 0;
            }
        }

        accelerationMult = Common.map(acceleration, 0, accelerationTime, 0, 1);

        for (int i = 0; i < 2; i++)
        {
            if (input[i] != 0)
            {
                movement[i] = input[i] * maxSpeed * accelerationMult[i];
                directionFacing[i] = input[i];
            }
            else
                movement[i] = maxSpeed * accelerationMult[i] * directionFacing[i];
        }

        // animation manager
        Vector2 newInput;

#if UNITY_ANDROID || UNITY_IOS
            newInput = new Vector2(joystick.Horizontal, joystick.Vertical);
#else
        newInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        if (newInput != Vector2.zero)
        {
            DirectionFacing = newInput;
        }
        playerAnim.SetFloat("MoveX", newInput.x);
        playerAnim.SetFloat("MoveY", newInput.y);
        if (input != Vector2.zero)
        {
            playerAnim.SetBool("isMoving", true);
            lastMove = newInput;
            playerAnim.SetFloat("LastMoveX", lastMove.x);
            playerAnim.SetFloat("LastMoveY", lastMove.y);
        }
        else
            playerAnim.SetBool("isMoving", false);

        body.MovePosition(new Vector2(transform.position.x, transform.position.y) + (movement * Time.deltaTime));
        body.velocity = Vector2.zero;
    }

    public void Attack()
    {
        StartCoroutine(AttackCooldown());
        audioSource.PlayOneShot(attackSound);
        StartCoroutine(StopMovingAndPerformAttackAnimation());
        DamageEnemies();
    }

    private void DamageEnemies()
    {
        Vector2 direction = DirectionFacing;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            direction = new Vector2(direction.x, 0);
            direction.Normalize();
        }
        else
        {
            direction = new Vector2(0, direction.y);
            direction.Normalize();
        }
        //boxcast in the direction
        Vector2 attackOrigin = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y) + boxOriginOffset+ direction * attackBoxDistance;
        float rotation;
        if (Mathf.Abs(DirectionFacing.x) == 1f && Mathf.Abs(DirectionFacing.y) != 1f) { rotation = 90f; } else { rotation = 0f; }

        RaycastHit2D[] hits = Physics2D.BoxCastAll(attackOrigin, attackBoxSize, rotation, new Vector2(0, 0));

        foreach (RaycastHit2D hit in hits)
        {
            if ((hit.collider != null) ? hit.collider.gameObject.tag == "Enemy" : false)
            {
                EventManager.DamageActor(hit.collider.gameObject, gameObject, attackDamage, knockback);
                Debug.Log(gameObject.name + " Hit Enemy!");
            }
        }
    }

    private IEnumerator StopMovingAndPerformAttackAnimation()
    {
        currentlyAttacking = true;
        playerAnim.SetBool("isAttacking", true);
        yield return new WaitForSeconds(attackAnimationLength);
        playerAnim.SetBool("isAttacking", false);
        currentlyAttacking = false;
    }

    private IEnumerator AttackCooldown()
    {
        attackOnCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        attackOnCooldown = false;
    }

    private IEnumerator EatCooldown()
    {
        isOnFoodCoooldown = true;
        yield return new WaitForSeconds(foodCooldown);
        isOnFoodCoooldown = false;
    }

    private void Mud()
    {
        if (ground != null)
        {
            Vector3Int position = ground.WorldToCell(transform.position);
            if (ground.GetTile(position))
            {
                if (ground.GetTile<Tile>(position) == badMud)
                {
                    onTheMud = true;
                    return;
                }
            }
            onTheMud = false;
        }
    }

    //Draw the attack square
    private void OnDrawGizmos()
    {
        //get Attack direction from 4 cardinal directions
        Vector2 direction = DirectionFacing;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            direction = new Vector2(direction.x, 0);
            direction.Normalize();
        }
        else
        {
            direction = new Vector2(0, direction.y);
            direction.Normalize();
        }
        //boxcast in the direction
        Vector2 attackOrigin = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y) + boxOriginOffset + direction * attackBoxDistance;

        Vector3 Cube = new Vector3(attackBoxSize.x, attackBoxSize.y, 1);
        if (Mathf.Abs(DirectionFacing.x) == 1f && Mathf.Abs(DirectionFacing.y) != 1f) { Cube = Quaternion.Euler(0, 0, 90) * Cube; }
        //Draw Attack 
        Gizmos.DrawWireCube(new Vector3(attackOrigin.x, attackOrigin.y), Cube);
    }

    public void EnableMovement(bool value)
    {
        movementEnable = value;
    }

    private void EventManager_onSavePlayerInventory()
    {
        inventory.Save();
    }

    private void EventManager_onLoadPlayerInventory()
    {
        inventory.Load();
    }

    private void EventManager_onControlsEnabled(bool value)
    {
        movementEnable = value;
    }

    private void EventManager_onItemUse(ItemObject _item, GameObject _obj)
    {
        if (_obj == this.gameObject)
        {
            inventory.RemoveItem(_item);
        }
    }

    private void EventManager_onFoodEat(FoodObject _food, GameObject _obj)
    {
        if (_obj == this.gameObject && health.health != health.maxHealth)
        {
            EventManager.HealthAdd(gameObject, _food.healAmount);
            inventory.RemoveItem(_food);
        }
    }

    private void EventManager_onAttack()
    {
        if (!currentlyAttacking && !attackOnCooldown && movementEnable && canAttack)
        {
            Attack();
        }
    }

    private void EventManager_onHeartContainerIncrease(HeartContainerObject _container, GameObject _obj)
    {
        inventory.RemoveItem(_container);
    }
}