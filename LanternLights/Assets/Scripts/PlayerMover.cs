using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    // Public Properties
    public PlayerAnim animScript;
    public float maxRunSpeed = 5f;
    public float secondsToMaxSpeed = 0.1f;
    public float groundedSecondsToStop = 0.1f;
    public float airborneSecondsToStop = 0.5f;
    public float jumpVelocity = 15f;
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
    private float accelerationMultiplier = 0f;
    private float decelerationMultiplier = 0f;
    private float lastAxisX = 0f;
    private float lastAxisY = 0f;
    private float startingGravity;
    private bool grounded = false;

    /// <summary>
    /// Start()
    /// </summary>
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        boxTrigger = GetComponent<BoxCollider2D>();
        startingGravity = rigid.gravityScale;
    }

    /// <summary>
    /// Update()
    /// </summary>
    void Update()
    {
        // Getting the input values
        float axisX = Input.GetAxisRaw("Horizontal");
        float axisY = Input.GetAxisRaw("Vertical");

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
        if (axisX != 0f && Mathf.Floor(rigid.velocity.x) != Mathf.Floor(maxRunSpeed * axisX))
        {
            // Calculating the movement vector
            float moveX = axisX * maxRunSpeed * (Time.deltaTime / secondsToMaxSpeed);

            // IF the player is grounded, follow the slope axis, otherwise do NOT do that
            if (grounded)
            {
                Vector2 moveVector = new Vector2(slopeAxis.x * moveX, slopeAxis.y * moveX);
                rigid.velocity += moveVector;
            }
            else
                rigid.velocity += new Vector2(moveX, 0f);
        }

        // IF the player is not holding down a button (but are still moving)...
        if (axisX == 0f && rigid.velocity.x != 0f)
        {
            // IF the player is grounded, decelerate along the slope axis
            if (grounded)
                rigid.velocity *= slopeAxis * decelerationMultiplier;
            else
                rigid.velocity *= new Vector2(decelerationMultiplier, 1f);
        }

        // Jumping
        if (axisY > 0f && axisY != lastAxisY && grounded)
        {
            rigid.velocity = new Vector2(rigid.velocity.x, jumpVelocity);
            grounded = false;
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
        lastVelocity = rigid.velocity;
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
            {
                // Telling the player it has been grounded
                grounded = true;

                // Getting the new slope axis
                slopeAxis = transform.position - closestPoint;
                slopeAxis = new Vector3(slopeAxis.y, -slopeAxis.x, 0f);
                slopeAxis.Normalize();

                // Disable gravity on the rigidbody
                rigid.gravityScale = 0f;
            }

            if (debug)
                if (grounded)
                    Instantiate(debugPrefab1, closestPoint, Quaternion.identity);
                else
                    Instantiate(debugPrefab2, closestPoint, Quaternion.identity);
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
            rigid.gravityScale = startingGravity;
            grounded = false;
        }
    }
}
