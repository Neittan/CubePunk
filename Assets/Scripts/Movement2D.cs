using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Movement2D : MonoBehaviour {

    private Rigidbody2D rb;
    
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
        wasGrounded,
        isJumping,
        isClimbing,
        isClimbableUp,
        isClimableDown,
        isCrouching,
        isDodging,
        isHurted,
        isAscending,
        isDescending;

    private float velocityDeadZone;
    
    public UnityEvent LandingEvent;
    
    private void Awake() {
        rb = GetComponent<Rigidbody2D>();        
        if (rb == null) throw new NotImplementedException("Object doesn't have Rigidbody2D component");
        if (LandingEvent == null) LandingEvent = new UnityEvent();
        isFacingRight = true;
        isGrounded = CheckIsGrounded();
        wasGrounded = isGrounded;
        isJumping = false;
        isClimbing = false;
        isClimbableUp = CheckIsClimableUp();
        isClimableDown = CheckIsClimableDown();
        isCrouching = false;
        isDodging = false;
        isHurted = false;
        isAscending = CheckIsAscending();
        isDescending = CheckIsDescending();
        
        
    }

    private void FixedUpdate() {
        isGrounded = CheckIsGrounded();
        isClimbableUp = CheckIsClimableUp();
        isClimableDown = CheckIsClimableDown();
        isAscending = CheckIsAscending();
        isDescending = CheckIsDescending();
        if (!wasGrounded && isGrounded) LandingEvent.Invoke();
        wasGrounded = isGrounded;
    }

    #region Status Check Methods

    private bool CheckIsGrounded() {
        return Physics2D.Raycast(GroundCheck.position, Vector2.down, groundCheckDistance, WhatIsGround);
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

    private bool CheckIsAscending() {
        return rb.velocity.y > velocityDeadZone;
    }
    
    private bool CheckIsDescending() {
        return rb.velocity.y < -velocityDeadZone;
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
