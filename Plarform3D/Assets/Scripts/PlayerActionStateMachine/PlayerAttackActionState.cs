using UnityEngine;

public class PlayerAttackActionState : PlayerActionBaseState
{
    private int _currentAttackLayer = 1;

    public PlayerAttackActionState(PlayerActionStateMachine currentContext, PlayerActionStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    public override void EnterState()
    {
        _currentAttackLayer = DetermineAttackLayer();

        int otherLayer = _currentAttackLayer == 1 ? 2 : 1;
        _ctx.Animator.SetLayerWeight(otherLayer, 0f);
        _ctx.Animator.SetLayerWeight(_currentAttackLayer, 1f);

        InitializeSubState();
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        _ctx.Animator.SetBool(_ctx.LeftPunchHash, false);
        _ctx.Animator.SetBool(_ctx.RightPunchHash, false);
        _ctx.Animator.SetLayerWeight(_currentAttackLayer, 0f);
    }

    public override void InitializeSubState()
    {
        SetSubState(_factory.PunchLeft());
    }

    public override void CheckSwitchStates()
    {
        if (_currentSubState == null && !_ctx.HasBufferedInput && !_ctx.IsAttackPressed)
        {
            SwitchState(_factory.Idle());
        }
    }

    private int DetermineAttackLayer()
    {
        if (_ctx.PlayerStateMachine != null)
        {
            bool isMoving = _ctx.PlayerStateMachine.IsMovementPressed;
            return isMoving ? 2 : 1;
        }
        return 1;
    }
}