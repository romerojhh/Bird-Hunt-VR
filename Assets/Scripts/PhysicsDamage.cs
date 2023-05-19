using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsDamage : MonoBehaviour, ITakeDamage
{
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void TakeDamage(Weapon weapon, Projectile projectile, Vector3 contactPoint)
    {
        
        _rigidbody.AddForce(projectile.transform.forward * weapon.GetShootingForce(), ForceMode.Impulse);
    }
}
