using Gameplay.Enemies;
using UnityEngine;
using Util;

public class ShootingState : BaseHandCanonState
{
    private CharacterController _controller;

    public ShootingState(HandCannon handCannon) : base(handCannon)
    {
        controller = handCannon.actor.GetComponent<CharacterController>();
    }

    public override void EnterState()
    {
        if (beam || beamObject) return;
        handCannon.muzzleFlash.SetActive(false);
        base.EnterState();
        RequestLaunch();
    }

    public override void ExitState()
    {
        base.ExitState();
        launchRequested = false;
    }


    private void RequestLaunch()
    {
        launchRequested = true;
    }

    private bool launchRequested;

    public override void FixedUpdate()
    {
        if (launchRequested)
        {
            if (handCannon.soloCannon && handCannon.blasterElement != ElementFlag.Electricity && 
                handCannon.blasterElement != ElementFlag.Fire && handCannon.blasterElement != ElementFlag.Water &&
                handCannon.blasterElement != ElementFlag.Wind && handCannon.blasterElement != ElementFlag.Rock)
            {
                beamObject = ElementPool.GetElement(handCannon.blasterElement, handCannon.barrelTransform.position);
                beam = true;
                launchRequested = false;
                beamObject.SetActive(true);
                return;
            }
            
            var ball = ElementPool.GetElement(handCannon.blasterElement, handCannon.barrelTransform.position);
            LaunchDodgeball(ball);
            handCannon.muzzleFlash.SetActive(true);
            handCannon.audioSource.PlayOneShot(ConfigurationManager.GetBlasterSound());
            launchRequested = false;
            ChangeState(CannonState.Idle);
        }
    }

    private bool beam;
    private GameObject beamObject;
    private CharacterController controller;
    public override void Update()
    {
        base.Update();
        if (!beam) return;
        beamObject.transform.position = handCannon.barrelTransform.position;
        beamObject.transform.rotation = handCannon.barrelTransform.rotation;
    }

    public override void FireReleaseAction()
    {
        base.FireReleaseAction();
        beamObject.SetActive(false);
        beamObject = null;
        beam = false;
        ChangeState(CannonState.Idle);
    }

    private void LaunchDodgeball(GameObject dodgeball)
    {
        Rigidbody rb = dodgeball.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        dodgeball.transform.position = handCannon.barrelTransform.position;
        dodgeball.transform.rotation = handCannon.barrelTransform.rotation;
        dodgeball.transform.position += handCannon.barrelTransform.forward * 0.5f;
        dodgeball.SetActive(true);

        var launchVelocity = handCannon.barrelTransform.forward * handCannon.launchForce;
        launchVelocity += controller.velocity;

        rb.velocity = launchVelocity;
    }
}