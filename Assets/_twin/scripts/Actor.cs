using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour
{
    public Weapon[] weapons = new Weapon[0];

    public Weapon CurrentWeapon()
    {
        if (currentWeapon != null)
            return currentWeapon;
        if (weapons.Length > 0)
            return weapons[0];
        return null;
    }

    Weapon currentWeapon;
}

