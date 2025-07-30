using System;
using System.Collections.Generic;

public enum PlayerActionStateType
{
    Idle,
    Attack,
    PunchLeft,
    PunchRight,
    Grab
}

public class PlayerActionStateFactory
{
    private PlayerActionStateMachine _context;
    private Dictionary<PlayerActionStateType, PlayerActionBaseState> _stateCache;

    public PlayerActionStateFactory(PlayerActionStateMachine currentContext)
    {
        if (currentContext == null)
            throw new ArgumentNullException(nameof(currentContext), "Context cannot be null");

        _context = currentContext;
        InitializeStates();
    }

    private void InitializeStates()
    {
        _stateCache = new Dictionary<PlayerActionStateType, PlayerActionBaseState>
        {
            { PlayerActionStateType.Idle, new PlayerIdleActionState(_context, this) },
            { PlayerActionStateType.Grab, new PlayerGrabActionState(_context, this) },
            { PlayerActionStateType.Attack, new PlayerAttackActionState(_context, this) },
            { PlayerActionStateType.PunchLeft, new PlayerPunchLeftState(_context, this) },
            { PlayerActionStateType.PunchRight, new PlayerPunchRightState(_context, this) },
        };
    }

    public PlayerActionBaseState GetState(PlayerActionStateType stateType)
    {
        if (_stateCache.TryGetValue(stateType, out PlayerActionBaseState state))
        {
            return state;
        }

        throw new ArgumentException($"State type {stateType} not found in cache");
    }

    public PlayerActionBaseState Idle() => GetState(PlayerActionStateType.Idle);
    public PlayerActionBaseState Grab() => GetState(PlayerActionStateType.Grab);
    public PlayerActionBaseState PunchLeft() => GetState(PlayerActionStateType.PunchLeft);
    public PlayerActionBaseState PunchRight() => GetState(PlayerActionStateType.PunchRight);
    public PlayerActionBaseState Attack() => GetState(PlayerActionStateType.Attack);
}