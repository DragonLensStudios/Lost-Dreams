using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAI : MonoBehaviour
{
    public float speed = 3f;
    public float catchUpSpeed = 6.5f;
    public float catchUpStartDistance = 5f;
    public float catchUpStopDistance = 3f;

    private bool movementEnable = true;
    private bool isCatchingUpToPlayer = false;
    private GameObject playerGameObject;
    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        playerGameObject = GameObject.FindGameObjectWithTag("Player");

        animator = GetComponent<Animator>();
        //Initialize destinationSetter
        destinationSetter = GetComponent<AIDestinationSetter>();
        //Initialize aiPath
        aiPath = GetComponent<AIPath>();
        //print warnings for nulls
        if (aiPath == null)
        {
            Debug.LogError("EnemyAI: No AIPath on GameObject: " + gameObject.name);
        }
        if (destinationSetter == null)
        {
            Debug.LogError("EnemyAI: No AIDestinationSetter on GameObject: " + gameObject.name);
            return;
        }
        if (animator == null)
        {
            Debug.LogError("EnemyAI: No Animator on GameObject: " + gameObject.name);
            return;
        }
        destinationSetter.target = playerGameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (movementEnable)
        {
            UpdateMoveSpeed();
            UpdateAnimator();
        }
    }

    //update the movespeed if dog should catchup or stop catching up
    private void UpdateMoveSpeed()
    {
        if (!isCatchingUpToPlayer)
        {
            if (GetDistanceToPlayer() > catchUpStartDistance)
            {
                isCatchingUpToPlayer = true;
                animator.SetBool("IsCatchingUp", true);
                aiPath.maxSpeed = catchUpSpeed;
            }
        }
        else
        {
            if (GetDistanceToPlayer() < catchUpStopDistance)
            {
                isCatchingUpToPlayer = false;
                animator.SetBool("IsCatchingUp", false);
                aiPath.maxSpeed = speed;
            }
        }
    }

    //update the animator
    private void UpdateAnimator()
    {
        Vector2 Direction = GetDirectionFacing();
        animator.SetFloat("MoveX", Direction.x);
        animator.SetFloat("MoveY", Direction.y);
        animator.SetBool("IsMoving", GetIsMoving());
    }

    //returns true if moving
    private bool GetIsMoving()
    {
        return !aiPath.reachedDestination;
    }

    //return direction dog is facing
    private Vector2 GetDirectionFacing()
    {
        //get direction from 4 cardinal directions
        Vector2 direction = aiPath.desiredVelocity.normalized;
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
    //returns distance to player
    private float GetDistanceToPlayer()
    {
        if (playerGameObject == null) return float.NaN;
        // get normalized direction of player
        Vector2 playerDirection = playerGameObject.transform.position - gameObject.transform.position;
        return playerDirection.magnitude;
    }
}
