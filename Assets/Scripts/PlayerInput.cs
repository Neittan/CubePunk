using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInput : MonoBehaviour {

    private CharacterMovement movement;
    
    [Header("Input")]
    [SerializeField] private float xInput;
    [SerializeField] private float yInput;
    
    [Header("Buttons")]
    private KeyCode resetLevel;

    private bool actionsInputDisabled;

    private void Awake() {
        movement = GetComponent<CharacterMovement>();
    }

    private void FixedUpdate() {
        
        ApplyMovement();
    }

    private void Update() {
        CheckInput();
        ApplyActions();
    }

    private void CheckInput() {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        resetLevel = KeyCode.F12;
        
    }

    private void ApplyMovement() {
        if (movement.IsAllActionsDisabled()) return;
        movement.Move(xInput * Time.fixedDeltaTime);
    }
    private void ApplyActions() {
        if (movement.IsAllActionsDisabled()) return;
        if (Input.GetKeyDown(resetLevel)) SceneManager.LoadScene(0);
        
        if (Input.GetButton("Walk")) {
            movement.WalkON();
        } else movement.WalkOFF();
        
        if (Input.GetButtonDown("Jump")) {
            movement.Jump();
        }
        if (Input.GetButtonUp("Jump")) {
            movement.VariableJump();
        }
        
        if (Input.GetButtonDown("Dodge")) {
            movement.Dodge();
        }
        
        if (Input.GetButton("Crouch")) {
            movement.Crouch();
        } else movement.UnCrouch();

        if (Input.GetButtonDown("Fire2")) movement.Dash();

    }
    
}

