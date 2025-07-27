using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerJumpState : PlayerBaseState, IRootState
{

    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) {
        _isRootState = true;
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.CharacterController.isGrounded)
        {
            SwitchState(_factory.Grounded());
        }
    }

    public override void EnterState()
    {
        InitializeSubState();
        HandleJump();
    }

    public override void ExitState()
    {
        _ctx.Animator.SetBool(_ctx.IsJumpingHash, false);
        if (_ctx.IsJumpPressed)
        {
            _ctx.RequireNewJumpPress = true;
        }
        _ctx.CurrentJumpResetRoutine = _ctx.StartCoroutine(ResetJumpTimer());
        if (_ctx.JumpCount == 3)
        {
            _ctx.JumpCount = 0;
            _ctx.Animator.SetInteger(_ctx.JumpCountHash, _ctx.JumpCount);
        }

        if (_ctx.CharacterController.isGrounded && !_ctx.ShouldSlide)
        {
            _ctx.Invoke("SpawnLandingParticles", 0.05f);
        }
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

    void HandleJump()
    {
        if (_ctx.CurrentJumpResetRoutine != null && _ctx.JumpCount < 3)
        {
            _ctx.StopCoroutine(_ctx.CurrentJumpResetRoutine);
        }

        _ctx.JumpCount++;
        if (_ctx.JumpCount > 3)
        {
            _ctx.JumpCount = 1;
        }

        _ctx.Animator.SetBool(_ctx.IsJumpingHash, true);
        _ctx.Animator.SetInteger(_ctx.JumpCountHash, _ctx.JumpCount);
        _ctx.IsJumping = true;
        _ctx.CurrentMovementY = _ctx.InitialJumpVelocities[_ctx.JumpCount];
        _ctx.AppliedMovementY = _ctx.InitialJumpVelocities[_ctx.JumpCount];
    }

    IEnumerator ResetJumpTimer()
    {
        yield return new WaitForSeconds(.5f);
        _ctx.JumpCount = 0;
    }

    public void HandleGravity()
    {
        bool isFalling = _ctx.CurrentMovementY <= 0.0f || !_ctx.IsJumpPressed;
        float fallingSpeed = 1.2f;

        if (isFalling)
        {
            float oldVelocityVal = _ctx.CurrentMovementY;
            _ctx.CurrentMovementY = _ctx.CurrentMovementY + (_ctx.JumpGravities[_ctx.JumpCount] * fallingSpeed * Time.deltaTime);
            _ctx.AppliedMovementY = Mathf.Max((oldVelocityVal + _ctx.CurrentMovementY) * 0.5f, -20.0f);
        }
        else
        {
            float oldVelocityVal = _ctx.CurrentMovementY;
            _ctx.CurrentMovementY = _ctx.CurrentMovementY + (_ctx.JumpGravities[_ctx.JumpCount] * Time.deltaTime);
            _ctx.AppliedMovementY = (oldVelocityVal + _ctx.CurrentMovementY) * 0.5f;

        }
    }

}
