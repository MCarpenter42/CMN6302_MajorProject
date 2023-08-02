using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEditor;
using TMPro;

using NeoCambion;
using NeoCambion.Collections;
using NeoCambion.Collections.Unity;
using NeoCambion.Encryption;
using NeoCambion.Heightmaps;
using NeoCambion.IO;
using NeoCambion.IO.Unity;
using NeoCambion.Maths;
using NeoCambion.Maths.Matrices;
using NeoCambion.Random;
using NeoCambion.Random.Unity;
using NeoCambion.Sorting;
using NeoCambion.TaggedData;
using NeoCambion.TaggedData.Unity;
using NeoCambion.Unity;
using NeoCambion.Unity.Editor;
using NeoCambion.Unity.Events;
using NeoCambion.Unity.Geometry;
using NeoCambion.Unity.Interpolation;

public class StatusEffect
{
    public class SE_DamageOverTime
    {
        public DamageType type;
        public int value;
    }

    public enum TargetAttribute { Health, Attack, Defence, Speed }
    public class SE_AttributeModifier
    {
        public TargetAttribute attribute;
        public bool additive;
        private int valAdd;
        private float valMlt;
        public float value
        {
            get
            {
                if (additive)
                    return valAdd;
                else
                    return valMlt;
            }
            set
            {
                if (additive)
                    valAdd = Mathf.RoundToInt(value);
                else
                    valMlt = value;
            }
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public string displayName = "StatusEffect_DefaultName";
    public SE_DamageOverTime damageOverTime = null;
    public SE_AttributeModifier attributeModifier = null;

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public virtual void OnTurnStart()
    {

    }

    public virtual void OnTurnEnd()
    {

    }

    public virtual void OnApply()
    {

    }

    public virtual void OnExpire()
    {

    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */


}

/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

// FIRE --> BURN
//  - Fire DoT
// ICE --> FREEZE
//  - High % speed reduction
//  - Small % damage vulnerability
// EARTH --> RESTRAINT
//  - Earth DoT
//  - Small % speed reduction
// LIGHTNING --> SHOCK
//  - Lightning DoT
// PHYSICAL --> BLEED
//  - Physical DoT
// PSYCHIC --> CONFUSION
//  - Reduced action effectiveness
// LIGHT --> DAZZLE
//  - ???
// DARK --> BLIND
//  - Chance of 
