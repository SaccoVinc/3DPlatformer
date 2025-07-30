using UnityEngine;
using System.Collections;

public class PlayerGroundedState : PlayerBaseState, IRootState
{
    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    public string CurrentSubStateType
    {
        get
        {
            if (_currentSubState is PlayerIdleState) return "idle";
            if (_currentSubState is PlayerWalkState) return "walk";
            if (_currentSubState is PlayerRunState) return "run";
            return "unknown";
        }
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.CanJump && !_ctx.IsGrabbingStarted)
        {
            _ctx.ConsumeJumpBuffer();
            SwitchState(_factory.Jump());
        }

        if (_ctx.ShouldSlide)
        {
            SwitchState(_factory.WallSliding());
        }
    }

    public void HandleGravity()
    {
        _ctx.AppliedMovementY = _ctx.Gravity;
        _ctx.CurrentMovementY = _ctx.Gravity;
    }

    public override void EnterState()
    {
        _ctx.isMovementRelativeToCamera = true;
        _ctx.CheckForSlopeDirection = Vector3.down * 5f;

        HandleGravity();

        if (!_ctx.IsMovementPressed)
        {
            _ctx.AppliedMovementX = 0f;
            _ctx.AppliedMovementZ = 0f;
        }

        // Usa una coroutine per ritardare l'inizializzazione del substate
        // Questo garantisce che gli input siano completamente aggiornati
        _ctx.StartCoroutine(DelayedSubstateInitialization());

    }

    public override void InitializeSubState()
    {
        if (!_ctx.IsMovementPressed && !_ctx.IsSprintPressed)
        {
            SetSubState(_factory.Idle());
        }
        else if (!_ctx.IsSprintPressed && _ctx.IsMovementPressed)
        {
            SetSubState(_factory.Walk());
        }
        else
        {
            SetSubState(_factory.Run());
        }
    }

    public override void ExitState()
    {

    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    private IEnumerator DelayedSubstateInitialization()
    {
        yield return null;

        // Forza un refresh degli input
        _ctx.ForceInputRefresh();

        // Ora inizializza il substate con gli input freschi
        InitializeSubState();

    }
}