using System.Linq;
using Gameplay;
using UnityEngine;

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
        if (launchRequested && handCannon.dodgeBallAmmo.Count > 0)
        {
            if (handCannon.dodgeBallAmmo.Count > 0)
            {
                LaunchDodgeball(handCannon.dodgeBallAmmo.FirstOrDefault());
                handCannon.dodgeBallAmmo.RemoveAt(0);
            }

            handCannon.muzzleFlash.SetActive(true);
            handCannon.audioSource.PlayOneShot(ConfigurationManager.GetBlasterSound());
            handCannon.ChangeState(CannonState.Idle);
            launchRequested = false;
            ChangeState(CannonState.Idle);
        }
        else if (launchRequested) ChangeState(CannonState.Idle);
    }

    private CharacterController controller;

    private void LaunchDodgeball(Projectile dodgeball)
    {
        // Get the Rigidbody and ensure it is ready for physics interactions
        Rigidbody rb = dodgeball.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        dodgeball.transform.position = handCannon.barrelTransform.position;
        dodgeball.transform.rotation = handCannon.barrelTransform.rotation;
        dodgeball.transform.position += handCannon.barrelTransform.forward * 0.5f;
        dodgeball.gameObject.SetActive(true);

        var launchVelocity = handCannon.barrelTransform.forward * handCannon.launchForce;
        launchVelocity += controller.velocity;

        // Apply the final velocity
        rb.velocity = launchVelocity;

        // Optionally add additional effects like spin, etc., here
    }
}