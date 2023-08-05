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
using System;

public class ActiveEffects
{
    public enum RemovalTarget { First, Last, All }

    private List<StatusEffect> Special = new List<StatusEffect>();
    private List<StatusEffect> Normal = new List<StatusEffect>();
    private List<byte> activeUIDs = new List<byte>();
    private List<byte> inactiveUIDs = new List<byte>();
    private static byte[] allUIDs = new byte[]
    {
        001, 002, 003, 004, 005, 006, 007, 008, 009, 010, 011, 012, 013, 014, 015, 016, 017, 018, 019, 020,
        021, 022, 023, 024, 025, 026, 027, 028, 029, 030, 031, 032, 033, 034, 035, 036, 037, 038, 039, 040,
        041, 042, 043, 044, 045, 046, 047, 048, 049, 050, 051, 052, 053, 054, 055, 056, 057, 058, 059, 060,
        061, 062, 063, 064, 065, 066, 067, 068, 069, 070, 071, 072, 073, 074, 075, 076, 077, 078, 079, 080,
        081, 082, 083, 084, 085, 086, 087, 088, 089, 090, 091, 092, 093, 094, 095, 096, 097, 098, 099, 120,
        100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
        121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140,
        141, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160,
        161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180,
        181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200,
        201, 202, 203, 204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220,
        221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234, 235, 236, 237, 238, 239, 240,
        241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255
    };

    public ActiveEffects()
    {
        inactiveUIDs.AddRange(allUIDs);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    private byte NewUID()
    {
        if (inactiveUIDs.Count > 1)
        {
            byte UID = inactiveUIDs[0];
            activeUIDs.Add(inactiveUIDs[0]);
            activeUIDs.RemoveAt(0);
            return UID;
        }
        return 0;
    }

    private bool DeactivateUID(byte UID)
    {
        int ind = activeUIDs.IndexOf(UID);
        if (ind >= 0)
        {
            activeUIDs.RemoveAt(ind);
            inactiveUIDs.Add(UID);
        }
        return false;
    }

    public bool Add(StatusEffect newEffect)
    {
        byte newUID;
        if (newEffect != null)
        {
            if (newEffect.special)
            {
                for (int i = 0; i < Special.Count; i++)
                {
                    if (Special[i].internalName == newEffect.internalName)
                        return false;
                }
                if ((newUID = NewUID()) > 0)
                {
                    newEffect.container = this;
                    newEffect.instanceUID = newUID;
                    Special.Add(newEffect);
                }
            }
            else
            {
                int matches = 0;
                for (int i = 0; i < Normal.Count; i++)
                {
                    if (Normal[i].internalName == newEffect.internalName)
                        matches++;
                    if (matches > 2)
                        return false;
                }
                if ((newUID = NewUID()) > 0)
                {
                    newEffect.container = this;
                    newEffect.instanceUID = newUID;
                    Normal.Add(newEffect);
                }
            }
            return true;
        }
        return false;
    }

    public bool Remove(byte instanceUID, bool special = false)
    {
        int i;
        if (special)
        {
            for (i = 0; i < Special.Count; i++)
            {
                if (Special[i].instanceUID == instanceUID)
                {
                    Special.RemoveAt(i);
                    DeactivateUID(instanceUID);
                    return true;
                }
            }
        }
        else
        {
            for (i = 0; i < Normal.Count; i++)
            {
                if (Normal[i].instanceUID == instanceUID)
                {
                    Normal.RemoveAt(i);
                    DeactivateUID(instanceUID);
                    return true;
                }
            }
        }
        return false;
    }
    
    private bool Remove(int index, bool special = false)
    {
        if (special)
        {
            if (Special.InBounds(index))
            {
                byte UID = Special[index].instanceUID;
                Special.RemoveAt(index);
                DeactivateUID(UID);
                return true;
            }
        }
        else
        {
            if (Normal.InBounds(index))
            {
                byte UID = Normal[index].instanceUID;
                Normal.RemoveAt(index);
                DeactivateUID(UID);
                return true;
            }
        }
        return false;
    }

    public int Remove(string internalName, bool special = false, bool onlyHarmful = true)
    {
        return Remove(internalName, RemovalTarget.First, false, true);
    }
    
    public int Remove(string internalName, RemovalTarget target = RemovalTarget.First, bool special = false, bool onlyHarmful = true)
    {
        int removed, i;
        if (special)
        {
            switch (target)
            {
                default:
                case RemovalTarget.First:
                    for (i = 0; i < Special.Count; i++)
                    {
                        if (internalName == Special[i].internalName)
                        {
                            Remove(i, true);
                            return 1;
                        }
                    }
                    return 0;

                case RemovalTarget.Last:
                    for (i = Special.Count - 1; i >= 0; i--)
                    {
                        if (internalName == Special[i].internalName)
                        {
                            Remove(i, true);
                            return 1;
                        }
                    }
                    return 0;

                case RemovalTarget.All:
                    removed = 0;
                    for (i = Special.Count - 1; i >= 0; i--)
                    {
                        if (internalName == Special[i].internalName)
                        {
                            Remove(i, true);
                            removed++;
                        }
                    }
                    return removed;
            }
        }
        else
        {
            switch (target)
            {
                default:
                case RemovalTarget.First:
                    for (i = 0; i < Normal.Count; i++)
                    {
                        if (internalName == Normal[i].internalName)
                        {
                            Remove(i, true);
                            return 1;
                        }
                    }
                    return 0;

                case RemovalTarget.Last:
                    for (i = Normal.Count - 1; i >= 0; i--)
                    {
                        if (internalName == Normal[i].internalName)
                        {
                            Remove(i, true);
                            return 1;
                        }
                    }
                    return 0;

                case RemovalTarget.All:
                    removed = 0;
                    for (i = Normal.Count - 1; i >= 0; i--)
                    {
                        if (internalName == Normal[i].internalName)
                        {
                            Remove(i, true);
                            removed++;
                        }
                    }
                    return removed;
            }
        }
    }
}

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

    public ActiveEffects container = null;

    public string displayName = "UNASSIGNED";
    public string internalName = null;

    public byte instanceUID = 0;

    public bool special = false;
    public bool removable = true;
    public bool harmful;

    public int lifetime = 1;
    public bool tickOnTurnStart = true;

    public SE_DamageOverTime damageOverTime = null;
    public SE_AttributeModifier attributeModifier = null;

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public virtual void OnTurnStart()
    {
        if (tickOnTurnStart)
            lifetime -= 1;
        if (lifetime <= 0)
            OnExpire();
    }

    public virtual void OnTurnEnd()
    {
        if (!tickOnTurnStart)
            lifetime -= 1;
        if (lifetime <= 0)
            OnExpire();
    }

    public virtual void OnApply()
    {

    }

    public virtual void OnExpire()
    {
        container.Remove(instanceUID, special);
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
