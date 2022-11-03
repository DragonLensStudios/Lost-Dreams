using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    enum EnemyAIState { Idle, AttackingPlayer, Stunned, Dead }
    [Header("Aggro Settings")]
    [Tooltip("Distance the Player will aggro the enemy")]
    public float aggroRange = 3f;
    [Tooltip("Distance the Enemy will lose aggro and return to wandering")]
    public float deAggroRange = 4f;
    [Space(5)]

    [Header("Wander Settings")]
    [Tooltip("The range from the Enemies spawn that he may set a wander target within")]
    public float wanderRadius = 5f;
    [Tooltip("The minimum time the enemy will wait once reaching it's wander destination")]
    public float wanderMinimumIdleTime = 1f;
    [Tooltip("The maximum time the enemy will wait once reaching it's wander destination")]
    public float wanderMaximumIdleTime = 2f;
    [Space(5)]

    [Header("Knockback Settings")]
    public float damageStunTime = .5f;
    [Space(5)]

    [Header("Obstacles")]
    public LayerMask ObstacleLayerMask;

    [Header("Movement")]
    public bool MovementLocked = false;
    private bool movementEnable = true;

    private EnemyAIState currentState = EnemyAIState.Idle;
    private Coroutine stunnedCoroutine;
    private Coroutine wanderCoroutine;

    private Vector2 spawnLocation;
    private GameObject wanderTargetObject;
    private bool waitingForNewWanderTarget = false;


    private GameObject playerGameObject;
    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;
    private Rigidbody2D rb;
    protected Animator animator;
    private EnemyBaseAttackComponent attackComponent;
    
    protected virtual void Awake()
    {
        //Initialize aiPath
        aiPath = GetComponent<AIPath>();
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {
        //Initialize Rigidbody
        rb = GetComponent<Rigidbody2D>();
        //Initialize animator
        animator = GetComponent<Animator>();
        //Initialize PlayerGameObject
        playerGameObject = GameObject.FindGameObjectWithTag("Player");
        //Initialize SpawnLocation
        spawnLocation = gameObject.transform.position;
        //Initialize targetObject
        wanderTargetObject = new GameObject(gameObject.name + "_" + "TargetObject");
        wanderTargetObject.transform.position = spawnLocation;
        //Initialize destinationSetter
        destinationSetter = GetComponent<AIDestinationSetter>();
        //Initialize Attack Component
        attackComponent = GetComponent<EnemyBaseAttackComponent>();


        //print warnings for nulls
        if (attackComponent == null)
        {
            Debug.LogError("EnemyAI: No attackComponent on GameObject: " + gameObject.name);
        }
        if (aiPath == null)
        {
            Debug.LogError("EnemyAI: No AIPath on GameObject: " + gameObject.name);
        }
        if (destinationSetter == null)
        {
            Debug.LogError("EnemyAI: No AIDestinationSetter on GameObject: " + gameObject.name);
            return;
        }
        destinationSetter.target = wanderTargetObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (movementEnable)
        {
            //Pre Action
            //Check if should switch state
            switch (currentState)
            {
                case EnemyAIState.Idle:
                    if (ShouldAggroToPlayer())
                    {
                        if (wanderCoroutine != null) StopCoroutine(wanderCoroutine);
                        SetCanEnemyMove(true);
                        currentState = EnemyAIState.AttackingPlayer;
                        destinationSetter.target = playerGameObject.transform;
                    }
                    break;
                case EnemyAIState.AttackingPlayer:
                    if (GetDistanceToPlayer() > deAggroRange)//if player has gone further than deaggro range
                    {
                        currentState = EnemyAIState.Idle;
                        destinationSetter.target = wanderTargetObject.transform;
                    }
                    break;
                default:
                    break;
            }
            //Action
            //Take action based on state
            switch (currentState)
            {
                case EnemyAIState.Idle:
                    Wander();
                    break;
                case EnemyAIState.AttackingPlayer:
                    attackComponent.TryPerformAttack();
                    break;
                default:
                    break;
            }
            UpdateAnimator();
        }

    }
    private void OnEnable()
    {
        EventManager.onObjectDied += OnObjectDied;
        EventManager.onDamageActor += OnDamageActor;
        EventManager.onPause += EventManager_onPause;

    }

    private void EventManager_onPause(bool value)
    {
        movementEnable = !value;
    }

    private void OnDisable()
    {
        EventManager.onObjectDied -= OnObjectDied;
        EventManager.onDamageActor -= OnDamageActor;
        EventManager.onPause += EventManager_onPause;
    }

    //Gets the direction the character is facing(Up, Down, Left, Right)
    public Vector2 GetDirectionFacing()
    {
        Vector2 direction = Vector2.zero;
        //get direction from 4 cardinal directions
        switch (currentState)
        {
            case EnemyAIState.Idle:
                direction = aiPath.desiredVelocity.normalized;
                break;
            case EnemyAIState.AttackingPlayer:
                direction = playerGameObject.transform.position - gameObject.transform.position;
                break;
            case EnemyAIState.Stunned:
                direction = playerGameObject.transform.position - gameObject.transform.position;
                break;
            default:
                break;
        }
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
        return direction;
    }

    public bool GetCanEnemyMove()
    {
        return aiPath.canMove;
    }

    public void SetCanEnemyMove(bool bCanEnemyMove) 
    {
        if(currentState == EnemyAIState.Dead || MovementLocked)
        {
            aiPath.canMove = false;
            return;
        }
        aiPath.canMove = bCanEnemyMove;
    }

    private void OnObjectDied(GameObject objectThatDied)
    {
        if(objectThatDied == gameObject)
        {
            currentState = EnemyAIState.Dead;
            attackComponent.StopAttack();
            if(stunnedCoroutine != null)StopCoroutine(stunnedCoroutine);
            if(wanderCoroutine != null)StopCoroutine(wanderCoroutine);
            SetCanEnemyMove(false);
            rb.velocity = Vector2.zero;
            Collider2D collider = gameObject.GetComponent<Collider2D>();
            aiPath.enabled = false;
            if (collider != null)
            {
                collider.enabled = false;
            }
        }
    }

    private void OnDamageActor(GameObject _target, GameObject _attacker, int _dmg, float _knockback)
    {
        if(_target == gameObject)
        {
            if (currentState == EnemyAIState.Dead) return;
            Vector2 direction = (transform.position-_attacker.transform.position).normalized;
            stunnedCoroutine = StartCoroutine(Stunned());
            rb.AddForce(direction * _knockback);
        }
    }

    //update the animator
    private void UpdateAnimator()
    {
        Vector2 Direction = GetDirectionFacing();
        animator.SetFloat("MoveX", Direction.x);
        animator.SetFloat("MoveY", Direction.y);
        animator.SetFloat("LastMoveX", Direction.x);
        animator.SetFloat("LastMoveY", Direction.y);
        animator.SetBool("isMoving", GetIsMoving());
    }

    //returns true if moving
    private bool GetIsMoving()
    {
        return !aiPath.reachedDestination  && GetCanEnemyMove();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if collided with other Enemy while idle, wait and find a new wander location
        //TODO: This is stupid and I hate it. There's a better way. -the man who wrote the code
        if (currentState == EnemyAIState.Idle && collision.gameObject.tag == "Enemy")
        {
            wanderCoroutine = StartCoroutine(WaitThenSetNewWanderLocation());
        }
    }

    //If AI has reached the end of its current wandering path, wait for a new one
    private void Wander()
    {
        if (aiPath.reachedEndOfPath && !waitingForNewWanderTarget)
        {
            waitingForNewWanderTarget = true;
            StartCoroutine(WaitThenSetNewWanderLocation());
        }
    }

    //If player is in Line of Sight and distance is less than aggro range
    private bool ShouldAggroToPlayer()
    {
        if (GetDistanceToPlayer() < aggroRange && PlayerInLOS())
            return true;
        else
            return false;
    }

    //returns distance to player
    private float GetDistanceToPlayer()
    {
        if (playerGameObject == null) return float.NaN;
        // get normalized direction of player
        Vector2 playerDirection = playerGameObject.transform.position - gameObject.transform.position;
        return playerDirection.magnitude;
    }

    //returns true if there is nothing blocking a raycast between player and enemy on ObstacleLayerMask
    private bool PlayerInLOS()
    {
        //return true if no player
        if (playerGameObject == null) return false;
        Vector2 playerDirection = playerGameObject.transform.position - gameObject.transform.position;
        float playerDistance = playerDirection.magnitude;
        playerDirection.Normalize();

        //raycast in the player direction to the distance of the player looking for obstacles
        RaycastHit2D hit = Physics2D.Raycast(gameObject.transform.position, playerDirection, playerDistance, ObstacleLayerMask);
        if (hit.collider != null) // if we hit anything LOS is blocked
        {
            return false;
        }
        //otherwise
        return true;
    }

   private IEnumerator Stunned()
    {
        attackComponent.StopAttack();
        SetCanEnemyMove(false);
        currentState = EnemyAIState.Stunned;
        yield return new WaitForSeconds(damageStunTime);
        currentState = EnemyAIState.Idle;
        SetCanEnemyMove(true);
    }
    //waits then sets a new Wander Target
    private IEnumerator WaitThenSetNewWanderLocation()
    {
        rb.velocity = Vector3.zero;
        SetCanEnemyMove(false);
        float idleTime = Random.Range(wanderMinimumIdleTime, wanderMaximumIdleTime);
        yield return new WaitForSeconds(idleTime);
        wanderTargetObject.transform.position = spawnLocation + (Random.insideUnitCircle * wanderRadius);
        //Needed to wait for aiPath to realize it's no longer at end of path, or this method would get called again
        //TODO: find a fix for this. Easiest methord would be to force aiPath to recalculate, but its only available in the paid version
        yield return new WaitForSeconds(.5f);
        waitingForNewWanderTarget = false;
        SetCanEnemyMove(true);
    }
}
