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
    public float    moveSpeed = 6;
    [Tooltip("Max angle you can walk on without sliding down")]
    public float    maxSlopeAngle = 80;

    private float   m_gravity;
    private float   m_maxJumpVelocity;
    private float   m_minJumpVelocity;
    private Vector3 m_velocity;
    private float   m_velocityXSmoothing;
    private Vector2 m_directionalInput;

    [Header("Wall jumping")]
    public Vector2  wallJumpClimb;
    public Vector2  wallJumpOff;
    public Vector2  wallLeap;
    public float    wallSlideSpeedMax = 3;
    [Tooltip("Time before detaching the player from the wall")]
    public float    wallStickTime = .25f;

    private float   m_timeToWallUnstick;
    private bool    m_wallSliding;
    private int     m_wallDirX;

    [Header("Refs")]
    Controller2D controller;

    void Start()
    {
        controller = GetComponent<Controller2D>();

        m_gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        m_maxJumpVelocity = Mathf.Abs(m_gravity) * timeToJumpApex;
        m_minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(m_gravity) * minJumpHeight);
    }

    void Update()
    {
        CalculateVelocity();
        HandleWallSliding();

        controller.Move(m_velocity * Time.deltaTime, m_directionalInput);

        if (controller.collisions.above || controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                m_velocity.y += controller.collisions.slopeNormal.y * -m_gravity * Time.deltaTime;
            }
            else
            {
                m_velocity.y = 0;
            }
        }
    }

    public void SetDirectionalInput(Vector2 input)
    {
        m_directionalInput = input;
    }
    /// <summary>
    /// Call when the jump input is pressed.
    /// </summary>
    public void OnJumpInputDown()
    {
        if (m_wallSliding)
        {
            if (m_wallDirX == m_directionalInput.x)
            {
                m_velocity.x = -m_wallDirX * wallJumpClimb.x;
                m_velocity.y = wallJumpClimb.y;
            }
            else if (m_directionalInput.x == 0)
            {
                m_velocity.x = -m_wallDirX * wallJumpOff.x;
                m_velocity.y = wallJumpOff.y;
            }
            else
            {
                m_velocity.x = -m_wallDirX * wallLeap.x;
                m_velocity.y = wallLeap.y;
            }
        }
        if (controller.collisions.below)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                if (m_directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x))
                { // not jumping against max slope
                    m_velocity.y = m_maxJumpVelocity * controller.collisions.slopeNormal.y;
                    m_velocity.x = m_maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            }
            else
            {
                m_velocity.y = m_maxJumpVelocity;
            }
        }
    }
    /// <summary>
    /// Call when the jump input is released.
    /// </summary>
    public void OnJumpInputUp()
    {
        if (m_velocity.y > m_minJumpVelocity)
        {
            m_velocity.y = m_minJumpVelocity;
        }
    }


    void HandleWallSliding()
    {
        m_wallDirX = (controller.collisions.left) ? -1 : 1;
        m_wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && m_velocity.y < 0)
        {
            m_wallSliding = true;

            if (m_velocity.y < -wallSlideSpeedMax)
            {
                m_velocity.y = -wallSlideSpeedMax;
            }

            if (m_timeToWallUnstick > 0)
            {
                m_velocityXSmoothing = 0;
                m_velocity.x = 0;

                if (m_directionalInput.x != m_wallDirX && m_directionalInput.x != 0)
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
    /// <summary>
    /// Start calculating the velocity while adding friction.
    /// </summary>
    void CalculateVelocity()
    {
        float targetVelocityX = m_directionalInput.x * moveSpeed;
        m_velocity.x = Mathf.SmoothDamp(m_velocity.x, targetVelocityX, ref m_velocityXSmoothing, (controller.collisions.below) ? m_accelerationTimeGrounded : m_accelerationTimeAirborne);
        m_velocity.y += m_gravity * Time.deltaTime;
    }
}