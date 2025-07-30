using UnityEngine;

public class PlayerFallState : PlayerBaseState, IRootState
{
    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.CharacterController.isGrounded)
        {
            SwitchState(_factory.Grounded());
            SoundManager.PlaySound3D(SoundType.Step, _ctx.transform.position, 1);
        }
        else if (_ctx.CanJump)
        {
            _ctx.ConsumeJumpBuffer();
            SwitchState(_factory.Jump());
        }
        else if (_ctx.ShouldSlide)
        {
            SwitchState(_factory.WallSliding());
        }
    }

    public override void EnterState()
    {
        InitializeSubState();
        _ctx.Animator.SetBool(_ctx.IsFallingHash, true);
    }

    public override void ExitState()
    {
        _ctx.Animator.SetBool(_ctx.IsFallingHash, false);
    }

    public void HandleGravity()
    {
        float previousYVelocity = _ctx.CurrentMovementY;
        _ctx.CurrentMovementY = _ctx.CurrentMovementY + (_ctx.Gravity * Time.deltaTime);
        _ctx.AppliedMovementY = Mathf.Max((previousYVelocity + _ctx.CurrentMovementY) * 0.5f, -20.0f);
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

    public override void UpdateState()
    {
        HandleGravity();
        CheckSwitchStates();
    }
}