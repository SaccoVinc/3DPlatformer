using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerActionStateMachine : MonoBehaviour
{
    PlayerInput playerInput;
    Animator animator;
    CharacterController characterController;

    PlayerStateMachine _playerStateMachine;
    PlayerActionBaseState _currentState;
    PlayerActionStateFactory _states;

    // Hash per le animazioni
    int _leftPunchHash;
    int _rightPunchHash;
    int _grabHash;

    // Stati input
    bool _isAttackPressed = false;
    bool _isGrabPressed = false;
    bool _isInGrabState = false;
    bool _requireNewAttackPress = false;
    bool _hasBufferedInput = false;

    [Header("Attack Settings")]
    [SerializeField] float AttackDamage = 3f;
    [SerializeField] float _attacksDuration = 0.25f;

    [Header("Animation Settings")]
    [SerializeField] float _layerWeightTransitionSpeed = 5f;

    [Header("Dash Settings")]
    [SerializeField] float _dashForce = 5f;
    [SerializeField] float _dashDuration = 0.1f;

    [Header("Debug Settings")]
    [SerializeField] bool _showDebugGUI = true;

    [Header("Shooting Settings")]
    [SerializeField] int _shootingLayerId = 4;
 
    private Coroutine _layerWeightCoroutine;
    private Coroutine _dashCoroutine;

    // Properties pubbliche
    public PlayerActionBaseState CurrentState
    {
        get => _currentState;
        set { _currentState = value; }
    }
    public PlayerStateMachine PlayerStateMachine { get => _playerStateMachine; set => _playerStateMachine = value; }
    public bool IsAttackPressed => _isAttackPressed;
    public bool IsGrabPressed => _isGrabPressed;
    public bool IsInGrabState => _isInGrabState; // Nuova property
    public bool RequireNewAttackPress { get => _requireNewAttackPress; set => _requireNewAttackPress = value; }
    public bool HasBufferedInput { get => _hasBufferedInput; set => _hasBufferedInput = value; }
    public int RightPunchHash => _rightPunchHash;
    public int LeftPunchHash => _leftPunchHash;
    public int ShootingLayerId => _shootingLayerId;
    public int GrabHash => _grabHash;
    public Animator Animator => animator;
    
    public PlayerActionStateFactory States => _states;
    public CharacterController CharacterController => characterController;
    public float AttacksDuration { get => _attacksDuration; set => _attacksDuration = value; }

    void Awake()
    {
        playerInput = new PlayerInput();
        animator = GetComponentInChildren<Animator>();
        _playerStateMachine = GetComponent<PlayerStateMachine>();
        characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogError("CharacterController not found on " + gameObject.name);
        }

        _rightPunchHash = Animator.StringToHash("rightPunch");
        _leftPunchHash = Animator.StringToHash("leftPunch");
        _grabHash = Animator.StringToHash("isGrabbing");

        _states = new PlayerActionStateFactory(this);
        _currentState = _states.Idle();
        _currentState.EnterState();

        playerInput.CharachterControls.Attack.performed += OnAttack;
        playerInput.CharachterControls.Attack.canceled += OnAttack;

        playerInput.CharachterControls.Grab.performed += OnGrab;
        playerInput.CharachterControls.Grab.canceled += OnGrab;
    }
    void Update()
    {
        _currentState.UpdateStates();

        _isInGrabState = _currentState is PlayerGrabActionState;

        if (!_isAttackPressed && _requireNewAttackPress)
        {
            _requireNewAttackPress = false;
        }
    }

    void LateUpdate()
    {
        if (animator != null && animator.transform != transform)
        {
            animator.transform.position = transform.position;
            animator.transform.rotation = transform.rotation;
        }
    }



    void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isAttackPressed = true;
        }
        else if (context.canceled)
        {
            _isAttackPressed = false;
        }
    }

    void OnGrab(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isGrabPressed = true;
        }
        else if (context.canceled)
        {
            _isGrabPressed = false;
        }
    }

    

    public void OnPunchDashStart()
    {
        StartDash();
    }

    public void OnPunchDashEnd()
    {
        StopDash();
    }

    private void StartDash()
    {
        if (_dashCoroutine != null)
        {
            StopCoroutine(_dashCoroutine);
        }

        _dashCoroutine = StartCoroutine(DashForward());
    }

    private void StopDash()
    {
        if (_dashCoroutine != null)
        {
            StopCoroutine(_dashCoroutine);
            _dashCoroutine = null;
        }
    }

    private IEnumerator DashForward()
    {
        float elapsedTime = 0f;
        Vector3 dashDirection = transform.forward;

        while (elapsedTime < _dashDuration)
        {
            elapsedTime += Time.deltaTime;

            Vector3 dashMovement = dashDirection * _dashForce * Time.deltaTime;

            characterController.Move(dashMovement);

            if (animator != null && animator.transform != transform)
            {
                animator.transform.position = transform.position;
            }

            yield return null;
        }

        _dashCoroutine = null;
    }

    public void SetLayerWeightSmooth(int layerIndex, float targetWeight)
    {
        if (_layerWeightCoroutine != null)
        {
            StopCoroutine(_layerWeightCoroutine);
        }

        _layerWeightCoroutine = StartCoroutine(LerpLayerWeight(layerIndex, targetWeight));
    }

    private IEnumerator LerpLayerWeight(int layerIndex, float targetWeight)
    {
        float currentWeight = animator.GetLayerWeight(layerIndex);
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * _layerWeightTransitionSpeed;
            float newWeight = Mathf.Lerp(currentWeight, targetWeight, elapsedTime);
            animator.SetLayerWeight(layerIndex, newWeight);
            yield return null;
        }

        animator.SetLayerWeight(layerIndex, targetWeight);
        _layerWeightCoroutine = null;
    }

    private void OnEnable()
    {
        playerInput.CharachterControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.CharachterControls.Disable();

        if (_layerWeightCoroutine != null)
        {
            StopCoroutine(_layerWeightCoroutine);
        }

        if (_dashCoroutine != null)
        {
            StopCoroutine(_dashCoroutine);
        }
    }
}