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

public class ActiveEffects
{
    private CombatantCore combatant;

    [System.Serializable]
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

    public ActiveEffects(CombatantCore combatant)
    {
        inactiveUIDs.AddRange(allUIDs);
        this.combatant = combatant;
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public readonly static string removeAny = "ANY";

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
    
    public bool Remove(int index = -1, bool special = false)
    {
        if (index >= 0)
        {
            if (special)
            {
                if (index < Special.Count)
                {
                    byte UID = Special[index].instanceUID;
                    Special.RemoveAt(index);
                    DeactivateUID(UID);
                    return true;
                }
            }
            else
            {
                if (index < Normal.Count)
                {
                    byte UID = Normal[index].instanceUID;
                    Normal.RemoveAt(index);
                    DeactivateUID(UID);
                    return true;
                }
            }
        }
        else
        {
            if (Special.Count > 0)
            {
                List<int> inds = new List<int>().IncrementalPopulate(0, 1, Special.Count);
                int r;
                for (int i = 0; i < Special.Count; i++)
                {
                    r = Random.Range(0, inds.Count);
                    if (Special[inds[r]].removable)
                    {
                        byte UID = Special[inds[r]].instanceUID;
                        Special.RemoveAt(inds[r]);
                        DeactivateUID(UID);
                        return true;
                    }
                    else
                    {
                        inds.RemoveAt(r);
                    }
                }
            }
            if (Normal.Count > 0)
            {
                List<int> inds = new List<int>().IncrementalPopulate(0, 1, Normal.Count);
                int r;
                for (int i = 0; i < Normal.Count; i++)
                {
                    r = Random.Range(0, inds.Count);
                    if (Normal[inds[r]].removable)
                    {
                        byte UID = Normal[inds[r]].instanceUID;
                        Normal.RemoveAt(inds[r]);
                        DeactivateUID(UID);
                        return true;
                    }
                    else
                    {
                        inds.RemoveAt(r);
                    }
                }
            }
        }
        return false;
    }

    public int Remove(string internalName, bool special = false, bool onlyHarmful = true)
    {
        return Remove(internalName, RemovalTarget.First, special, onlyHarmful);
    }
    
    public int Remove(string internalName, RemovalTarget target = RemovalTarget.First, bool special = false, bool onlyHarmful = true)
    {
        int removed, i;
        bool validForRemoval;
        if (internalName == "ANY")
        {
            switch (target)
            {
                default:
                case RemovalTarget.First:
                    for (i = 0; i < Special.Count; i++)
                    {
                        validForRemoval = !onlyHarmful || (onlyHarmful && Special[i].harmful);
                        if (Special[i].removable && validForRemoval)
                        {
                            Remove(i, true);
                            return 1;
                        }
                    }
                    for (i = 0; i < Normal.Count; i++)
                    {
                        validForRemoval = !onlyHarmful || (onlyHarmful && Special[i].harmful);
                        if (Normal[i].removable && validForRemoval)
                        {
                            Remove(i, true);
                            return 1;
                        }
                    }
                    return 0;

                case RemovalTarget.Last:
                    for (i = Special.Count - 1; i >= 0; i--)
                    {
                        validForRemoval = !onlyHarmful || (onlyHarmful && Special[i].harmful);
                        if (Special[i].removable && validForRemoval)
                        {
                            Remove(i, true);
                            return 1;
                        }
                    }
                    for (i = Normal.Count - 1; i >= 0; i--)
                    {
                        validForRemoval = !onlyHarmful || (onlyHarmful && Special[i].harmful);
                        if (Normal[i].removable && validForRemoval)
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
                        validForRemoval = !onlyHarmful || (onlyHarmful && Special[i].harmful);
                        if (Special[i].removable && validForRemoval)
                        {
                            Remove(i, true);
                            removed++;
                        }
                    }
                    for (i = Normal.Count - 1; i >= 0; i--)
                    {
                        validForRemoval = !onlyHarmful || (onlyHarmful && Special[i].harmful);
                        if (Normal[i].removable && validForRemoval)
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

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public int Stacks(string internalName, bool special = false, bool highestInstance = true)
    {
        int val = highestInstance ? int.MinValue : int.MaxValue;
        if (special)
        {
            if (highestInstance)
            {
                foreach (StatusEffect effect in Special)
                {
                    if (internalName == effect.internalName && effect.stacks > val)
                        val = effect.stacks;
                }
                if (val == int.MinValue)
                    return -1;
                return val;
            }
            else
            {
                foreach (StatusEffect effect in Special)
                {
                    if (internalName == effect.internalName && effect.stacks < val)
                        val = effect.stacks;
                }
                if (val == int.MaxValue)
                    return -1;
                return val;
            }
        }
        else
        {
            if (highestInstance)
            {
                foreach (StatusEffect effect in Normal)
                {
                    if (internalName == effect.internalName && effect.stacks > val)
                        val = effect.stacks;
                }
                if (val == int.MinValue)
                    return -1;
                return val;
            }
            else
            {
                foreach (StatusEffect effect in Normal)
                {
                    if (internalName == effect.internalName && effect.stacks < val)
                        val = effect.stacks;
                }
                if (val == int.MaxValue)
                    return -1;
                return val;
            }
        }
    }

    public int Lifetime(string internalName, bool special = false, bool highestInstance = true)
    {
        int val = highestInstance ? int.MinValue : int.MaxValue;
        if (special)
        {
            if (highestInstance)
            {
                foreach (StatusEffect effect in Special)
                {
                    if (internalName == effect.internalName && effect.lifetime > val)
                        val = effect.lifetime;
                }
                if (val == int.MinValue)
                    return -1;
                return val;
            }
            else
            {
                foreach (StatusEffect effect in Special)
                {
                    if (internalName == effect.internalName && effect.lifetime < val)
                        val = effect.lifetime;
                }
                if (val == int.MaxValue)
                    return -1;
                return val;
            }
        }
        else
        {
            if (highestInstance)
            {
                foreach (StatusEffect effect in Normal)
                {
                    if (internalName == effect.internalName && effect.lifetime > val)
                        val = effect.lifetime;
                }
                if (val == int.MinValue)
                    return -1;
                return val;
            }
            else
            {
                foreach (StatusEffect effect in Normal)
                {
                    if (internalName == effect.internalName && effect.lifetime < val)
                        val = effect.lifetime;
                }
                if (val == int.MaxValue)
                    return -1;
                return val;
            }
        }
    }
}

public class StatusEffect
{
    public class SE_HealthOverTime
    {
        public DamageType type;
        public int value;
        public bool damage;

        public void Trigger()
        {

        }
    }
    public static SE_HealthOverTime DamageOverTime(DamageType damageType, int value)
    {
        return new SE_HealthOverTime() { type = damageType, value = value, damage = true };
    }
    public static SE_HealthOverTime HealingOverTime(int value)
    {
        return new SE_HealthOverTime() { type = DamageType.None, value = value, damage = false };
    }

    public class SE_AttributeModifier
    {
        public string name;
        public CombatantAttribute attribute;
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
    private void AddAttributeMod()
    {
        if (attributeModifier != null)
        {
            switch (attributeModifier.attribute)
            {
                default:
                    break;

                case CombatantAttribute.Health:
                    break;

                case CombatantAttribute.Defence:
                    break;

                case CombatantAttribute.Speed:
                    break;

                case CombatantAttribute.InflictChance:
                    break;

                case CombatantAttribute.InflictResist:
                    break;
            }
        }
    }
    private void RemoveAttributeMod()
    {

    }

    public class SE_DamageDealtModifier
    {
        public string name;
        public int typeID;
        public float value;

        public DamageDealtModifier Modifier { get { return new DamageDealtModifier(name, typeID, value); } }
    }
    
    public class SE_DamageTakenModifier
    {
        public string name;
        public int typeID;
        public DamageTakenModifier.ModType value;

        public DamageTakenModifier Modifier { get { return new DamageTakenModifier(name, typeID, value); } }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public ActiveEffects container = null;

    public string displayName = "UNASSIGNED";
    public string internalName = null;

    public byte instanceUID = 0;

    public bool special = false;
    public bool removable = true;
    public bool harmful;

    public int stacks = 1;
    public int lifetime = 1;
    public bool tickOnTurnStart = true;
    public bool noLifetime = false;

    private SE_HealthOverTime _healthOverTime = null;
    public SE_HealthOverTime healthOverTime
    {
        get
        {
            return _healthOverTime;
        }
        set
        {
            _healthOverTime = value;
            if (tickOnTurnStart)
            {
                if (onTurnStart.Contains("HealthOverTime"))
                    onTurnStart[onTurnStart.IndexOf("HealthOverTime")].Set(_healthOverTime.Trigger);
                else
                    onTurnStart.Add(new NamedCallback("HealthOverTime", _healthOverTime.Trigger));
            }
            else
            {
                if (onTurnEnd.Contains("HealthOverTime"))
                    onTurnEnd[onTurnEnd.IndexOf("HealthOverTime")].Set(_healthOverTime.Trigger);
                else
                    onTurnEnd.Add(new NamedCallback("HealthOverTime", _healthOverTime.Trigger));
            }
        }
    }
    private SE_AttributeModifier _attributeModifier = null;
    public SE_AttributeModifier attributeModifier
    {
        get
        {
            return _attributeModifier;
        }
        set
        {
            _attributeModifier = value;
            _attributeModifier.name = internalName + "_" + instanceUID;
            if (value != null)
            {
                if (onApply.Contains("HealthOverTime"))
                    onApply[onApply.IndexOf("HealthOverTime")].Set(_healthOverTime.Trigger);
                else
                    onTurnStart.Add(new NamedCallback("HealthOverTime", _healthOverTime.Trigger));
            }
        }
    }
    private SE_DamageDealtModifier _dmgOutModifier = null;
    public SE_DamageDealtModifier dmgOutModifier
    {
        get
        {
            return _dmgOutModifier;
        }
        set
        {
            _dmgOutModifier = value;
            _dmgOutModifier.name = internalName + "_" + instanceUID;
        }
    }
    private SE_DamageTakenModifier _dmgInModifier = null;
    public SE_DamageTakenModifier dmgInModifier
    {
        get
        {
            return _dmgInModifier;
        }
        set
        {
            _dmgInModifier = value;
            _dmgInModifier.name = internalName + "_" + instanceUID;
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public List<NamedCallback> onTurnStart = new List<NamedCallback>();
    public List<NamedCallback> onTurnEnd = new List<NamedCallback>();
    public List<NamedCallback> onApply = new List<NamedCallback>();
    public List<NamedCallback> onExpire = new List<NamedCallback>();
    public List<NamedCallback> onDispel = new List<NamedCallback>();

    public void LifetimeTick()
    {
        lifetime -= 1;
        if (lifetime <= 0)
            onExpire.Invoke();
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public StatusEffect()
    {

    }

    public StatusEffect(bool tickOnTurnStart)
    {
        this.tickOnTurnStart = tickOnTurnStart;
        NamedCallback lifetime = new NamedCallback("Lifetime", LifetimeTick);
        if (tickOnTurnStart)
            onTurnStart.Add(lifetime);
        else
            onTurnEnd.Add(lifetime);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public readonly static string dftFire = "defaultEffect_Fre";
    public readonly static string dftIce = "defaultEffect_Ice";
    public readonly static string dftEarth = "defaultEffect_Ert";
    public readonly static string dftLightning = "defaultEffect_Ltn";
    public readonly static string dftPhysical = "defaultEffect_Phy";
    public readonly static string dftPsychic = "defaultEffect_Psy";
    public readonly static string dftLight = "defaultEffect_Lht";
    public readonly static string dftDark = "defaultEffect_Drk";

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static StatusEffect Fire()
    {
        StatusEffect effect = new StatusEffect()
        {

        };
        return effect;
    }

    public static StatusEffect Ice()
    {
        StatusEffect effect = new StatusEffect()
        {

        };
        return effect;
    }

    public static StatusEffect Earth()
    {
        StatusEffect effect = new StatusEffect()
        {

        };
        return effect;
    }

    public static StatusEffect Lightning()
    {
        StatusEffect effect = new StatusEffect()
        {

        };
        return effect;
    }

    public static StatusEffect Physical()
    {
        StatusEffect effect = new StatusEffect()
        {

        };
        return effect;
    }

    public static StatusEffect Psychic()
    {
        StatusEffect effect = new StatusEffect()
        {

        };
        return effect;
    }

    public static StatusEffect Light()
    {
        StatusEffect effect = new StatusEffect()
        {

        };
        return effect;
    }

    public static StatusEffect Dark()
    {
        StatusEffect effect = new StatusEffect()
        {

        };
        return effect;
    }
}

public struct StatusModifier
{
    public enum ModType { Weak, Resist, Immune }

    public string modifierName;
    public int typeID;
    public ModType modType;
    public float modifier { get { return modType == ModType.Weak ? 2.0f : (modType == ModType.Resist ? 0.5f : 0.0f); } }

    public StatusModifier(string modifierName, int typeID, ModType modType)
    {
        this.modifierName = modifierName;
        this.typeID = typeID;
        this.modType = modType;
    }
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
