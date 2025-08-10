using System;
using System.Collections.Generic;

public enum PlayerStateType
{
    Idle,
    Grounded,
    Run,
    Walk,
    Jump,
    Fall,
    WallSliding,
    LedgeGrabbing
}

public class PlayerStateFactory
{
    private PlayerStateMachine _context;
    private Dictionary<PlayerStateType, PlayerBaseState> _stateCache;

    public PlayerStateFactory(PlayerStateMachine currentContext)
    {
        _context = currentContext;
        InitializeStates();
    }

    private void InitializeStates()
    {
        _stateCache = new Dictionary<PlayerStateType, PlayerBaseState>
        {
            { PlayerStateType.Idle, new PlayerIdleState(_context, this) },
            { PlayerStateType.Grounded, new PlayerGroundedState(_context, this) },
            { PlayerStateType.Run, new PlayerRunState(_context, this) },
            { PlayerStateType.Walk, new PlayerWalkState(_context, this) },
            { PlayerStateType.Jump, new PlayerJumpState(_context, this) },
            { PlayerStateType.Fall, new PlayerFallState(_context, this) },
            { PlayerStateType.WallSliding, new PlayerWallSlidingState(_context, this) },
            { PlayerStateType.LedgeGrabbing, new PlayerLedgeGrabState(_context, this) }
        };
    }

    public PlayerBaseState GetState(PlayerStateType stateType)
    {
        if (_stateCache.TryGetValue(stateType, out PlayerBaseState state))
        {
            return state;
        }

        throw new ArgumentException($"State type {stateType} not found in cache");
    }

    public PlayerBaseState Idle() => GetState(PlayerStateType.Idle);
    public PlayerBaseState Grounded() => GetState(PlayerStateType.Grounded);
    public PlayerBaseState Run() => GetState(PlayerStateType.Run);
    public PlayerBaseState Walk() => GetState(PlayerStateType.Walk);
    public PlayerBaseState Jump() => GetState(PlayerStateType.Jump);
    public PlayerBaseState Fall() => GetState(PlayerStateType.Fall);
    public PlayerBaseState WallSliding() => GetState(PlayerStateType.WallSliding);
    public PlayerBaseState LedgeGrabbing() => GetState(PlayerStateType.LedgeGrabbing);
}