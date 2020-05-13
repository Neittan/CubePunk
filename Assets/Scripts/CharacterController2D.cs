using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour {
    
    private Rigidbody2D rb;
    private Transform transform;
    
    [SerializeField] private BoxCollider2D UpperBodyTrigger;
    private CapsuleCollider2D CharacterBodyCollider;
    
    

    #region Status variables declaration
    
    [SerializeField] private Transform
        HandsCheck,
        GroundCheck,
        UnderGroundCheck;

    [SerializeField] private float 
        groundCheckDistance,
        climbCheckDistance;

    [SerializeField] private LayerMask
        WhatIsGround,
        WhatIsLadder;


    public bool
        isFacingRight,
        isGrounded,
        isWalking,
        wasGrounded,
        isJumping,
        isClimbing,
        isClimbableUp,
        isClimableDown,
        isCrouching,
        isDodging,
        isHurted,
        isFalling;

    private bool jumpedFromLadder = false;
    
    
    
    
    #endregion

    #region Movement variables declaration

    private Vector2 movement;

    
    [SerializeField] private float inJumpSpeedMult;
    [SerializeField] private int extraJumps = 0;
    [SerializeField] private float extraJumpHeightMult = .7f;
    [SerializeField] private float walkMult = .5f;
    [SerializeField] private float velocityDeadZone = 1f;
    [SerializeField] private bool isClimable = false;
    [SerializeField] private bool wasClimable = false;
    
    #endregion
    
    
    
    private void Awake() {
        
        rb = GetComponent<Rigidbody2D>();
        transform = GetComponent<Transform>();
        CharacterBodyCollider = GetComponent<CapsuleCollider2D>();
        
        isFacingRight = true;
        isGrounded = CheckIsGrounded();
        wasGrounded = isGrounded;
        isJumping = false;
        isClimbing = false;
        isClimbableUp = CheckIsClimableUp();
        isClimableDown = CheckIsClimableDown();
        isClimable = (isClimbableUp || isClimableDown);
        wasClimable = isClimable;
        isCrouching = false;
        isDodging = false;
        isHurted = false;
    }
    private void FixedUpdate() {
        
        isGrounded = CheckIsGrounded();
        isClimbableUp = CheckIsClimableUp();
        isClimableDown = CheckIsClimableDown();
        isClimable = (isClimbableUp || isClimableDown);
        
        
        
        if (!isJumping && !isClimbing && rb.velocity.y < -velocityDeadZone) isFalling = true;
        if (!wasGrounded && isGrounded && !isClimbing && rb.velocity.y <=0) OnLanding();
        if ((isFalling || jumpedFromLadder) && (isClimable && !wasClimable)) StartClimb();
        if ((Mathf.Abs(rb.velocity.y) < velocityDeadZone) && jumpedFromLadder && !isClimbing && isClimable) StartClimb();
        
        wasGrounded = isGrounded;
        wasClimable = isClimable;
    }

    #region Status Check Methods

    private bool CheckIsGrounded() {
        return (Physics2D.OverlapCircle(GroundCheck.position,  groundCheckDistance, WhatIsGround) && !isClimbing);
    }

    private bool CheckIsClimableUp()
    {
        return Physics2D.Raycast(GroundCheck.position, Vector2.up, climbCheckDistance, WhatIsLadder) || 
               Physics2D.Raycast(HandsCheck.position, Vector2.up, climbCheckDistance, WhatIsLadder)
            ;
    }
    private bool CheckIsClimableDown() {
        return Physics2D.Raycast(UnderGroundCheck.position, Vector2.down, climbCheckDistance, WhatIsLadder) &&
               Physics2D.Raycast(HandsCheck.position, Vector2.down, climbCheckDistance, WhatIsLadder);
    }

    
    #endregion

    #region Movement Methods

    public void Move(float horSpeed_) {
        float speed_ = horSpeed_;
        if (isCrouching || isClimbing)  speed_ = 0f; else
            if (isJumping) speed_ = horSpeed_ * inJumpSpeedMult; else
                if (isWalking) speed_ = horSpeed_ * walkMult;
        rb.velocity = new Vector2(speed_, rb.velocity.y);
        if (isGrounded) FlipIfWrongFacing(horSpeed_);
    }

    public void MovementUp(float speed_) {
        if (isCrouching) {
            UnCrouch();
        }
        if (isClimbableUp) {
                if (isClimbing) {
                    rb.velocity = new Vector2(rb.velocity.x, speed_);
                }
                else if(!jumpedFromLadder) StartClimb(); 
                    else
                        if (rb.velocity.y < velocityDeadZone) StartClimb();
                    
                        
                    
        } else if (isClimbing) StopClimb();
    }
    
    public void MovementDown(float speed_) {
        if (isClimableDown) {
            if (isClimbing) {
                rb.velocity = new Vector2(rb.velocity.x, speed_);
            }
            else
                StartClimb();
        } else 
            if (isClimbing) {
                StopClimb();
            }

        if (!isCrouching && isGrounded) {
                Crouch();
        }
    }
    public void ClimbIdle() {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
    }
    

    private void OnLanding() {
        EnableUpperBodyCollider();
        isJumping = false;
        isFalling = false;
        extraJumps = 0;
        jumpedFromLadder = false;
        Debug.Log("Landed");
    }
    private void StartClimb() {
        isClimbing = true;
        isJumping = false;
        extraJumps = 0;
        jumpedFromLadder = false;
        CharacterBodyCollider.enabled = false;
        StartCoroutine(MoveToLadderPosition());
        GravityOFF();
    }
    private void StopClimb() {
        isClimbing = false;
        CharacterBodyCollider.enabled = true;
        GravityON();
    }

    
    private void GravityON() {
        rb.gravityScale = 1f;
    }
    private void GravityOFF() {
        rb.gravityScale = 0f;
    }
    private void Crouch() {
        isCrouching = true;
        DisableUpperBodyCollider();
    }
    public void UnCrouch() {
        isCrouching = false;
        EnableUpperBodyCollider();
    }
    private void DisableUpperBodyCollider() {
        if (UpperBodyTrigger != null) UpperBodyTrigger.enabled = false;
    }
    private void EnableUpperBodyCollider() {
        if (UpperBodyTrigger != null) UpperBodyTrigger.enabled = true;
    }
    public void Jump(float jumpHeight_) {
        isCrouching = false;
        DisableUpperBodyCollider();
        if (isClimbing) {
            StopClimb();
            jumpedFromLadder = true;
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight_ * extraJumpHeightMult);
        }else if (isJumping && extraJumps > 0) {
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight_ * extraJumpHeightMult);
            extraJumps--;
            isJumping = true;
        } else
            if (isGrounded) {
                rb.velocity = new Vector2(rb.velocity.x, jumpHeight_);
                extraJumps = 1;
                isJumping = true;
            } 
    }
    

    IEnumerator MoveToLadderPosition() {
        float m_pos = rb.position.x;
        float m_desiredPos = Mathf.Floor(rb.position.x) + 0.5f;
        for (int i = 1; i <= 10; i++) {
            rb.position = new Vector2(rb.position.x - ((m_pos - m_desiredPos) / 10), rb.position.y);
            yield return new WaitForSeconds(.01f);
        }
        rb.position = new Vector2(m_desiredPos, rb.position.y);
    }
    private void Flip() {
        isFacingRight = !isFacingRight;
        var objScale = transform.localScale;
        objScale.x *= -1;
        transform.localScale = objScale;
    }
    
    private void FlipIfWrongFacing(float moveDirection_)
    {
        if (!isFacingRight && moveDirection_ > 0) {
            Flip();
        } else if (isFacingRight  && moveDirection_ < 0) Flip();
    }
    
    #endregion

    
    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(HandsCheck.position, climbCheckDistance);
        Gizmos.DrawWireSphere(UnderGroundCheck.position, climbCheckDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GroundCheck.position, groundCheckDistance);
        
    }
}
