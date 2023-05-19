using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pistol : Weapon
{
    [SerializeField] private Projectile bulletPrefab;
    [SerializeField] private AudioSource _gunSound;
    
    protected override void StartShooting(ActivateEventArgs arg0)
    {
        // base.StartShooting(arg0);
        Shoot();
    }

    protected override void Shoot()
    {
        // TODO: Make the trigger animated (?)
        // GameObject trigger = this.transform.Find("Trigger").gameObject;
        // Debug.Log(trigger.name);
        _gunSound.Play();
        
        base.Shoot();
        Projectile projectileInstance = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        projectileInstance.Init(this);
        projectileInstance.Launch();
    }

    protected override void StopShooting(DeactivateEventArgs arg0)
    {
        
    }
}
