using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class PlayerStateMachine : MonoBehaviour
{
    PlayerInput playerInput;
    CharacterController characterController;
    PlayerActionStateMachine playerActionStateMachine;
    Animator animator;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 appliedMovement;
    Vector3 finalMovement;
    Vector3 slopeNormal;
    Vector3 slopeSlideVelocity;
    Vector3 checkForSlopeDirection;
    Vector3 ledgeGrabPoint;
    Vector3 ledgeGrabDirection;

    bool isMovementPressed = false;
    bool isSprintPressed = false;
    bool isJumpPressed = false;
    bool isJumping = false;
    bool requireNewJumpPress = false;
    bool shouldSlide = false;
    bool shouldLedgeGrab = false;
    bool blockMovement = false;
    bool rotateTowardsCameraForward = false;

    float jumpBufferTimer = 0f;
    float coyoteTimer = 0f;
    bool wasGroundedLastFrame = false;
    bool isGrabbingStarted = false;

    int isSprintingHash;
    int isWalkingHash;
    int isJumpingHash;
    int isLedgeGrabbingHash;
    int isFallingHash;
    int isWallSlidingHash;
    int jumpCountHash;
    int jumpCount = 0;

    float initialJumpVelocity;
    float gravity = -9.81f;

    Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> jumpGravities = new Dictionary<int, float>();

    Coroutine currentJumpResetRoutine = null;

    PlayerBaseState _currentState;
    PlayerStateFactory _states;

    [Header("Movement")]
    [SerializeField] float rotationSpeed = 3f;
    [SerializeField] float baseSpeedMultiplyer = 5f;
    [SerializeField] float runSpeedMultiplyer = 10f;
    [SerializeField] float fallingSpeed = -9.81f;

    [Header("Jump")]
    [SerializeField] float maxJumpHeight = 1f;
    [SerializeField] float maxJumpTime = 0.5f;

    [Header("Jump Timing")]
    [SerializeField] float jumpBufferTime = 0.2f;
    [SerializeField] float coyoteTime = 0.1f;

    [Header("Particles")]
    [SerializeField] GameObject _dustParticles;
    [SerializeField] GameObject _landingParticles;
    [SerializeField] GameObject _dustParticlesSpawnLocation;

    //Getters and Setters

    public PlayerActionStateMachine PlayerActionStateMachine { get { return playerActionStateMachine; } set { playerActionStateMachine = value; } }
    public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
    public bool IsJumpPressed { get { return isJumpPressed; } }
    public bool CanJump { get { return (jumpBufferTimer > 0f) && (characterController.isGrounded || coyoteTimer > 0f) && !requireNewJumpPress; } }
    public Coroutine CurrentJumpResetRoutine { get { return currentJumpResetRoutine; } set { currentJumpResetRoutine = value; } }
    public int JumpCount { get { return jumpCount; } set { jumpCount = value; } }
    public Animator Animator { get { return animator; } }
    public bool RequireNewJumpPress { get { return requireNewJumpPress; } set { requireNewJumpPress = value; } }
    public bool IsJumping { get { return isJumping; } set { isJumping = value; } }
    public bool IsGrabbingStarted { get { return isGrabbingStarted; } set { isGrabbingStarted = value; } }
    public bool RotateTorwardsCamera { get { return rotateTowardsCameraForward; } set { rotateTowardsCameraForward = value; } }
    public int IsJumpingHash { get { return isJumpingHash; } }
    public int JumpCountHash { get { return jumpCountHash; } }
    public int IsSprintingHash { get { return isSprintingHash; } }
    public int IsLedgeGrabbingHash { get { return isLedgeGrabbingHash; } }
    public int IsWallSlidingHash { get { return isWallSlidingHash; } }
    public int IsWalkingHash { get { return isWalkingHash; } }
    public int IsFallingHash { get { return isFallingHash; } }
    public float CurrentMovementInputX { get { return currentMovementInput.x; } set { currentMovementInput.x = value; } }
    public float CurrentMovementInputY { get { return currentMovementInput.y; } set { currentMovementInput.y = value; } }
    public float CurrentMovementX { get { return currentMovement.x; } set { currentMovement.x = value; } }
    public float CurrentMovementY { get { return currentMovement.y; } set { currentMovement.y = value; } }
    public float CurrentMovementZ { get { return currentMovement.z; } set { currentMovement.z = value; } }
    public float AppliedMovementX { get { return appliedMovement.x; } set { appliedMovement.x = value; } }
    public float AppliedMovementY { get { return appliedMovement.y; } set { appliedMovement.y = value; } }
    public float AppliedMovementZ { get { return appliedMovement.z; } set { appliedMovement.z = value; } }
    public float FallingSpeed { get { return fallingSpeed; } set { fallingSpeed = value; } }
    public Vector3 CurrentMovement { get { return currentMovement; } set { currentMovement = value; } }
    public Vector3 LedgeGrabPoint { get { return ledgeGrabPoint; } set { ledgeGrabPoint = value; } }
    public Vector3 CheckForSlopeDirection { get { return checkForSlopeDirection; } set { checkForSlopeDirection = value; } }
    public Vector3 LedgeGrabDirection { get { return ledgeGrabDirection; } set { ledgeGrabDirection = value; } }
    public Vector3 AppliedMovement { get { return appliedMovement; } set { appliedMovement = value; } }
    public Vector3 FinalMovement { get { return finalMovement; } set { finalMovement = value; } }
    public Vector3 SlopeSlideVelocity { get { return slopeSlideVelocity; } set { slopeSlideVelocity = value; } }
    public Vector3 SlopeNormal { get { return slopeNormal; } set { slopeNormal = value; } }
    public Dictionary<int, float> InitialJumpVelocities { get { return initialJumpVelocities; } }
    public Dictionary<int, float> JumpGravities { get { return jumpGravities; } }
    public CharacterController CharacterController { get { return characterController; } }
    public bool IsMovementPressed { get { return isMovementPressed; } set { isMovementPressed = value; } }
    public bool ShouldSlide { get { return shouldSlide; } }
    public bool ShouldLedgeGrab { get { return shouldLedgeGrab; } }
    public bool IsSprintPressed { get { return isSprintPressed; } set { isSprintPressed = value; } }
    public bool BlockMovement { get { return blockMovement; } set { blockMovement = value; } }
    public float RunSpeedMultiplier { get { return runSpeedMultiplyer; } }
    public float BaseSpeedMultiplier { get { return baseSpeedMultiplyer; } }
    public float Gravity { get { return gravity; } }
    public GameObject DustParticles { get { return _dustParticles; } set { _dustParticles = value; } }
    public GameObject LandingParticles { get { return _landingParticles; } set { _landingParticles = value; } }
    public GameObject DustParticlesSpawnLocation { get { return _dustParticlesSpawnLocation; } set { _dustParticlesSpawnLocation = value; } }

    public bool isMovementRelativeToCamera { get; set; } = true;
    public bool canRotate { get; set; } = true;

    public string CurrentMovementState
    {
        get
        {
            if (_currentState is PlayerGroundedState groundedState)
            {
                return "grounded";
            }
            else if (_currentState is PlayerJumpState)
            {
                return "jumping";
            }
            else if (_currentState is PlayerWallSlidingState)
            {
                return "sliding";
            }

            return "unknown";
        }
    }

    public void ConsumeJumpBuffer()
    {
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
    }

    void UpdateJumpTimers()
    {
        if (jumpBufferTimer > 0f)
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (wasGroundedLastFrame && !characterController.isGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else if (characterController.isGrounded)
        {
            coyoteTimer = 0f;
        }
        else if (coyoteTimer > 0f)
        {
            coyoteTimer -= Time.deltaTime;
        }

        wasGroundedLastFrame = characterController.isGrounded;
    }

    void SetupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;

        float initialGravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;

        float secondJumpGravity = (-2 * (maxJumpHeight + 1f)) / Mathf.Pow(timeToApex + 0.15f, 2);
        float secondJumpInitialVelocity = (2 * (maxJumpHeight + 1f)) / (timeToApex + 0.15f);

        float thirdJumpGravity = (-2 * (maxJumpHeight + 1.5f)) / Mathf.Pow(timeToApex + 0.2f, 2);
        float thirdJumpInitialVelocity = (2 * (maxJumpHeight + 1.5f)) / (timeToApex + 0.2f);

        initialJumpVelocities.Add(1, initialJumpVelocity);
        initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        jumpGravities.Add(0, initialGravity);
        jumpGravities.Add(1, initialGravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJumpGravity);
    }

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        playerActionStateMachine = GetComponent<PlayerActionStateMachine>();
        animator = GetComponentInChildren<Animator>();

        isSprintingHash = Animator.StringToHash("isSprinting");
        isWalkingHash = Animator.StringToHash("isWalking");
        isJumpingHash = Animator.StringToHash("isJumping");
        isLedgeGrabbingHash = Animator.StringToHash("isLedgeGrabbing");
        isFallingHash = Animator.StringToHash("isFalling");
        jumpCountHash = Animator.StringToHash("jumpCount");
        isWallSlidingHash = Animator.StringToHash("isWallSliding");

        checkForSlopeDirection = Vector3.down * 5f;

        _states = new PlayerStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();

        playerInput.CharachterControls.Move.started += OnMovement;
        playerInput.CharachterControls.Move.canceled += OnMovement;
        playerInput.CharachterControls.Move.performed += OnMovement;

        playerInput.CharachterControls.Sprint.started += OnSprint;
        playerInput.CharachterControls.Sprint.canceled += OnSprint;

        playerInput.CharachterControls.Jump.started += OnJump;
        playerInput.CharachterControls.Jump.canceled += OnJump;

        isMovementRelativeToCamera = true;

        SetupJumpVariables();
    }

    

    void HandleRotation()
    {
        if (!canRotate || isGrabbingStarted || blockMovement)
        {
            return;
        }

        Vector3 directionToLookAt;

        if (rotateTowardsCameraForward)
        {
            // Ruota il player verso la direzione della camera, solo sul piano XZ (Y fisso)
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            directionToLookAt = camForward.normalized;
        }
        else if (!isMovementPressed) return;
        else
        {
            // Ruota il player verso la direzione del movimento finale (di default)
            directionToLookAt = new Vector3(finalMovement.x, 0, finalMovement.z);

            if (directionToLookAt.sqrMagnitude < 0.01f) return;
        }

        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(directionToLookAt);

        transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
    }


    void Start()
    {
        CharacterController.Move(AppliedMovement * Time.deltaTime);
    }

    void Update()
    {
        UpdateJumpTimers();
        HandleRotation();
        checkForSlope();
        CheckForLedgeGrab();

        _currentState.UpdateStates();

        if (blockMovement) return;

        finalMovement = isMovementRelativeToCamera
        ? ConvertFromWorldToCameraSpace(appliedMovement)
        : appliedMovement;

        characterController.Move(finalMovement * Time.deltaTime);
    }

    void OnMovement(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        isMovementPressed = (currentMovement.x != 0 || currentMovement.z != 0);
    }

    void OnSprint(InputAction.CallbackContext context)
    {
        isSprintPressed = context.ReadValueAsButton();
    }

    void OnJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();

        if (context.started)
        {
            jumpBufferTimer = jumpBufferTime;
            requireNewJumpPress = false;
        }
    }

    private void OnEnable()
    {
        playerInput.CharachterControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.CharachterControls.Disable();
    }

    Vector3 ConvertFromWorldToCameraSpace(Vector3 vectorToRotate)
    {
        float previousY = vectorToRotate.y;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 cameraForwardZ = vectorToRotate.z * cameraForward;
        Vector3 cameraForwardX = vectorToRotate.x * cameraRight;

        Vector3 rotatedVector = cameraForwardZ + cameraForwardX;

        rotatedVector.y = previousY;

        return rotatedVector;
    }

    void checkForSlope()
    {
        Debug.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + checkForSlopeDirection, Color.red);

        shouldSlide = false;

        if (Physics.Raycast(transform.position + Vector3.up, checkForSlopeDirection, out RaycastHit hitInfo, 5, ~0))
        {
            float angle = Vector3.Angle(hitInfo.normal, Vector3.up);

            if (angle > characterController.slopeLimit)
            {
                shouldSlide = true;
                slopeNormal = hitInfo.normal;
                return;
            }

        }
    }

    void CheckForLedgeGrab()
    {
        shouldLedgeGrab = false;

        if(!characterController.isGrounded && AppliedMovement.y < 0f){
            RaycastHit downHit;
            Vector3 lineDownStart = (transform.position + Vector3.up * 1.5f) + (transform.forward * 0.8f);
            Vector3 lineDownEnd = (transform.position + Vector3.up * 0.7f) + (transform.forward * 0.8f);
            Physics.Linecast(lineDownStart, lineDownEnd, out downHit, ~0);
            Debug.DrawLine(lineDownStart, lineDownEnd, Color.red);
            if(downHit.collider != null)
            {
                RaycastHit fwdHit;
                Vector3 lineFwdStart = new Vector3(transform.position.x, downHit.point.y - 0.1f,  transform.position.z);
                Vector3 lineFwdEnd = new Vector3(transform.position.x, downHit.point.y - 0.1f, transform.position.z) + transform.forward;
                Physics.Linecast(lineFwdStart, lineFwdEnd, out fwdHit, ~0);
                Debug.DrawLine(lineFwdStart, lineFwdEnd, Color.blue);
                if(fwdHit.collider != null)
                {
                    Vector3 rawPoint = new Vector3(fwdHit.point.x, downHit.point.y, fwdHit.point.z);
                    Vector3 offset = transform.forward * -0.3f + transform.up * -1.2f;
                    ledgeGrabPoint = rawPoint + offset;
                    ledgeGrabDirection = -fwdHit.normal;

                    Debug.DrawRay(ledgeGrabPoint, Vector3.up * 0.5f, Color.yellow, 2f);

                    shouldLedgeGrab = true;

                }
            }
        }
    }

    public void ForceInputRefresh()
    {
        Vector2 currentInput = playerInput.CharachterControls.Move.ReadValue<Vector2>();
        bool currentMovementPressed = (currentInput.x != 0 || currentInput.y != 0);
        bool currentSprintPressed = playerInput.CharachterControls.Sprint.ReadValue<float>() > 0.5f;

        
        isMovementPressed = currentMovementPressed;
        isSprintPressed = currentSprintPressed;

       
        currentMovementInput = currentInput;
        currentMovement.x = currentInput.x;
        currentMovement.z = currentInput.y;

    }

    [System.Obsolete]
    public void SpawnLandingParticles()
    {
        ParticleManager.Instance.SpawnParticle(_landingParticles, _dustParticlesSpawnLocation.transform.position, Quaternion.identity);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), "IsGrabbingStarted " + isGrabbingStarted);
        GUI.Label(new Rect(10, 30, 300, 20), "Can rotate: " + canRotate);
        GUI.Label(new Rect(10, 50, 300, 20), "Current Sub State: " + _currentState._currentSubState);
    }
}