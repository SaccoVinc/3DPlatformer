using UnityEngine;

public class PlayerPaintGunActionState : PlayerActionBaseState
{
    private int _currentAttackLayer = 1;

    public PlayerPaintGunActionState(PlayerActionStateMachine currentContext, PlayerActionStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    public override void EnterState()
    {
        _ctx.Animator.SetLayerWeight(_ctx.ShootingLayerId, 1f);

        InitializeSubState();
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public override void ExitState()
    {
        _ctx.Animator.SetLayerWeight(_ctx.ShootingLayerId, 0f);
    }

    public override void InitializeSubState()
    {

    }

    public override void CheckSwitchStates()
    {
        
    }
}