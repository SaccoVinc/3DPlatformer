public abstract class PlayerBaseState
{
    protected bool _isRootState = false;
    protected PlayerStateMachine _ctx;
    protected PlayerStateFactory _factory;
    protected PlayerBaseState _currentSuperState;
    public PlayerBaseState _currentSubState;

    public PlayerBaseState(PlayerStateMachine ctx, PlayerStateFactory factory)
    {
        _ctx = ctx;
        _factory = factory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void InitializeSubState();
    public abstract void CheckSwitchStates();

    public void UpdateStates()
    {
        UpdateState();
        if (_currentSubState != null)
        {
            _currentSubState.UpdateStates();
        }
    }

    public void ExitStates()
    {
        ExitState();
        if (_currentSubState != null)
        {
            _currentSubState.ExitStates();
        }
    }

    protected void SetSuperState(PlayerBaseState newSuperState)
    {
        _currentSuperState = newSuperState;
    }

    public void SetSubState(PlayerBaseState newSubState)
    {
        // Prima esci dal substate corrente se esiste
        if (_currentSubState != null)
        {
            _currentSubState.ExitStates();
        }

        _currentSubState = newSubState;
        if (newSubState != null)
        {
            newSubState.SetSuperState(this);
        }
    }

    protected void SwitchState(PlayerBaseState newState)
    {
        // Esci dallo stato corrente e tutti i suoi substates
        ExitStates();

        // Entra nel nuovo stato
        newState.EnterState();

        // Aggiorna il riferimento appropriato
        if (_isRootState)
        {
            _ctx.CurrentState = newState;
        }
        else if (_currentSuperState != null)
        {
            _currentSuperState.SetSubState(newState);
        }
    }
}