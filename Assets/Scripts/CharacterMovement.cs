using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;


public class CharacterMovement : MonoBehaviour {
    
    private Rigidbody2D rb;
    private Animator anim;
    
    [Header("Colliders")]
    public BoxCollider2D UpperBodyCollider;
    
    [Header("Checks")]
    public Transform GroundCheck;
    public Transform WallCheck;
    public float groundCheckDistance;
    public float wallCheckDistance;
    
    
    [Header("Layermasks")]
    public LayerMask WhatIsGround;
    
    [Header("Movement parameters")]
    public float moveSpeed;
    public float walkSpeedMult;
    public float jumpForce;
    public float inJumpSpeedMult;
    public float variableJumpHeightMult;
    public float inAirDragMult;
    public float dodgeDuration;
    public float noDamageFallVelocity;
    public float wallSlideSpeed;
    public float wallSlideTimeLimit;
    public float dashSpeed;
    public float dashDuration;
    public float distanceBetweenImages;
    public float dashCooldown;
    public int wallSlidesMax;
    public int extraJumpsMax;
    
    //defined states
    private float facingDirection = 1; // right = 1, left = -1
    private bool isFacingRight = true;
    private bool isJumping;
    private bool isTouchingWall;
    private bool isWalking;
    private bool isDodging;
    private bool isShooting;
    private bool isCrouching;
    private bool isDashing;
    //states by checks
    private bool isGrounded;
    private bool isFalling;
    private bool isWallSliding;
    
    //restrictions
    public bool canMove;
    public bool canFlip;
    public bool canJump;
    public bool canCrouch;
    public bool canDash;
    public bool canDodge;
    public bool canShoot;
    public bool canWallSlide;
    
    //inner state parameters
    private bool wasGrounded;
    private float prevPositionY;
    private float curFallVelocity;
    private float nextMovementX;
    private float dashTimeLeft;
    private float lastAfterImageXpos;
    private float lastDashStartTime = -100;
    private float wallSlideStartTime;
    private float velX, velY;
    private int extraJumpsLeft;
    private int wallSlidesLeft;

    //animations hashes
    private static readonly int MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int Walking = Animator.StringToHash("isWalking");
    private static readonly int Jumping = Animator.StringToHash("isJumping");
    private static readonly int Crouching = Animator.StringToHash("isCrouching");
    private static readonly int Falling = Animator.StringToHash("isFalling");
    private static readonly int Dodging = Animator.StringToHash("isDodging");
    private static readonly int WallSliding = Animator.StringToHash("isWallSliding");
    private static readonly int Shooting = Animator.StringToHash("isShooting");
    private static readonly int Idle = Animator.StringToHash("Idle");

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        UpperBodyCollider = GetComponent<BoxCollider2D>();

        StateResetToIdle();
    } 
    //======================================================================================
    private void FixedUpdate() {
        StateChecks();
        ActionsRestrictionsHandle();
        CollidersHandle();
        VelocityHandle();
        DashHandle();
        MovementsBlockEmpty(); //utility link
        SaveFrameEndStates();
        UpdateAnimations();

        velX = rb.velocity.x;
        velY = rb.velocity.y;
    }
    
    //======================================================================================
    
    private void StateResetToIdle() {
        isJumping = false;
        isCrouching = false;
        isWalking = false;
        isDashing = false;
        isDodging  = false;
        isShooting  = false;
        curFallVelocity = 0;
        UpperBodyCollider.enabled = true;
    }
    
    private void StateChecks() {
        isGrounded = Physics2D.OverlapCircle(GroundCheck.position, groundCheckDistance, WhatIsGround);
        isTouchingWall = Physics2D.Raycast(WallCheck.position, Vector2.right,
            (isFacingRight)? wallCheckDistance : -wallCheckDistance, WhatIsGround);
        isFalling = !isWallSliding && !isJumping && !isGrounded && curFallVelocity < 0;
        
        LandingHandle();
        WallSlideHandle();
    }
    private void ActionsRestrictionsHandle() {
        canMove = !isCrouching && !isWallSliding && !(isWalking && isShooting);
        canFlip = !isWallSliding;
        canJump = isGrounded || (extraJumpsLeft > 0 && isJumping) || isWallSliding;
        canCrouch = isGrounded && !isJumping;
        canDash = !isWallSliding && (Time.time >= lastDashStartTime + dashCooldown);
        canDodge = isGrounded;
        canWallSlide = !isGrounded && isTouchingWall && (rb.velocity.y <= -wallSlideSpeed) && wallSlidesLeft > 0;
        canShoot = !isWallSliding;
    }
    private void LandingHandle() {
        if (!wasGrounded && isGrounded && (rb.position.y < prevPositionY)) OnLanding();
    }
    private void OnLanding() {
        StateResetToIdle();
        wallSlidesLeft = wallSlidesMax;
        wallSlideStartTime = Mathf.Infinity;
        extraJumpsLeft = extraJumpsMax;
        curFallVelocity = 0;
    }
    private void WallSlideHandle() {
        CheckStartWallSlide();
        PerformWallSlide();
        CheckStopWallSlide();
    }
    private void CheckStartWallSlide() {
        if (!(!isWallSliding && canWallSlide)) return;
        StateResetToIdle();
        isWallSliding = true;
        wallSlidesLeft--;
        wallSlideStartTime = Time.time;
    }
    private void CheckStopWallSlide() {
        if (!isWallSliding) return;
        if (!canWallSlide) isWallSliding = false;
        if (Time.time - wallSlideStartTime > wallSlideTimeLimit) wallSlidesLeft = 0;
    }
    private void PerformWallSlide() {
        if (!isWallSliding) return; 
        if (rb.velocity.y < -wallSlideSpeed) {
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
        }
    }
    
    private void CollidersHandle() {
        UpperBodyCollider.enabled = !isCrouching && !isWallSliding && !isJumping;
    }
    
    private void VelocityHandle() {
        if (rb.velocity.y < curFallVelocity) curFallVelocity = rb.velocity.y;
        if (rb.velocity.y == 0) curFallVelocity = 0;
        
        PreventUnintendedAscending();
    }

    private void PreventUnintendedAscending() {
        float maxVelocity = (isJumping)? jumpForce : 0f;
        if (rb.velocity.y> maxVelocity) rb.velocity = new Vector2(rb.velocity.x, maxVelocity);
    }
    
    private void DashHandle() {
        if(!isDashing) return;
        if (dashTimeLeft > 0){
            rb.velocity=new Vector2(dashSpeed * facingDirection, rb.velocity.y);
            dashTimeLeft -= Time.fixedDeltaTime;
        }
        if(dashTimeLeft <= 0 || isTouchingWall){
            rb.velocity = new Vector2(0, rb.velocity.y);
            dashTimeLeft = 0;
            isDashing = false;
        }
        if (Mathf.Abs(transform.position.x - lastAfterImageXpos) > distanceBetweenImages) {
            PlayerAfterimagePool.Instance.GetFromPool();
            lastAfterImageXpos = transform.position.x;
        }
    }

    public bool IsAllActionsDisabled() {
        return (isDashing || isDodging);
    }
    
    private void SaveFrameEndStates() {
        wasGrounded = isGrounded;
        prevPositionY = rb.position.y;
    }
    
    private static void MovementsBlockEmpty(){}
    public void Move(float moveDirectionX_) {
        FlipIfWrongFacing(moveDirectionX_);
        if (isWallSliding && moveDirectionX_ != 0) wallSlideStartTime = Time.time;
        if (!canMove) {
            nextMovementX = 0;
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }
        nextMovementX =  moveSpeed * moveDirectionX_;
        if (!isGrounded && !isWallSliding && moveDirectionX_ == 0) nextMovementX = rb.velocity.x * inAirDragMult; 
        else if (isJumping || isFalling) {
                nextMovementX *= inJumpSpeedMult;
            }
            else if (isWalking) {
                nextMovementX *=walkSpeedMult;
            } 
        rb.velocity = new Vector2(nextMovementX, rb.velocity.y);
    }

    public void WalkON() {
        isWalking = true;
    }
    public void WalkOFF() {
        isWalking = false;
    }
    private void FlipIfWrongFacing(float moveDirection_) {
        if (!canFlip) return;
        if (!isFacingRight && moveDirection_ > 0) {
            Flip();
        } else if (isFacingRight  && moveDirection_ < 0) Flip();
    }
    private void Flip() {
        facingDirection *= -1;
        isFacingRight = !isFacingRight;
        transform.Rotate(0, 180, 0);
        
    }
    public void Jump() {
        if (!canJump) return;
        if (isGrounded) {
            NormalJump();
        } else if (isWallSliding) WallJump();
            else ExtraJump();
        rb.velocity = new Vector2(x: rb.velocity.x, jumpForce);
        StateResetToIdle();
        isJumping = true;
    }
    private void NormalJump() {
        extraJumpsLeft = extraJumpsMax;
    }
    private void ExtraJump() {
        extraJumpsLeft--;
        anim.SetTrigger(Idle);
    }
    private void WallJump() {
        extraJumpsLeft = extraJumpsMax;
    }

    
    public void VariableJump() {
        if(isJumping) rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHeightMult);
    }
    public void Crouch() {
        if (!canCrouch) isCrouching = false; 
        else isCrouching = true;
    }
    public void UnCrouch() {
        if (isCrouching) isCrouching = false; 
    }
    public void Dash() {
        if (!canDash) return;
        StateResetToIdle();
        isDashing = true;
        dashTimeLeft = dashDuration;
        lastDashStartTime = Time.time;
    }

    
    public void Dodge() {
        if (!canDodge) return;
        StateResetToIdle();
        StartCoroutine(TimedDodge(dodgeDuration));
    }
    private IEnumerator TimedDodge(float dodgeTime_) {
        float prevSpeed = anim.speed;
        isDodging = true;
        anim.speed = 1 / dodgeTime_;
        yield return new WaitForSeconds(dodgeTime_);
        isDodging = false;
        anim.speed = prevSpeed;
    }
    private void UpdateAnimations() {
        anim.SetFloat(MoveSpeed, Mathf.Abs(nextMovementX));
        anim.SetBool(Walking, isWalking);
        anim.SetBool(Jumping, isJumping);
        anim.SetBool(Crouching, isCrouching);
        anim.SetBool(Falling, isFalling);
        anim.SetBool(Dodging, isDodging);
        anim.SetBool(WallSliding, isWallSliding);
        anim.SetBool(Shooting, isShooting);
    }
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GroundCheck.position, groundCheckDistance);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(WallCheck.position, WallCheck.position + ((isFacingRight)? Vector3.right : Vector3.left) * wallCheckDistance);
    }
}
