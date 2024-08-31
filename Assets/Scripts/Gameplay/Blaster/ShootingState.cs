using System.Linq;
using Gameplay;
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
            LaunchDodgeball(ElementPool.GetElement(handCannon.blasterElement, handCannon.barrelTransform.position));
            handCannon.muzzleFlash.SetActive(true);
            handCannon.audioSource.PlayOneShot(ConfigurationManager.GetBlasterSound());
            launchRequested = false;
            ChangeState(CannonState.Idle);
        }
        else if (launchRequested) ChangeState(CannonState.Idle);
    }

    private CharacterController controller;

    private void LaunchDodgeball(GameObject dodgeball)
    {
        // Get the Rigidbody and ensure it is ready for physics interactions
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