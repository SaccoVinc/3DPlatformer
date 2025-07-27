using System.Collections;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{


    public PlayerWalkState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) { }

    public override void CheckSwitchStates()
    {
        if (!_ctx.IsMovementPressed)
        {
            SwitchState(_factory.Idle());
        }
        else if (_ctx.IsSprintPressed && _ctx.IsMovementPressed)
        {
            SwitchState(_factory.Run());
        }
    }

    public override void EnterState()
    {
        _ctx.Animator.SetBool(_ctx.IsWalkingHash, true);
        _ctx.Animator.SetBool(_ctx.IsSprintingHash, false);

    }

    public override void ExitState()
    {

    }

    public override void InitializeSubState()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateState()
    {
        _ctx.AppliedMovementX = _ctx.CurrentMovementInputX * _ctx.BaseSpeedMultiplier;
        _ctx.AppliedMovementZ = _ctx.CurrentMovementInputY * _ctx.BaseSpeedMultiplier;
        CheckSwitchStates();
    }


}