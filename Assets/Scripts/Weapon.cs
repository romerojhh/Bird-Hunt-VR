using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(XRGrabInteractable))]
public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected float shootingForce;
    [SerializeField] protected Transform bulletSpawn;
    [SerializeField] private float recoilForce;
    [SerializeField] private float damage;

    private Rigidbody _rigidbody;
    private XRGrabInteractable _interactableWeapon;

    private void Awake()
    {
        _interactableWeapon = GetComponent<XRGrabInteractable>();
        _rigidbody = GetComponent<Rigidbody>();
        SetupInteractableWeaponEvents();
    }

    private void SetupInteractableWeaponEvents()
    {
        // _interactableWeapon.selectEntered.AddListener(PickupWeapon);
        // _interactableWeapon.selectExited.AddListener(DropWeapon);
        _interactableWeapon.activated.AddListener(StartShooting);
        _interactableWeapon.deactivated.AddListener(StopShooting);
    }

    protected abstract void StopShooting(DeactivateEventArgs arg0);

    protected abstract void StartShooting(ActivateEventArgs arg0);
    protected virtual void Shoot()
    {
        ApplyRecoil();
    }

    private void ApplyRecoil()
    {
        _rigidbody.AddRelativeForce(Vector3.back * recoilForce, ForceMode.Impulse);
    }

    public float GetShootingForce()
    {
        return shootingForce;
    }

    public float GetDamage()
    {
        return damage;
    }
    
}
