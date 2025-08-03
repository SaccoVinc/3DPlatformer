public abstract class PlayerActionBaseState
{
    protected bool _isRootState = false;
    protected PlayerActionStateMachine _ctx;
    protected PlayerActionStateFactory _factory;
    protected PlayerActionBaseState _currentSuperState;
    protected PlayerActionBaseState _currentSubState;

    public PlayerActionBaseState(PlayerActionStateMachine ctx, PlayerActionStateFactory factory)
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

    protected void SetSuperState(PlayerActionBaseState newSuperState)
    {
        _currentSuperState = newSuperState;
    }

    public void SetSubState(PlayerActionBaseState newSubState)
    {
       
        if (_currentSubState != null)
        {
            _currentSubState.ExitStates();
        }

        _currentSubState = newSubState;
        newSubState.SetSuperState(this);

       
        newSubState.EnterState();
    }

    public void SwitchState(PlayerActionBaseState newState)
    {

        ExitStates();

        newState.EnterState();

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