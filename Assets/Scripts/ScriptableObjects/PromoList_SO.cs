using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PromoList", menuName = "ScriptableObjects/PromoList")]
public class PromoList_SO : ScriptableObject
{
    public PromoContainer content;
}

[Serializable]
public class PromoContainer
{
    public List<Promo_SO> list;
}
