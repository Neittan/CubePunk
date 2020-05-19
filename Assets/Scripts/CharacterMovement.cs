using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CharacterMovement : MonoBehaviour {
    
    private Rigidbody2D rb;
    private Animator anim;
    
    [Header("Colliders")]
    private CapsuleCollider2D CharacterCollider;
    private BoxCollider2D UpperBodyCollider;
    
    [Header("Checks")]
    [SerializeField] private Transform GroundCheck;
    [SerializeField] private Transform WallCheck;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private bool isGrounded;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private bool isTouchingWall;
    
    [Header("Layermasks")]
    [SerializeField] private LayerMask WhatIsGround;
    
    [Header("Movement parameters")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float walkSpeedMult;
    [SerializeField] private float inJumpSpeedMult;
    [SerializeField] private float jumpForce;
    [SerializeField] private int extraJumpsMax;
    [SerializeField] private float dodgeTime;
    [SerializeField]private int noDamageFallVelocity;
    
    [Header("State")]
    [SerializeField] private bool isFacingRight = true;
    [SerializeField] private bool isJumping = false;
    [SerializeField] private bool isFalling  = false;
    [SerializeField] private bool isDodging  = false;
    [SerializeField] private bool isShooting  = false;
    [SerializeField] private bool isCrouching = false;
    
    [SerializeField] private bool isWalking;

    [Header("Move Restrictions")]
    public bool actionsRestricted;
    [SerializeField] private bool canMove;
    [SerializeField] private bool canJump;
    [SerializeField] private bool canCrouch;
    [SerializeField] private bool canDodge;
    [SerializeField] private bool canShoot;
    
    //=========================
    private float prevPositionY;
    private float curFallVelocity;
    private float nextMovementX;
    private int extraJumpsLeft;
        
    
    
    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        CharacterCollider = GetComponent<CapsuleCollider2D>();
        UpperBodyCollider = GetComponent<BoxCollider2D>();
        
        anim.SetInteger("NoDamageVelocity", noDamageFallVelocity);
    } 
    //======================================================================================
    private void FixedUpdate() {
        UpdateState();
        UpdateAnimations();
    }
    //======================================================================================
    
    public void UpdateState() {
        bool wasGrounded = isGrounded;
        isGrounded = CheckIsGrounded();
        CheckFallVelocity();
        isFalling = !isJumping && !isGrounded && rb.position.y < prevPositionY;
        CheckActionsRestrictions();
        if (!wasGrounded && isGrounded && (rb.position.y < prevPositionY)) DoOnLanding();
        prevPositionY = rb.position.y;
        
    }
    private bool CheckIsGrounded() {
        return Physics2D.OverlapCircle(GroundCheck.position, groundCheckDistance, WhatIsGround);
    }
    private void CheckFallVelocity() {
        if (rb.velocity.y < curFallVelocity) curFallVelocity = rb.velocity.y;
        if (Mathf.Approximately(rb.velocity.y, 0f)) curFallVelocity = 0;
    }
    private void CheckActionsRestrictions() {
        if (actionsRestricted) {
            canMove = false;
            canJump = false;
            canCrouch = false;
            canDodge = false;
            canShoot = false;
        }
        else {
            canMove = !isCrouching;
            canJump = isGrounded || (extraJumpsLeft > 0 && isJumping);
            canCrouch = isGrounded && !isJumping && !isDodging;
            canDodge = isGrounded;
            canShoot = true;
        }
    }
    
    //======================================================================================
    public void Move(float moveDirectionX_, bool walkCondition_) {
        if (isGrounded)FlipIfWrongFacing(moveDirectionX_);
        if (!canMove) {
            nextMovementX = 0;
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }
        nextMovementX =  moveSpeed * moveDirectionX_;
        isWalking = walkCondition_;
        if (isJumping) nextMovementX *= inJumpSpeedMult; 
                    else if (walkCondition_) 
                        nextMovementX *=walkSpeedMult;
        rb.velocity = new Vector2(nextMovementX, rb.velocity.y);
    }
    private void FlipIfWrongFacing(float xDirection_)
    {
        if (!isFacingRight && xDirection_ > 0) {
            Flip();
        } else if (isFacingRight  && xDirection_ < 0) Flip();
    }
    private void Flip() {
        isFacingRight = !isFacingRight;
        transform.Rotate(0, 180, 0);
    }
    
    
    
    public void Jump() {
        if (!canJump) return;
        UnCrouch();
        if (isGrounded)
            extraJumpsLeft = extraJumpsMax;
        else {
            extraJumpsLeft--;
            anim.speed = 2f;
        }
        isJumping = true;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        FlipIfWrongFacing(nextMovementX);
        
    }
    public void Crouch() {
        if (!canCrouch) return;
        if (!isJumping) isCrouching = true;
        UpperBodyCollider.enabled = false;
    }
    public void UnCrouch() {
        if (!isCrouching) return;
        isCrouching = false;
        UpperBodyCollider.enabled = true;
    }
    public void Dodge() {
        if (!canDodge) return;
        UnCrouch();
        if (!isDodging) StartCoroutine(TimedDodge(dodgeTime));

    }
    IEnumerator TimedDodge(float dodgeTime_) {
        isDodging = true;
        anim.speed = 1 / dodgeTime_;
        yield return new WaitForSeconds(dodgeTime_);
        isDodging = false;
        anim.speed = 1;
    }
    IEnumerator RestrictAllActions(float restrictTime_) {
        actionsRestricted = true;
        yield return new WaitForSeconds(restrictTime_);
        actionsRestricted = false;
    }
    //======================================================================================
    private void DoOnLanding() {
        isJumping = false;
        isFalling = false;
        anim.speed = 1f;
        curFallVelocity = 0;
    }
    //======================================================================================
    private void UpdateAnimations() {
        anim.SetFloat("MoveSpeed", Mathf.Abs(nextMovementX));
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isJumping", isJumping);
        anim.SetBool("isCrouching", isCrouching);
        anim.SetBool("isFalling", isFalling);
        anim.SetBool("isDodging", isDodging);
    }
    //======================================================================================
    private void OnDrawGizmos() {
       //groundCheck
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GroundCheck.position, groundCheckDistance);
        //wallCheck
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(WallCheck.position, WallCheck.position + Vector3.right * wallCheckDistance);
        //cliffCheck
    }
}
