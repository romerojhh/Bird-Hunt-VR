using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pistol : Weapon
{
    [SerializeField] private Projectile bulletPrefab;
    [SerializeField] private AudioSource _gunSound;
    [SerializeField] private ParticleSystem particleSystem;
    
    protected override void StartShooting(ActivateEventArgs arg0)
    {
        // base.StartShooting(arg0);
        Shoot();
    }

    public override void Shoot()
    {
        // TODO: Make the trigger animated (?)
        // GameObject trigger = this.transform.Find("Trigger").gameObject;
        // Debug.Log(trigger.name);
        _gunSound.Play();
        
        base.Shoot();
        Projectile projectileInstance = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
        projectileInstance.Init(this);
        projectileInstance.Launch();
        // ParticleSystem effect = Instantiate()
        particleSystem.Stop();
        particleSystem.Play();
    }

    protected override void StopShooting(DeactivateEventArgs arg0)
    {
        
    }
}
