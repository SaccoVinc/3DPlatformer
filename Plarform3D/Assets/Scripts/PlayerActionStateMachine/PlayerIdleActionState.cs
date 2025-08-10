using UnityEngine;

public class PlayerIdleActionState : PlayerActionBaseState
{
    public PlayerIdleActionState(PlayerActionStateMachine currentContext, PlayerActionStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    public override void CheckSwitchStates()
    {
        if (!_ctx.CanPerformActions) return;

        if (_ctx.IsGrabPressed)
        {
            SwitchState(_factory.Grab());
            return;
        }

        if (_ctx.IsAttackPressed && !_ctx.RequireNewAttackPress)
        {
            SwitchState(_factory.Attack());
        }
    }

    public override void EnterState()
    {
        _ctx.SetLayerWeightSmooth(1, 0f);
    }

    public override void ExitState()
    {
    }

    public override void InitializeSubState() { }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }
}