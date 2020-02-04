﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    // Public Properties
    public PlayerAnim animScript;
    public float maxRunSpeed = 5f;
    public float secondsToMaxSpeed = 0.2f;
    public float groundedSecondsToStop = 0.2f;
    public float airborneSecondsToStop = 1.0f;
    public float jumpVelocity = 15f;
    public int midairJumps = 1;
    public float gravity = 3f;
    public GameObject wandPrefab;
    // public float hookMultiplier = 0.2f;
    public float hookSpeed = 20f;
    public bool debug = false;
    public GameObject debugPrefab1;
    public GameObject debugPrefab2;

    // Private Properties
    private Rigidbody2D rigid;
    private CapsuleCollider2D capsuleCollider;
    private BoxCollider2D boxTrigger;
    private Vector2 lastVelocity;
    private Vector3 slopeAxis = Vector3.right;
    private float runTimeStart = 0f;
    private float stopTimeStart = 0f;
    private float decelTime = 0f;
    private float decelerationMultiplier = 0f;
    private float lastAxisX = 0f;
    private float lastAxisY = 0f;
    private float lastFire1 = 0f;
    private int jumpCounter = 0;
    private GameObject spawnedWand;
    private StaffTrigger wandScript;
    private Vector3 wandSpawnLocation;
    private bool grounded = false;

    /// <summary>
    /// Start()
    /// </summary>
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        boxTrigger = GetComponent<BoxCollider2D>();
    }

    /// <summary>
    /// Update()
    /// </summary>
    void Update()
    {
        // Getting the input values
        float axisX = Input.GetAxisRaw("Horizontal");
        float axisY = Input.GetAxisRaw("Vertical");
        float fire1 = Input.GetAxisRaw("Fire1");

        // IF there is new running input...
        if (axisX != 0f && axisX != lastAxisX)
        {
            runTimeStart = Time.time;
        }

        // IF there is new stopping input...
        if (axisX == 0f && axisX != lastAxisX)
        {
            stopTimeStart = Time.time;
        }

        // Calculating the deceleration multiplier
        decelTime = (grounded) ? groundedSecondsToStop : airborneSecondsToStop;
        decelerationMultiplier = Mathf.Clamp((stopTimeStart + decelTime - Time.time) / decelTime, 0f, 1f);

        // Running
        if (axisX != 0f)
        {
            // Calculating the add velocity factor
            float addVelocityX = axisX * maxRunSpeed * (Time.deltaTime / secondsToMaxSpeed);

            // Creating the move vector
            Vector2 moveVector = new Vector2();

            // IF the player is grounded, follow the slope axis, otherwise do NOT do that
            if (grounded)
                moveVector = new Vector2(slopeAxis.x * addVelocityX, slopeAxis.y * addVelocityX);
            else
                moveVector = new Vector2(addVelocityX, 0f);

            // IF the velocity and move vectors are different OR the new velocity does not exceed the max velocity...
            if (rigid.velocity.x / Mathf.Abs(rigid.velocity.x) != axisX || (rigid.velocity + moveVector).magnitude < maxRunSpeed)
            {
                rigid.velocity += moveVector;
            }
            // ELSE IF the current velocity is less than the max...
            else if (rigid.velocity.magnitude < maxRunSpeed)
            {
                rigid.velocity = rigid.velocity.normalized * maxRunSpeed;
            }
        }

        // IF the player is not holding down a button (but is still moving)...
        if (axisX == 0f && rigid.velocity.x != 0f)
        {
            // IF the player is grounded, decelerate along the slope axis
            if (grounded)
            {
                // Calculate the deceleration vector
                Vector2 decelVector = (Time.deltaTime / decelTime) * rigid.velocity.normalized * maxRunSpeed;

                // IF the deceleration vector is LESS than the current velocity...
                if (decelVector.magnitude < rigid.velocity.magnitude)
                    rigid.velocity -= decelVector;
                else
                    rigid.velocity = new Vector2(0f, 0f);
            }
            // ELSE... (decelerate exclusively on the X axis)
            else
            {
                // Calulate the deceleration vector X value
                float decelX = (Time.deltaTime / decelTime) * (rigid.velocity.x / Mathf.Abs(rigid.velocity.x)) * maxRunSpeed;

                // IF the deceleration vector X value is LESS than the current X velocity...
                if (Mathf.Abs(decelX) < Mathf.Abs(rigid.velocity.x))
                    rigid.velocity -= new Vector2(decelX, 0f);
                else
                    rigid.velocity = new Vector2(0f, rigid.velocity.y);
            }
        }

        // Jumping
        if (axisY > 0f && axisY != lastAxisY && (jumpCounter < midairJumps + 1))
        {
            rigid.velocity = new Vector2(rigid.velocity.x, jumpVelocity);
            grounded = false;
            jumpCounter++;
        }
        if (axisY == 0f && axisY != lastAxisY && !grounded && rigid.velocity.y > 0f)
        {
            rigid.velocity *= new Vector2(1f, 0.5f);
        }

        // Waving The Wand
        if (fire1 > 0f && fire1 != lastFire1)
        {
            // Spawning the Wand
            wandSpawnLocation = new Vector2(axisX, axisY);
            wandSpawnLocation.Normalize();
            spawnedWand = Instantiate(wandPrefab, transform.position + wandSpawnLocation, Quaternion.identity);
            wandScript = spawnedWand.GetComponent<StaffTrigger>();

            // Rotating the Wand
            float rotation = Vector3.SignedAngle(Vector3.right, wandSpawnLocation, Vector3.forward);
            spawnedWand.transform.Rotate(Vector3.forward, rotation);
        }
        if (fire1 == 0f && fire1 != lastFire1)
        {
            Destroy(spawnedWand, 0f);
        }

        // IF the wand has been spawned...
        if (spawnedWand != null)
        {
            // Moving the Wand to the right place
            spawnedWand.transform.position = transform.position + wandSpawnLocation;

            // IF the Wand has been hooked...
            if (wandScript.IsHooked())
            {
                Vector2 modVector = wandSpawnLocation * hookSpeed * Time.deltaTime * maxRunSpeed;
                rigid.velocity += modVector;

            }
        }

        // ANIMATING
        if (axisX > 0f)
            animScript.MoveRight();
        else if (axisX < 0f)
            animScript.MoveLeft();
        else
            animScript.StandStill();

        // Setting the "Last Run Time" variables
        lastAxisX = axisX;
        lastAxisY = axisY;
        lastFire1 = fire1;
        lastVelocity = rigid.velocity;
    }

    /// <summary>
    /// GroundPlayer()
    /// </summary>
    private void GroundPlayer(Vector3 closestPoint)
    {
        // Telling the player it has been grounded
        grounded = true;

        // Resetting the jump counter
        jumpCounter = 0;

        // Getting the new slope axis
        slopeAxis = transform.position - closestPoint;
        slopeAxis = new Vector3(slopeAxis.y, -slopeAxis.x, 0f);
        slopeAxis.Normalize();

        // Disable gravity on the rigidbody
        rigid.gravityScale = 0f;
    }

    /// <summary>
    /// OnTriggerEnter()
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter2D(Collider2D other)
    {
        // IF the other collider is a Platform...
        if (other.gameObject.tag == "Platform")
        {
            // Casting to the correct collider type
            BoxCollider2D otherBox = (BoxCollider2D) other;

            // Get the closest point to the player
            Vector3 closestPoint = otherBox.ClosestPoint(transform.position);

            // IF the closest point is on the ground...
            if (closestPoint.y <= transform.position.y - boxTrigger.bounds.extents.y / 2)
                GroundPlayer(closestPoint);

            if (debug)
            {
                if (grounded)
                {
                    Debug.Log("\t=== GROUNDED ===");
                    Instantiate(debugPrefab1, closestPoint, Quaternion.identity);
                }
                else
                {
                    Debug.Log("\t=== STILL IN AIR ===");
                    Instantiate(debugPrefab2, closestPoint, Quaternion.identity);
                }
            }
        }
    }

    /// <summary>
    /// OnTriggerStay()
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerStay2D(Collider2D other)
    {
        // IF the other collider is a Platform...
        if (other.gameObject.tag == "Platform")
        {
            // Casting to the correct collider type
            BoxCollider2D otherBox = (BoxCollider2D)other;

            // Get the closest point to the player
            Vector3 closestPoint = otherBox.ClosestPoint(transform.position);

            // IF the closest point is on the ground...
            if (closestPoint.y <= transform.position.y - boxTrigger.bounds.extents.y / 2)
            {
                GroundPlayer(closestPoint);
            }
        }
    }

    /// <summary>
    /// OnTriggerExit2D()
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit2D(Collider2D other)
    {
        // IF the other collider is a Platform...
        if (other.gameObject.tag == "Platform")
        {
            // Enable gravity
            rigid.gravityScale = gravity;
            grounded = false;
        }
    }
}
