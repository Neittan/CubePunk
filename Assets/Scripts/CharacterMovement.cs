using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class CharacterMovement : MonoBehaviour {
    
    private Rigidbody2D rb;
    private Transform transform;
    private Animator anim;
    
    [Header("Colliders")]
    [SerializeField] private BoxCollider2D BodyTrigger;
    [SerializeField] private BoxCollider2D UpperBodyTrigger;
    [SerializeField] private CapsuleCollider2D CharacterBodyCollider;
    
    [Header("Checks")]
    [SerializeField] private Transform CeilingCheck;
    [SerializeField] private Transform HandsCheck;
    [SerializeField] private Transform GroundCheck;
    [SerializeField] private Transform UnderGroundCheck;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float climbCheckDistance;
    [SerializeField] private float saveFallDistance;
    
    [Header("Layermasks")]
    [SerializeField] private LayerMask WhatIsGround;
    [SerializeField] private LayerMask WhatIsLadder;
    [SerializeField] private LayerMask WhatIsCeiling;

    [Header("Movement parameters")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float walkSpeedMult;
    [SerializeField] private float crouchSpeedMult;
    [SerializeField] private float inJumpSpeedMult;
    [SerializeField] private float climbSpeed;
    [SerializeField] private float jumpForce;
    
    private float velocityDeadZoneX = 0.01f;
    private float velocityDeadZoneY = 0.1f;
    private float fallingVelocity = -5f;
    [SerializeField]private bool isFacingRight;
    [SerializeField]private bool isGrounded;
    [SerializeField]private bool isRunning;
    [SerializeField]private bool isWalking;
    [SerializeField]private bool isJumping;
    [SerializeField]private bool isClimbing;
    [SerializeField]private bool isClimbingIdle;
    [SerializeField]private bool isCrouching;
    private bool isDodging;
    private bool isHurted;
    [SerializeField]private bool isFalling;

    private bool isOnLadder;
    private bool isCeilinged;
    private bool isJumpable;
    private bool isClimbableUp;
    private bool isClimbableDown;
    private bool isClimbable;
    private float prevPosition;
    [SerializeField]private int extraJumps;
    
    
    
    
    
    
    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        transform = GetComponent<Transform>();
        CharacterBodyCollider = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
        
        StateHandle();
        isFacingRight = true;
        isJumping = false;
        isClimbing = false;
        isCrouching = false;
        isDodging = false;
        isHurted = false;
    }
    private void FixedUpdate() {
       
        StateHandle();
        UpdateAnimations();

    }

    #region Character State
    public void StateHandle() {
        bool wasGrounded = isGrounded;
        isGrounded = CheckIsGrounded();
        isCeilinged = CheckIsCeilinged();
        isClimbable = CheckClimbableDown() || CheckClimbableUp();
        if (!isClimbable) isOnLadder = false;
        isJumpable = (isGrounded || isClimbing || (extraJumps > 0 && isJumping));
        if (UpperBodyTrigger != null) UpperBodyTrigger.enabled = !(isCrouching || isJumping);

        if (!isOnLadder && !CheckSaveFall() && (CheckClimbableUp() && CheckClimbableDown())) StartClimb();
        if ((Mathf.Abs(rb.velocity.y) < velocityDeadZoneY) && isOnLadder && !isClimbing && isClimbable) StartClimb();
        isFalling = (!isJumping && !isClimbing && (rb.velocity.y < fallingVelocity));
        

        if (!wasGrounded && isGrounded && (rb.position.y < prevPosition)) OnLanding();
        if (isClimbing || isClimbingIdle) rb.gravityScale = 0;
            else rb.gravityScale = 1f;
        CharacterBodyCollider.enabled = !isClimbing;
        UpperBodyTrigger.enabled = !(isCrouching || isJumping);
        prevPosition = rb.position.y;
    }
    private bool CheckIsGrounded() {
        return (Physics2D.Raycast(GroundCheck.position, Vector2.down,  groundCheckDistance, WhatIsGround))
            && !(Physics2D.Raycast(GroundCheck.position, Vector2.up,  groundCheckDistance, WhatIsGround));
    }
    private bool CheckIsCeilinged() {
        return Physics2D.OverlapCircle(CeilingCheck.position,  groundCheckDistance, WhatIsCeiling);
    }
    
    private bool CheckClimbableUp() {
        return (Physics2D.Raycast(UnderGroundCheck.position, Vector2.up, climbCheckDistance, WhatIsLadder) || 
               Physics2D.Raycast(HandsCheck.position, Vector2.up, climbCheckDistance, WhatIsLadder)) && !isCeilinged;
    }
    
    private bool CheckClimbableDown() {
        return Physics2D.Raycast(UnderGroundCheck.position, Vector2.up, climbCheckDistance, WhatIsLadder) &&
                Physics2D.Raycast(HandsCheck.position, Vector2.down, climbCheckDistance, WhatIsLadder);
    }
    private bool CheckSaveFall() {
        return Physics2D.Raycast(GroundCheck.position, Vector2.down, saveFallDistance, WhatIsGround);
    }

    
    #endregion
   
    #region Movement Handle
    
    public void MoveLeftRight(float moveDirectionX_, bool walkCondition_) {
        float totalSpeed_ =  moveSpeed * moveDirectionX_;
        isWalking = walkCondition_;
        if (isClimbing) totalSpeed_ = 0f;
            else if (isCrouching)  totalSpeed_ *= crouchSpeedMult; 
                else if (isJumping) totalSpeed_ *= inJumpSpeedMult; 
                    else if (walkCondition_) 
                        totalSpeed_ *= walkSpeedMult;
        isRunning = (Mathf.Abs(moveDirectionX_) > velocityDeadZoneX);        
        rb.velocity = new Vector2(totalSpeed_, rb.velocity.y);
        FlipIfWrongFacing(totalSpeed_);
    }
    private void FlipIfWrongFacing(float moveDirection_)
    {
        if (!isFacingRight && moveDirection_ > 0) {
            Flip();
        } else if (isFacingRight  && moveDirection_ < 0) Flip();
    }
    private void Flip() {
        isFacingRight = !isFacingRight;
        Vector3 objScale = transform.localScale;
        objScale.x *= -1;
        transform.localScale = objScale;
    }
    
    public void MoveUpDown(float moveDirectionY_, bool jumpDownCondition) {
        bool isClimbableUp = moveDirectionY_ > 0 && CheckClimbableUp();
        bool isClimbableDown = (moveDirectionY_ < 0 && CheckClimbableDown() && (isClimbing || jumpDownCondition));
        isClimbingIdle = isClimbing && moveDirectionY_ == 0;
        if (isClimbableUp || isClimbableDown || isClimbingIdle) {
            if (!isClimbing) StartClimb();
            rb.velocity = new Vector2(rb.velocity.x, moveDirectionY_ * climbSpeed);
        }
        else
            isClimbing = false;

        if (!isClimbableDown && isGrounded) 
            if (moveDirectionY_ <0) Crouch(); else UnCrouch();
    }
    
    private void StartClimb() {
        isJumping = false;
        isFalling = false;
        isClimbing = true;
        isOnLadder = true;
        rb.gravityScale = 0;
        StartCoroutine(MoveToLadderPosition());
    }
    
    IEnumerator MoveToLadderPosition() {
        float curPosition_ = rb.position.x;
        float desiredPos_ = Mathf.Floor(rb.position.x) + 0.5f;
        for (int i = 1; i <= 15; i++) {
            rb.position = new Vector2(rb.position.x - ((curPosition_ - desiredPos_) / 15), rb.position.y);
            yield return new WaitForSeconds(.01f);
        }
        rb.position = new Vector2(desiredPos_, rb.position.y);
    }
    
    private void OnLanding() {
        isOnLadder = false;
        isJumping = false;
        if (isFalling) anim.SetTrigger("LandedFromFall");
        isFalling = false;    
        Debug.Log("Landed");
    }
    
    #endregion

    #region Character Actions

    public void Jump() {
        if (!isJumpable) return;
        extraJumps--;
        if (isGrounded)
            extraJumps = 1;
        else
            extraJumps = 0;
        isClimbing = false;
        isJumping = true;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        FlipIfWrongFacing(rb.velocity.x);
        
    }

    public void Crouch() {
        isCrouching = true;
        UpperBodyTrigger.enabled = false;
    }
    public void UnCrouch() {
        isCrouching = false;
        UpperBodyTrigger.enabled = true;
    }
    #endregion

    #region Animations Handle

    private void UpdateAnimations() {
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isJumping", isJumping);
        anim.SetInteger("ExtraJumps", extraJumps);
        anim.SetBool("isClimbing", isClimbing);
        anim.SetBool("isClimbingIdle", isClimbingIdle);
        anim.SetBool("isCrouching", isCrouching);
        anim.SetBool("isFalling", isFalling);
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
