using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected Weapon weapon;

    public virtual void Init(Weapon weapon)
    {
        this.weapon = weapon;
    }

    public Weapon GetWeapon()
    {
        return weapon;
    }

    public abstract void Launch();
}
