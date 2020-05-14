using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class CharacterMovement : MonoBehaviour {
    
    private Rigidbody2D rb;
    private Transform transform;
    private Animator anim;
    
    [SerializeField] private BoxCollider2D BodyTrigger;
    [SerializeField] private BoxCollider2D UpperBodyTrigger;
    private CapsuleCollider2D CharacterBodyCollider;

    [SerializeField]private Transform CeilingCheck;
    [SerializeField]private Transform HandsCheck;
    [SerializeField]private Transform GroundCheck;
    [SerializeField]private Transform UnderGroundCheck;

    [SerializeField]private float groundCheckDistance;
    [SerializeField]private float climbCheckDistance;
    [SerializeField]private float saveFallDistance;

    [SerializeField]private LayerMask WhatIsGround;
    [SerializeField]private LayerMask WhatIsLadder;
    [SerializeField]private LayerMask WhatIsCeiling;

    private bool isFacingRight;
    private bool isGrounded;
    [SerializeField] private bool isCeiled;
    private bool wasGrounded;
    private bool isWalking;
    private bool isJumping;
    private bool isClimbing;
    private bool isClimbableUp;
    private bool isClimableDown;
    private bool isOnLadder;
    private bool isCrouching;
    private bool isDodging;
    private bool isHurted;
    private bool isFalling;
    
    #region Movement variables declaration
    
    [SerializeField] private float velocityDeadZone = 1f;
    private bool isClimable;
    private int extraJumps = 0;
    
    #endregion
    
    
    
    private void Awake() {
        
        rb = GetComponent<Rigidbody2D>();
        transform = GetComponent<Transform>();
        CharacterBodyCollider = GetComponent<CapsuleCollider2D>();
        StatusHandle();
        isFacingRight = true;
        isJumping = false;
        isClimbing = false;
        isCrouching = false;
        isDodging = false;
        isHurted = false;
    }
    private void FixedUpdate() {
       
        StatusHandle();
        SaveMovementStatusOnUpdateEnd();
    }

    #region Status Handle
    public void StatusHandle() {
        
        isGrounded = CheckIsGrounded();
        isClimbableUp = CheckIsClimableUp();
        isClimableDown = CheckIsClimableDown();
        isCeiled = CheckIsCeiled();
        isClimable = (isClimbableUp || isClimableDown);
        if (!isClimable) isOnLadder = false;
        isFalling = (!isJumping && !isClimbing && (rb.velocity.y < -velocityDeadZone));
        if (UpperBodyTrigger != null) UpperBodyTrigger.enabled = !(isCrouching || isJumping);
        if (!wasGrounded && isGrounded && !isClimbing && rb.velocity.y <=0) OnLanding();
        if (!isOnLadder && !CheckSaveFall() && isClimable) StartClimb();
        if ((Mathf.Abs(rb.velocity.y) < velocityDeadZone) && isOnLadder && !isClimbing && isClimable) StartClimb();
        
    }
    private void SaveMovementStatusOnUpdateEnd() {
        wasGrounded = isGrounded;
        
    }
    private bool CheckIsGrounded() {
        return (Physics2D.OverlapCircle(GroundCheck.position,  groundCheckDistance, WhatIsGround) && !isClimbing);
    }
    private bool CheckIsCeiled() {
        return Physics2D.OverlapCircle(CeilingCheck.position,  groundCheckDistance, WhatIsCeiling);
    }

    private bool CheckIsClimableUp()
    {
        return Physics2D.Raycast(GroundCheck.position, Vector2.up, climbCheckDistance, WhatIsLadder) || 
               Physics2D.Raycast(HandsCheck.position, Vector2.up, climbCheckDistance, WhatIsLadder)
            ;
    }
    private bool CheckIsClimableDown() {
        return Physics2D.Raycast(UnderGroundCheck.position, Vector2.up, climbCheckDistance, WhatIsLadder) &&
               (!isGrounded || Physics2D.Raycast(HandsCheck.position, Vector2.down, climbCheckDistance, WhatIsLadder));

    }

    private bool CheckSaveFall() {
        return Physics2D.Raycast(GroundCheck.position, Vector2.down, saveFallDistance, WhatIsGround);
    }
    
    #endregion
   
    #region Movement Handle
    
    public void Move(float moveSpeed_, float crouchSpeedMult_, float inJumpSpeedMult_, float walkSpeedMult_) {
        float totalSpeed_ = moveSpeed_;
        if (isClimbing) totalSpeed_ = 0f;
        else 
            if (isCrouching)  totalSpeed_ *= crouchSpeedMult_; 
            else
                if (isJumping) totalSpeed_ *= inJumpSpeedMult_; 
                else 
                    if (isWalking) totalSpeed_ *= walkSpeedMult_; 
        rb.velocity = new Vector2(totalSpeed_, rb.velocity.y);
        if (isGrounded) FlipIfWrongFacing(totalSpeed_);
    }

    public void MovementUp(float climbSpeed_) {
        isCrouching = false;
        if (isCeiled && isClimbing) ClimbIdle();
        else 
            if (isClimbableUp) {
                if (isClimbing) rb.velocity = new Vector2(rb.velocity.x, climbSpeed_);
                else 
                    if(isOnLadder && rb.velocity.y < velocityDeadZone) StartClimb();
                    else
                        if (rb.velocity.y < velocityDeadZone) StartClimb();
            } else 
                if (isClimbing) StopClimb();
    }
    
    public void MovementDown(float climbSpeed_, bool jumpDownCondition_) {
        if (isClimableDown) {
            if (isClimbing) rb.velocity = new Vector2(rb.velocity.x, climbSpeed_);
            else 
                if (!isGrounded) StartClimb();
                else 
                    if (jumpDownCondition_) StartClimb();
        } else 
            if (isClimbing) StopClimb();
        if (!isCrouching && isGrounded) {
                Crouch();
        }
    }
    
    private void StartClimb() {
        CharacterBodyCollider.enabled = false;
        rb.gravityScale = 0;
        StopFall();
        isClimbing = true;
        isOnLadder = true;
        StartCoroutine(MoveToLadderPosition());
        
    }
    private void StopClimb() {
        isClimbing = false;
        CharacterBodyCollider.enabled = true;
        rb.gravityScale = 1;
        
    }
    
    private void OnLanding() {
        StopFall();
        isOnLadder = false;
        
    }
    private void StopFall() {
        isJumping = false;
        isFalling = false;
        extraJumps = 0;
    }
    IEnumerator MoveToLadderPosition() {
        float curPosition_ = rb.position.x;
        float desiredPos_ = Mathf.Floor(rb.position.x) + 0.5f;
        for (int i = 1; i <= 10; i++) {
            rb.position = new Vector2(rb.position.x - ((curPosition_ - desiredPos_) / 10), rb.position.y);
            yield return new WaitForSeconds(.015f);
        }
        rb.position = new Vector2(desiredPos_, rb.position.y);
    }
    private void FlipIfWrongFacing(float moveDirection_)
    {
        if (!isFacingRight && moveDirection_ > 0) {
            Flip();
        } else if (isFacingRight  && moveDirection_ < 0) Flip();
    }
    private void Flip() {
        isFacingRight = !isFacingRight;
        var objScale = transform.localScale;
        objScale.x *= -1;
        transform.localScale = objScale;
    }
    
    #endregion

    #region Character Actions
    
    public void Jump(float jumpHeight_, float extraJumpHeightMult_) {
        isCrouching = false;
        if (isClimbing) {
            StopClimb();
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight_ * extraJumpHeightMult_);
        }else if (isJumping && extraJumps > 0) {
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight_ * extraJumpHeightMult_);
            extraJumps--;
            isJumping = true;
        } else
            if (isGrounded) {
                rb.velocity = new Vector2(rb.velocity.x, jumpHeight_);
                extraJumps = 1;
                isJumping = true;
            } 
    }
    public void Crouch() {
        isCrouching = true;
    }
    public void UnCrouch() {
        isCrouching = false;
    }
    public void ClimbIdle() {
        if (isClimbing) rb.velocity = new Vector2(rb.velocity.x, 0f);
    }

    public void Walk(bool condition_) {
        isWalking = condition_;
    }

    
    #endregion

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(HandsCheck.position, climbCheckDistance);
        Gizmos.DrawWireSphere(UnderGroundCheck.position, climbCheckDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GroundCheck.position, groundCheckDistance);
        Gizmos.DrawWireSphere(CeilingCheck.position, groundCheckDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GroundCheck.position, saveFallDistance);

    }
}
