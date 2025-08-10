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
        SetAnimationLayers();
        InitializeSubState();
    }

    public override void UpdateState()
    {
        int newAttackLayer = DetermineAttackLayer();
        if (newAttackLayer != _currentAttackLayer)
        {
            _currentAttackLayer = newAttackLayer;
            SetAnimationLayers();
        }

        CheckSwitchStates();
    }

    public override void ExitState()
    {
        _ctx.Animator.SetBool(_ctx.LeftPunchHash, false);
        _ctx.Animator.SetBool(_ctx.RightPunchHash, false);
        _ctx.Animator.SetLayerWeight(1, 0f);
        _ctx.Animator.SetLayerWeight(2, 0f);
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

    private void SetAnimationLayers()
    {
        _ctx.Animator.SetLayerWeight(1, 0f);
        _ctx.Animator.SetLayerWeight(2, 0f);
        _ctx.Animator.SetLayerWeight(_currentAttackLayer, 1f);
    }
}