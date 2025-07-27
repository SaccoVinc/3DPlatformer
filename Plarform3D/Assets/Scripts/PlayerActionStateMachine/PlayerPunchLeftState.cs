using UnityEngine;

public class PlayerPunchLeftState : PlayerActionBaseState
{
    float elapsedTime = 0;

    public PlayerPunchLeftState(PlayerActionStateMachine currentContext, PlayerActionStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        _isRootState = false;
    }

    public override void CheckSwitchStates()
    {
        elapsedTime += Time.deltaTime;

        // Attendi la durata dell'animazione
        if (elapsedTime >= _ctx.AttacksDuration)
        {
            _ctx.Animator.SetBool(_ctx.LeftPunchHash, false);

            if (_ctx.HasBufferedInput)
            {
                // Cambia al pugno destro rimanendo nel super-state Attack
                // USA SetSubState invece di SwitchState per rimanere nell'AttackActionState
                if (_currentSuperState != null)
                {
                    _currentSuperState.SetSubState(_factory.PunchRight());
                }
            }
            else
            {
                // Esci completamente dall'Attack state e torna a Idle
                // Qui SwitchState è corretto perché vogliamo uscire dall'AttackActionState
                if (_currentSuperState != null)
                {
                    _currentSuperState.SwitchState(_factory.Idle());
                }
            }
        }
    }

    public override void EnterState()
    {
        _ctx.Animator.SetBool(_ctx.LeftPunchHash, true);
        _ctx.RequireNewAttackPress = true;
        _ctx.HasBufferedInput = false;
        elapsedTime = 0;
    }

    public override void ExitState()
    {
        _ctx.Animator.SetBool(_ctx.LeftPunchHash, false);
    }

    public override void InitializeSubState() { }

    public override void UpdateState()
    {
        // Controlla se c'è un nuovo input di attacco
        if (_ctx.IsAttackPressed && !_ctx.RequireNewAttackPress)
        {
            _ctx.HasBufferedInput = true;
        }

        CheckSwitchStates();
    }
}