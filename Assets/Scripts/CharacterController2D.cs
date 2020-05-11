using System;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{

    private Rigidbody2D rb;
    private Animator anim;
    private Movement2D status;

    private float inputHorizontal;
    private float inputVertical;
    
    [SerializeField] private float baseSpeed;
    [SerializeField] private float runMult;
    [SerializeField] private float jumpForce;
    [SerializeField] private float extraJumpForceMult;
    [SerializeField] private float inJumpSpeedMult;
    [SerializeField] private float baseClimbSpeed;
    [SerializeField] private float climbSideSpeed;
    private float walkSpeed;
    private float runSpeed;
    private float totalSpeed;
    private float climbSpeed;
    private int extraJumps;
    
   
    
   
 
    
   
    [SerializeField] private float defaultGravity = 1f;
    

    

    

    private void Awake()
    {
        

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        
        
        
    }
    
    

    private void FixedUpdate()
    {
        inputHorizontal = Input.GetAxisRaw("Horizontal");
        inputVertical = Input.GetAxisRaw("Vertical");
        
        runSpeed = walkSpeed * runMult;
        totalSpeed = Mathf.Abs(inputHorizontal) * (Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed);
        if (status.isJumping) totalSpeed *= inJumpSpeedMult;
        
        rb.velocity = new Vector2(inputHorizontal * totalSpeed * Time.fixedDeltaTime, rb.velocity.y);
        
        if (inputVertical >= 0 && status.isCrouching) UnCrouch(); 
        
        if (inputVertical > 0)
        {
            if (status.isClimbableUp)
            {
                if (!status.isClimbing)
                {
                    Climb();
                }
                else
                {
                    
                    rb.velocity = new Vector2(rb.velocity.x, inputVertical * climbSpeed * Time.fixedDeltaTime);
                }
                    
            } else if (status.isClimbing)
            {
                    UnClimb();
            }   
        }
        
        
        

        
        if (status.isGrounded) FlipIfWrongFacing();
        
        
        anim.SetFloat("Speed", totalSpeed);

    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (status.isGrounded) Jump();
            else if (extraJumps > 0 ) ExtraJump();
        }
                
    }


    #region Utility Methods

    

    public void Landing()
    {
        anim.speed = 1f;
        status.isJumping = false;
        extraJumps = 0;
        UnClimb();
        anim.SetTrigger("Landed");
        anim.SetBool("isJumping", false);
    }

    private void Jump()
    {
        if (status.isCrouching) UnCrouch();
        if (status.isClimbing) UnClimb();
        extraJumps = 1;
        rb.velocity = Vector2.up * jumpForce;
        status.isJumping = true;
        anim.SetBool("isJumping", true);
    }

    private void StopJump()
    {
        status.isJumping = false;
        rb.velocity = Vector2.zero;
        anim.SetBool("isJumping", false);
    }

    private void ExtraJump()
    {
        rb.velocity = Vector2.up * (jumpForce * extraJumpForceMult);
        --extraJumps;
        FlipIfWrongFacing();
        anim.speed = 2f;
    }

    private void Crouch()
    {
        walkSpeed = 0f;
        status.isCrouching = true;
        anim.SetBool("isCrouch", true);
    }

    private void UnCrouch()
    {
        walkSpeed = baseSpeed;
        status.isCrouching = false;
        anim.SetBool("isCrouch", false);
    }
    
    private void Flip() {
        status.isFacingRight = !status.isFacingRight;
        var scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void Climb() {
        UnCrouch();
        StopJump();
        status.isClimbing = true;
        walkSpeed = climbSideSpeed;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        climbSpeed = baseClimbSpeed;
        anim.SetBool("isClimb", true);
    }

    private void UnClimb() {
        status.isClimbing = false;
        walkSpeed = baseSpeed;
        rb.gravityScale = defaultGravity;
        climbSpeed = 0f;
        anim.SetBool("isClimb", false);
    }

    private void FlipIfWrongFacing()
    {
        if (!status.isFacingRight && inputHorizontal > 0) {
            Flip();
        } else if (status.isFacingRight  && inputHorizontal < 0) Flip();
    }
    #endregion

    
}

