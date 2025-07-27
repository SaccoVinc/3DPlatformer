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
        // Controlla se c'è input di attacco e non è richiesta una nuova pressione
        if (_ctx.IsAttackPressed && !_ctx.RequireNewAttackPress)
        {
            SwitchState(_factory.Attack());
        }
    }

    public override void EnterState()
    {
        // Assicurati che la weight sia a 0 con lerp smooth
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