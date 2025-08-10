using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerPaintGunActionState : PlayerActionBaseState
{
    private int _currentAttackLayer = 1;
    private bool notPlaying = true;
    bool useOrbitalFollow = true;
    bool usePanTilt = true;
    Coroutine layers;
    public PlayerPaintGunActionState(PlayerActionStateMachine currentContext, PlayerActionStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
    {
        _isRootState = true;
    }

    public override void EnterState()
    {
        InitializeSubState();
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        Vector3 angle = _ctx.PaintParticlesParent.localEulerAngles;

        if (_ctx.IsAttackPressed && notPlaying)
        {
            _ctx.PaintParticles.Play();
            notPlaying = false;
            _ctx.PlayerStateMachine.RotateTorwardsCamera = true;

            if(layers != null)
                _ctx.StopCoroutine(layers);
            layers = _ctx.StartCoroutine(LerpRigWeight(_ctx.ShootingRig, 1, 0.2f));
        }
        else if (!_ctx.IsAttackPressed)
        {
            _ctx.PaintParticles.Stop();
            notPlaying = true;
            _ctx.PlayerStateMachine.RotateTorwardsCamera = false;

            if (layers != null)
                _ctx.StopCoroutine(layers);
            layers = _ctx.StartCoroutine(LerpRigWeight(_ctx.ShootingRig, 0, 0.2f));
        }

    }

    public override void ExitState()
    {
        _ctx.StartCoroutine(LerpRigWeight(_ctx.ShootingRig, 0, .2f));
    }

    public override void InitializeSubState()
    {

    }

    public override void CheckSwitchStates()
    {
        
    }

    public IEnumerator LerpRigWeight(Rig rig,float targetWeight, float duration)
    {
        float startWeight = rig.weight;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rig.weight = Mathf.Lerp(startWeight, targetWeight, t);
            yield return null;
        }

        rig.weight = targetWeight;
    }


}