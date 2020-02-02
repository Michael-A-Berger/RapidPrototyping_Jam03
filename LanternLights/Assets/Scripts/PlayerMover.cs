using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    // Public Properties
    public PlayerAnim animScript;
    public float speed = 5f;

    // Private Properties
    private Rigidbody2D rigidbody;
    private float lastAxisY = 0f;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Getting the input values
        float axisX = Input.GetAxisRaw("Horizontal");
        float axisY = Input.GetAxisRaw("Vertical");

        // Movement
        if (axisX > 0f)
        {
            rigidbody.AddForce(Vector2.right * Time.deltaTime * speed * 100f);
            animScript.MoveRight();
            // rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, 10f);
        }
        else if (axisX < 0f)
        {
            rigidbody.AddForce(Vector2.left * Time.deltaTime * speed * 100f);
            animScript.MoveLeft();
            // rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, 10f);
        }
        else
        {
            animScript.StandStill();
        }
        if (axisY > 0f && axisY != lastAxisY)
        {
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, 10f);
        }

        //
        lastAxisY = axisY;
    }
}
