using UnityEngine;
using System.Collections;

public class PlayerLedgeGrabState : PlayerBaseState, IRootState
{
    Coroutine slideParticleSpawner;

    public PlayerLedgeGrabState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.IsJumpPressed)
        {
            _ctx.ConsumeJumpBuffer();
            SwitchState(_factory.Jump());
        }
    }


    public override void EnterState()
    {
        InitializeSubState();

        _ctx.AppliedMovement = Vector3.zero;
        Vector3 target = _ctx.LedgeGrabPoint;
        Vector3 displacement = target - _ctx.transform.position;
        _ctx.CharacterController.Move(displacement);
        _ctx.transform.forward = _ctx.LedgeGrabDirection;
        SoundManager.PlaySound3D(SoundType.Land, _ctx.transform.position, 1);
        _ctx.Animator.SetBool(_ctx.IsLedgeGrabbingHash, true);

        _ctx.PlayerActionStateMachine.CanPerformActions = false;

        _ctx.BlockMovement = true;

    }

    public override void ExitState()
    {
        _ctx.Animator.SetBool(_ctx.IsLedgeGrabbingHash, false);

        _ctx.BlockMovement = false;

        _ctx.PlayerActionStateMachine.CanPerformActions = true;

        Debug.Log("Esco dal Ledge!");

    }

    public override void InitializeSubState()
    {
        if (_currentSubState != null)
        {
            _currentSubState.ExitStates();
            _currentSubState = null;
        }
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
    }

    public void HandleGravity()
    {

    }

}