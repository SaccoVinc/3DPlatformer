using UnityEngine;
using System.Collections;

public class PlayerRunState : PlayerBaseState
{

    public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) { }

    public override void CheckSwitchStates()
    {
        if (!_ctx.IsMovementPressed)
        {
            SwitchState(_factory.Idle());
        }
        else if (!_ctx.IsSprintPressed && _ctx.IsMovementPressed)
        {
            SwitchState(_factory.Walk());
        }
    }

    public override void EnterState()
    {
        _ctx.Animator.SetBool(_ctx.IsWalkingHash, true);
        _ctx.Animator.SetBool(_ctx.IsSprintingHash, true);

    }

    public override void ExitState()
    {

    }

    public override void InitializeSubState()
    {

    }

    public override void UpdateState()
    {
        _ctx.AppliedMovementX = _ctx.CurrentMovementInputX * _ctx.RunSpeedMultiplier;
        _ctx.AppliedMovementZ = _ctx.CurrentMovementInputY * _ctx.RunSpeedMultiplier;
        CheckSwitchStates();
    }
}