using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Promo", menuName = "ScriptableObjects/PromoCode")]
public class Promo_SO : ScriptableObject
{
    public string code;

    public float amount;

    public PromoType type;

    public string message;

}

public enum PromoType
{
    Percent,
    Flat,
    Shipping
}
