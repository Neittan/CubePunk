using System.Collections;
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
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float saveFallDistance;
    
    [Header("Layermasks")]
    [SerializeField] private LayerMask WhatIsGround;
    
    [Header("Movement parameters")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float walkSpeedMult;
    [SerializeField] private float inJumpSpeedMult;
    [SerializeField] private float jumpForce;
    [SerializeField] private int extraJumpsMax;
    [SerializeField] private float dodgeTime;
    
    private float velocityDeadZoneX = 0.01f;
    private float velocityDeadZoneY = 0.01f;
    
    [SerializeField] private bool isFacingRight;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isTouchingWall;
    [SerializeField] private bool isWalking;
    [SerializeField] private bool isJumping;
    [SerializeField] private bool isCrouching;
    [SerializeField] private bool isFalling;
    [SerializeField] private bool isDodging;
    [SerializeField] private bool isShooting;

    [Space]
    public bool actionsRestricted;
    [SerializeField] bool canMove;
    [SerializeField]private bool canJump;
    [SerializeField]private bool canCrouch;
    [SerializeField]private bool canDodge;
    [SerializeField]private float prevPositionY;
    [SerializeField]private float nextMovementX;
    private int extraJumpsLeft;
        
    
    
    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        CharacterCollider = GetComponent<CapsuleCollider2D>();
        UpperBodyCollider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        
        UpdateState();
        isFacingRight = true;
        isJumping = false;
        isDodging = false;
        isShooting = false;
    }
    private void FixedUpdate() {
       
        UpdateState();
        UpdateAnimations();
    }

    
    public void UpdateState() {
        bool wasGrounded = isGrounded;
        isGrounded = CheckIsGrounded();
        isFalling = !isJumping && !isGrounded && rb.position.y < prevPositionY;
        canMove = !isCrouching && !actionsRestricted;
        canJump = (isGrounded || (extraJumpsLeft > 0 && isJumping)) && !actionsRestricted;
        canCrouch = isGrounded && !isJumping && !isDodging && !actionsRestricted;
        canDodge = isGrounded && !actionsRestricted;
        if (!wasGrounded && isGrounded && (rb.position.y <= prevPositionY)) OnLanding();
        prevPositionY = rb.position.y;
    }
    private bool CheckIsGrounded() {
        return (Physics2D.Raycast(GroundCheck.position, Vector2.down,  groundCheckDistance, WhatIsGround));
    }
    
    public void Move(float moveDirectionX_, bool walkCondition_) {
        if (!canMove) {
            nextMovementX = 0f;
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }
        nextMovementX =  moveSpeed * moveDirectionX_;
        isWalking = walkCondition_;
        if (isJumping) nextMovementX *= inJumpSpeedMult; 
                    else if (walkCondition_) 
                        nextMovementX *=walkSpeedMult;
        rb.velocity = new Vector2(nextMovementX, rb.velocity.y);
        if (isGrounded)FlipIfWrongFacing(nextMovementX);
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
    
    private void OnLanding() {
        isJumping = false;
        if (isFalling) anim.SetTrigger("LandedFromFall");
        isFalling = false;
        anim.speed = 1f;
        Debug.Log("Landed");
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
        actionsRestricted = true;
        isDodging = true;
        anim.speed = 1 / dodgeTime_;
        yield return new WaitForSeconds(dodgeTime_);
        isDodging = false;
        actionsRestricted = false;
        anim.speed = 1f;

    }

    
    private void UpdateAnimations() {
        anim.SetFloat("MoveSpeed", Mathf.Abs(nextMovementX));
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isJumping", isJumping);
        anim.SetBool("isCrouching", isCrouching);
        anim.SetBool("isFalling", isFalling);
        anim.SetBool("isDodging", isDodging);
    }
    
    private void OnDrawGizmos() {
       //groundCheck
        Gizmos.color = Color.red;
        Gizmos.DrawLine(GroundCheck.position, GroundCheck.position + Vector3.down * groundCheckDistance);
        //wallCheck
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(WallCheck.position, WallCheck.position + Vector3.right * wallCheckDistance);
    }
}
