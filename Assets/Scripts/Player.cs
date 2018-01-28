using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{

    [Header("Jumping")]
    [Tooltip("Set the maximum jump height for when holding down the button")]
    public float maxJumpHeight = 4;
    [Tooltip("Set the jump height for when you tap and release the jump button")]
    public float minJumpHeight = 1;
    [Tooltip("The time it takes to reach the maximum jump height")]
    public float timeToJumpApex = .4f;

    float m_accelerationTimeAirborne = .2f;
    float m_accelerationTimeGrounded = .1f;

    [Header("Movement")]
    [Tooltip("Movement Speed")]
    public float moveSpeed = 6;

   
    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;
    private Vector2 m_directionalInput;

    [Header("Wall jumping")]
    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;
    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;

    private float m_timeToWallUnstick;
    private bool m_wallSliding;
    int wallDirX;

    [Header("Refs")]
    Controller2D controller;

    void Start()
    {
        controller = GetComponent<Controller2D>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    void Update()
    {
        CalculateVelocity();
        HandleWallSliding();

        controller.Move(velocity * Time.deltaTime, m_directionalInput);

        if (controller.collisions.above || controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }
    }

    public void SetDirectionalInput(Vector2 input)
    {
        m_directionalInput = input;
    }

    public void OnJumpInputDown()
    {
        if (m_wallSliding)
        {
            if (wallDirX == m_directionalInput.x)
            {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (m_directionalInput.x == 0)
            {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else
            {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
        if (controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                if (m_directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x))
                { // not jumping against max slope
                    velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                velocity.y = maxJumpVelocity;
            }
        }
    }

    public void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }


    void HandleWallSliding()
    {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        m_wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            m_wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if (m_timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (m_directionalInput.x != wallDirX && m_directionalInput.x != 0)
                {
                    m_timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    m_timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                m_timeToWallUnstick = wallStickTime;
            }

        }

    }

    void CalculateVelocity()
    {
        float targetVelocityX = m_directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? m_accelerationTimeGrounded : m_accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }
}