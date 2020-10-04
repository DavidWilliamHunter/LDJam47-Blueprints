using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2D : MonoBehaviour
{
    public float maxSpeed = 9.0f;
    public float runAcc = 10.0f;
    public float breakAcc = 1000.0f;
    public float flyingAcc = 5.0f;
    public float maxJumpHeight = 10.0f;
    public float jumpAccumSpeed = 10.0f;

    protected Vector2 targetHorizontalVelocity;
    protected float acc = 1.0f;

    public float jumpAccumAmount = 0.0f;
    public enum JumpStatus { NotJumping, Preparing, Launching, Jumping, Falling };
    public JumpStatus jumpStatus = JumpStatus.NotJumping;
    public float LaunchReset = 0.0f;

    protected bool isOnGround = true;
    protected Collider2D mainCollider;
    protected Vector3 groundCheckPos;
    // Check every collider except Player and Ignore Raycast (TODO make this relate to interactable platforms
    LayerMask groundLayerMask = 1 << 8; // ~(1 << 2 | 1 << 8);

    public bool PlayerControlled = false;

    public bool WillHold = true;        // is the character set to hold on if possible;
    public bool IsHolding = false;       // is the character holding onto something right now.
    public bool IsReleasing = false;    // is the character letting go, right now?

    public float MaxHoldForce = 100.0f; // Maximum force before the character lets go.
    public Vector2 ropeForce = Vector2.zero;

    protected Rigidbody2D rb;

    public CharacterController2D nextCharacter;

    protected CameraFollow cameraFollow;

    public bool MaxRope = false;

    public Vector2 holdPos = Vector2.zero;

    protected AudioController audioController;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Assert.IsNotNull(rb);

        mainCollider = GetComponent<Collider2D>();
        Bounds colliderBounds = mainCollider.bounds;
        groundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * .5f, 0.1f, 0.0f);
        cameraFollow = GetComponent<CameraFollow>();
        cameraFollow.enabled = PlayerControlled;

        audioController = GetComponent<AudioController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerControlled)
        {
            float hMove = Input.GetAxis("Horizontal");
            float vMove = Input.GetAxis("Vertical");

            if (isOnGround)
            {
                targetHorizontalVelocity = new Vector2(hMove, 0.0f) * maxSpeed;
                acc = (Mathf.Abs(hMove) > 0.001f) ? runAcc : breakAcc;
            }
            else
            {
                targetHorizontalVelocity = new Vector2(hMove, 0.0f) * maxSpeed;

                acc = flyingAcc;
            }


            bool isBeginJump = Input.GetButtonDown("Jump");
            bool isEndJump = Input.GetButtonUp("Jump");

            if(isEndJump && IsHolding)
            {
                IsReleasing = true;
            }

            switch (jumpStatus)
            {
                case JumpStatus.NotJumping:
                    if (isBeginJump)
                    {
                        jumpStatus = JumpStatus.Preparing;
                        jumpAccumAmount = 0.0f;
                    }
                    break;
                case JumpStatus.Preparing:
                    {
                        jumpAccumAmount += jumpAccumSpeed * Time.deltaTime;
                        jumpAccumAmount = jumpAccumAmount > maxJumpHeight ? maxJumpHeight : jumpAccumAmount;
                        if (isEndJump)
                        {
                            doJump();
                            jumpStatus = JumpStatus.Launching;
                            LaunchReset = Time.time + 0.2f;
                        }
                    }
                    break;
                case JumpStatus.Launching:
                    if (!isOnGround)
                        jumpStatus = JumpStatus.Jumping;
                    if (Time.time > LaunchReset)
                        jumpStatus = JumpStatus.NotJumping;
                    break;
                case JumpStatus.Jumping:
                    if (rb.velocity.y >= 0.0f)
                        jumpStatus = JumpStatus.Falling;
                    if (isOnGround)
                        jumpStatus = JumpStatus.NotJumping;
                    break;
                case JumpStatus.Falling:
                    if (rb.velocity.y >= 0.0f)
                        jumpStatus = JumpStatus.Jumping;
                    if (isOnGround)
                        jumpStatus = JumpStatus.NotJumping;
                    break;
            }

            if (Input.GetButtonDown("Fire1") || Input.GetKeyDown("q"))
            {
                if (IsHolding)
                {
                    IsReleasing = true;
                }
                else if (WillHold)
                {
                    WillHold = false;
                }
                else
                    WillHold = true;
            }

            if(Input.GetButtonDown("Fire3") || Input.GetKeyDown("e"))
            {
                StartCoroutine(SwapPlayerCharacter());
            }
        }
    }

    private void FixedUpdate()
    {
        /*if(Mathf.Abs(rb.velocity.x)>maxSpeed)
        {
            float breaks = Mathf.Abs(rb.velocity.x) - maxSpeed; // the amount to slow down by
            breaks *= rb.velocity.x > 0 ? -1.0f : 1.0f;   // the direction
            rb.AddForce(new Vector2(breaks, 0.0f), ForceMode2D.Impulse);
        } */

        // Perform Ground check
        Bounds colliderBounds = mainCollider.bounds;
        Vector3 localGroundCheckPos = colliderBounds.min + new Vector3(colliderBounds.size.x * 0.5f, 0.1f, 0);
        isOnGround = Physics2D.OverlapCircle(localGroundCheckPos, 0.23f, groundLayerMask);

        if (PlayerControlled)
        {
            if (!IsHolding)     // only move around if player controlled
            {
                if(isOnGround)
                {
                    Vector2 VelocityDifference = targetHorizontalVelocity - rb.velocity;
                    VelocityDifference.y = 0.0f;        // not interested in fall speed
                    float remainingDifferenceSQR = VelocityDifference.sqrMagnitude;
                    VelocityDifference.Normalize();
                    VelocityDifference *= acc;
                    if (VelocityDifference.sqrMagnitude < remainingDifferenceSQR)  // don't accelerate too much!
                    {
                        rb.AddForce(VelocityDifference);
                    }
                    else if (VelocityDifference.sqrMagnitude > remainingDifferenceSQR)
                    {
                        /*VelocityDifference = targetHorizontalVelocity - rb.velocity;
                        VelocityDifference.y = 0.0f;        // not interested in fall speed
                        rb.AddForce(VelocityDifference);
                        Debug.Log(string.Format("{0},{1},{2},{3},{4}", rb.velocity.x, acc, targetHorizontalVelocity ,VelocityDifference.x, Mathf.Abs(remainingDifferenceSQR)));
                        */
                        Vector2 vel = rb.velocity;
                        vel.x = targetHorizontalVelocity.x;
                        rb.velocity = vel;
                    }
                }
                else
                {
                    rb.AddForce(targetHorizontalVelocity * acc);

                }
            }

            if (IsHolding)    // Player controlled characters are still affected by the impulse when releasing (otherwise how else could they sproiing)
            {
                if (ropeForce.magnitude > MaxHoldForce)
                {
                    IsReleasing = true;
                    IsHolding = false;
                    WillHold = false;
                }
                rb.velocity = Vector2.zero;
                rb.MovePosition(holdPos);
            }
            //else
            //{
            if (IsReleasing)
            {
                rb.AddForce(ropeForce, ForceMode2D.Impulse);
                IsReleasing = false;        // we have release so clear the trigger
                IsHolding = false;

                audioController.PlayClip(audioController.release);
            } else if(MaxRope)
            {
                rb.AddForce(ropeForce, ForceMode2D.Force);
            }
            //}
        }
        else    // Non-player controlled characters are pulled by the rope unless they are holding on to something.
        {

            if (IsHolding)
            {
                if (ropeForce.magnitude > MaxHoldForce)
                {
                    IsReleasing = true;
                    IsHolding = false;
                    WillHold = false;
                }
                rb.velocity = Vector2.zero;
                rb.MovePosition(holdPos);
            }
            else
            {
                if (IsReleasing)
                {
                    rb.AddForce(ropeForce, ForceMode2D.Impulse);
                }
                else
                {
                    rb.AddForce(ropeForce, ForceMode2D.Force);
                }
                if(isOnGround)
                {
                    Vector2 VelocityDifference = Vector2.zero - rb.velocity;
                    VelocityDifference.y = 0.0f;        // not interested in fall speed
                    float remainingDifferenceSQR = VelocityDifference.sqrMagnitude;
                    VelocityDifference.Normalize();
                    VelocityDifference *= acc;
                    if (VelocityDifference.sqrMagnitude < remainingDifferenceSQR)  // don't accelerate too much!
                    {
                        rb.AddForce(VelocityDifference);
                    }
                    else if (VelocityDifference.sqrMagnitude > remainingDifferenceSQR)
                    {
                        /*VelocityDifference = targetHorizontalVelocity - rb.velocity;
                        VelocityDifference.y = 0.0f;        // not interested in fall speed
                        rb.AddForce(VelocityDifference);
                        Debug.Log(string.Format("{0},{1},{2},{3},{4}", rb.velocity.x, acc, targetHorizontalVelocity ,VelocityDifference.x, Mathf.Abs(remainingDifferenceSQR)));
                        */
                        Vector2 vel = rb.velocity;
                        vel.x = targetHorizontalVelocity.x;
                        rb.velocity = vel;
                    }
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int layerMask = groundLayerMask.value;
        int collisionLayer = 1 << collision.gameObject.layer;
        if ((layerMask & collisionLayer) != 0)
        {
            if (WillHold)
            {

                IsHolding = true;
                IsReleasing = false;
                WillHold = false;
                holdPos = transform.position;
                audioController.PlayClip(audioController.hitHold);
            }

            else
            {
                audioController.PlayClip(audioController.hitNoHold);
            }
        }
    }


    private void OnCollisionStay2D(Collision2D collision)
    {
        int layerMask = groundLayerMask.value;
        int collisionLayer = 1 << collision.gameObject.layer;
        if ((layerMask & collisionLayer) != 0)
        {
            if (WillHold)
            {

                IsHolding = true;
                IsReleasing = false;
                WillHold = false;
                holdPos = transform.position;

                audioController.PlayClip(audioController.hitHold);
            }
        }
    }

    protected void doJump()
    {
        rb.AddForce(new Vector2(0.0f, jumpAccumAmount), ForceMode2D.Impulse);
    }

    public void SetRopeForce(Vector2 force)
    {
        ropeForce = force;
    }

    // need to be co-routine so the swap is performed after the key press frame is over. Otherwise the nextCharacter class will also process the key press.
    public IEnumerator SwapPlayerCharacter()
    {
        yield return 0;
        LeaveCharacter();
        nextCharacter.EnterCharacter();
    }

    public void LeaveCharacter()
    {
        PlayerControlled = false;
        cameraFollow.enabled = PlayerControlled;
    }

    public void EnterCharacter()
    {
        PlayerControlled = true;
        cameraFollow.enabled = PlayerControlled;
    }

}