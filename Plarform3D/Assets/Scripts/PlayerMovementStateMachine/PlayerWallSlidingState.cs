using UnityEngine;
using System.Collections;

public class PlayerWallSlidingState : PlayerBaseState, IRootState
{
    Coroutine slideParticleSpawner;

    public PlayerWallSlidingState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    public override void CheckSwitchStates()
    {
        if (_ctx.CharacterController.isGrounded && !_ctx.ShouldSlide)
        {
            var grounded = _factory.Grounded();
            SwitchState(grounded);
        }
        else if (!_ctx.ShouldSlide)
        {
            SwitchState(_factory.Fall());
        }
        else if (_ctx.CanJump)
        {
            _ctx.ConsumeJumpBuffer();
        }
    }


    public override void EnterState()
    {
        InitializeSubState();

        _ctx.isMovementRelativeToCamera = false;

        slideParticleSpawner = _ctx.StartCoroutine(SpawnSlideParticles());
        _ctx.Animator.SetBool(_ctx.IsWallSlidingHash, true);
        _ctx.SlopeSlideVelocity = Vector3.ProjectOnPlane(new Vector3(0, _ctx.Gravity, 0), _ctx.SlopeNormal);
        Vector3 forwardPlanevector = new Vector3(-_ctx.SlopeNormal.x, 0, -_ctx.SlopeNormal.z);
        _ctx.CheckForSlopeDirection = (Vector3.down + 0.2f * forwardPlanevector) * 5;
        _ctx.CurrentMovementY = 0f;

        SoundManager.PlaySound3D(SoundType.Slide, _ctx.transform.position, 1);


    }

    public override void ExitState()
    {
        _ctx.isMovementRelativeToCamera = true;

        if (slideParticleSpawner != null)
            _ctx.StopCoroutine(slideParticleSpawner);

        _ctx.SlopeSlideVelocity = new Vector3(0, 0, 0);

        _ctx.Animator.SetBool(_ctx.IsWallSlidingHash, false);

    }

    public override void InitializeSubState()
    {
        
    }

    public override void UpdateState()
    {
        CheckSwitchStates();

        if (_ctx.SlopeSlideVelocity.magnitude > 1)
        {
            _ctx.SlopeSlideVelocity -= _ctx.SlopeSlideVelocity * Time.deltaTime * 2f;
        }

        _ctx.AppliedMovement = _ctx.SlopeSlideVelocity;
        _ctx.AppliedMovementY = _ctx.Gravity;

        Vector3 wallForward = _ctx.SlopeNormal;
        wallForward.y = 0f;
        if (wallForward != Vector3.zero )
        {
            Quaternion targetRotation = Quaternion.LookRotation(wallForward);
            _ctx.transform.rotation = Quaternion.Slerp(_ctx.transform.rotation, targetRotation, Time.deltaTime * 2f);
        }
    }

    public void HandleGravity()
    {

    }

    [System.Obsolete]
    IEnumerator SpawnSlideParticles()
    {
        while (true)
        {
            if (_ctx.CharacterController.isGrounded)
            {
                ParticleManager.Instance.SpawnParticle(_ctx.DustParticles, _ctx.DustParticlesSpawnLocation.transform.position, _ctx.DustParticlesSpawnLocation.transform.rotation);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}