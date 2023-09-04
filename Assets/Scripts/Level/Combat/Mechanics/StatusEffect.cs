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
    public CombatantCore combatant;

    [System.Serializable]
    public enum RemovalTarget { First, Last, All }

    public List<StatusEffect> Special = new List<StatusEffect>();
    public List<StatusEffect> Normal = new List<StatusEffect>();
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
        if (internalName == StatusEffect.checkAny)
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
        else if (internalName == StatusEffect.checkAnyNegative)
        {
            switch (target)
            {
                default:
                case RemovalTarget.First:
                    for (i = 0; i < Special.Count; i++)
                    {
                        if (Special[i].removable && Special[i].harmful)
                        {
                            Remove(i, true);
                            return 1;
                        }
                    }
                    for (i = 0; i < Normal.Count; i++)
                    {
                        if (Normal[i].removable && Normal[i].harmful)
                        {
                            Remove(i, true);
                            return 1;
                        }
                    }
                    return 0;

                case RemovalTarget.Last:
                    for (i = Special.Count - 1; i >= 0; i--)
                    {
                        if (Special[i].removable && Special[i].harmful)
                        {
                            Remove(i, true);
                            return 1;
                        }
                    }
                    for (i = Normal.Count - 1; i >= 0; i--)
                    {
                        if (Normal[i].removable && Normal[i].harmful)
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
                        if (Special[i].removable && Special[i].harmful)
                        {
                            Remove(i, true);
                            removed++;
                        }
                    }
                    for (i = Normal.Count - 1; i >= 0; i--)
                    {
                        if (Normal[i].removable && Normal[i].harmful)
                        {
                            Remove(i, true);
                            removed++;
                        }
                    }
                    return removed;
            }
        }
        else if (internalName == StatusEffect.checkAnyPositive)
        {
            switch (target)
            {
                default:
                case RemovalTarget.First:
                    for (i = 0; i < Special.Count; i++)
                    {
                        if (Special[i].removable && !Special[i].harmful)
                        {
                            Remove(i, true);
                            return 1;
                        }
                    }
                    for (i = 0; i < Normal.Count; i++)
                    {
                        if (Normal[i].removable && !Normal[i].harmful)
                        {
                            Remove(i, true);
                            return 1;
                        }
                    }
                    return 0;

                case RemovalTarget.Last:
                    for (i = Special.Count - 1; i >= 0; i--)
                    {
                        if (Special[i].removable && !Special[i].harmful)
                        {
                            Remove(i, true);
                            return 1;
                        }
                    }
                    for (i = Normal.Count - 1; i >= 0; i--)
                    {
                        if (Normal[i].removable && !Normal[i].harmful)
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
                        if (Special[i].removable && !Special[i].harmful)
                        {
                            Remove(i, true);
                            removed++;
                        }
                    }
                    for (i = Normal.Count - 1; i >= 0; i--)
                    {
                        if (Normal[i].removable && !Normal[i].harmful)
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

    public byte AddAttributeMod(StatusEffect.SE_AttributeModifier modifier)
    {
        if (modifier != null && combatant != null)
        {
            switch (modifier.attribute)
            {
                default:
                    return 0;

                case CombatantAttribute.Health:
                    if (modifier.additive)
                        return combatant.health.modifiers.AddToAdd(modifier.value);
                    else
                        return combatant.health.modifiers.AddToMultiply(modifier.value);

                case CombatantAttribute.Attack:
                    if (modifier.additive)
                        return combatant.attack.modifiers.AddToAdd(modifier.value);
                    else
                        return combatant.attack.modifiers.AddToMultiply(modifier.value);

                case CombatantAttribute.Defence:
                    if (modifier.additive)
                        return combatant.defence.modifiers.AddToAdd(modifier.value);
                    else
                        return combatant.defence.modifiers.AddToMultiply(modifier.value);

                case CombatantAttribute.Speed:
                    if (modifier.additive)
                        return combatant.speed.modifiers.AddToAdd(modifier.value);
                    else
                        return combatant.speed.modifiers.AddToMultiply(modifier.value);

                case CombatantAttribute.InflictChance:
                    return 0;

                case CombatantAttribute.InflictResist:
                    return 0;
            }
        }
        else
            return 0;
    }
    public bool RemoveAttributeMod(StatusEffect.SE_AttributeModifier modifier)
    {
        if (modifier != null && combatant != null)
        {
            switch (modifier.attribute)
            {
                default:
                    return false;

                case CombatantAttribute.Health:
                    return combatant.health.modifiers.Remove(modifier.modID);

                case CombatantAttribute.Attack:
                    return combatant.attack.modifiers.Remove(modifier.modID);

                case CombatantAttribute.Defence:
                    return combatant.defence.modifiers.Remove(modifier.modID);

                case CombatantAttribute.Speed:
                    return combatant.speed.modifiers.Remove(modifier.modID);

                case CombatantAttribute.InflictChance:
                    return false;

                case CombatantAttribute.InflictResist:
                    return false;
            }
        }
        else
            return false;
    }

    public string AddDamageDealtMod(StatusEffect.SE_DamageDealtModifier modifier)
    {
        string name = modifier.typeID + "_" + System.DateTime.Now.Ticks;
        modifier.name = name;
        combatant.damageOutMods.Add(modifier.asStruct);
        return name;
    }
    public void RemoveDamageDealtMod(StatusEffect.SE_DamageDealtModifier modifier)
    {
        for (int i = 0; i < combatant.damageOutMods.Count; i++)
        {
            if (modifier.name == combatant.damageOutMods[i].modifierName)
                combatant.damageOutMods.RemoveAt(i);
        }
    }
    
    public string AddDamageTakenMod(StatusEffect.SE_DamageTakenModifier modifier)
    {
        string name = modifier.typeID + "_" + System.DateTime.Now.Ticks;
        modifier.name = name;
        combatant.damageInMods.Add(modifier.asStruct);
        return name;
    }
    public void RemoveDamageTakenMod(StatusEffect.SE_DamageTakenModifier modifier)
    {
        for (int i = 0; i < combatant.damageInMods.Count; i++)
        {
            if (modifier.name == combatant.damageInMods[i].modifierName)
                combatant.damageInMods.RemoveAt(i);
        }
    }

    public void ModifyShieldCount(bool increment) => combatant.activeShieldCount += (increment ? 1 : -1);

    public void ClearTaunted() => combatant.brain.tauntedBy = -1;

    public void Interrupt(StatusEffect effect, float baseChance, bool guaranteed = false) => combatant.Interrupt(effect, baseChance, guaranteed);

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

public struct StatusEffectReturnData
{
    public string internalName;
    public bool special;

    public StatusEffectReturnData(string internalName = null, bool special = false)
    {
        this.internalName = internalName;
        this.special = special;
    }

    public bool Null => internalName == null;
}
[System.Serializable]
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
        public byte modID;
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

    public class SE_DamageDealtModifier
    {
        public string name;
        public int typeID;
        public float value;

        public DamageDealtModifier asStruct { get { return new DamageDealtModifier(name, typeID, value); } }
    }
    
    public class SE_DamageTakenModifier
    {
        public string name;
        public int typeID;
        public DamageTakenModifier.ModType value;
        public float floatValue { get { return asStruct.mod; } }

        public DamageTakenModifier asStruct { get { return new DamageTakenModifier(name, typeID, value); } }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public ActiveEffects container = null;

    public string displayName = "UNASSIGNED";
    public string internalName = null;

    private string _descriptionRaw = null;
    public string descriptionRaw
    {
        get
        {
            return _descriptionRaw;
        }
        set
        {
            descriptionRaw = value;
            UpdateDescription();
        }
    }
    public string description { get; private set; }

    public byte instanceUID = 0;
    public KeyValuePair<bool, int> appliedBy = new KeyValuePair<bool, int>(true, -1);

    public bool special = false;
    public bool removable = true;
    public bool resistable = true;
    public bool harmful = true;

    public bool interruptOnApply = false;
    public bool guaranteedInterrupt = false;

    public bool noExpiry = false;

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

    public void AddAttributeMod() { _attributeModifier.modID = container.AddAttributeMod(_attributeModifier); }
    public void RemoveAttributeMod() { container.RemoveAttributeMod(_attributeModifier); }
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
            if (value != null)
            {
                if (onApply.Contains("AttributeMod"))
                    onApply[onApply.IndexOf("AttributeMod")].Set(AddAttributeMod);
                else
                    onApply.Add(new NamedCallback("AttributeMod", AddAttributeMod));

                if (onExpire.Contains("AttributeMod"))
                    onExpire[onExpire.IndexOf("AttributeMod")].Set(RemoveAttributeMod);
                else
                    onExpire.Add(new NamedCallback("AttributeMod", RemoveAttributeMod));

                if (onDispel.Contains("AttributeMod"))
                    onDispel[onDispel.IndexOf("AttributeMod")].Set(RemoveAttributeMod);
                else
                    onDispel.Add(new NamedCallback("AttributeMod", RemoveAttributeMod));
            }
        }
    }

    public void AddDamageDealtMod() { _dmgOutModifier.name = container.AddDamageDealtMod(_dmgOutModifier); }
    public void RemoveDamageDealtMod() { container.RemoveDamageDealtMod(_dmgOutModifier); }
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
            if (value != null)
            {
                if (onApply.Contains("DmgOutMod"))
                    onApply[onApply.IndexOf("DmgOutMod")].Set(AddDamageDealtMod);
                else
                    onApply.Add(new NamedCallback("DmgOutMod", AddDamageDealtMod));

                if (onExpire.Contains("DmgOutMod"))
                    onExpire[onExpire.IndexOf("DmgOutMod")].Set(RemoveDamageDealtMod);
                else
                    onExpire.Add(new NamedCallback("DmgOutMod", RemoveDamageDealtMod));

                if (onDispel.Contains("DmgOutMod"))
                    onDispel[onDispel.IndexOf("DmgOutMod")].Set(RemoveDamageDealtMod);
                else
                    onDispel.Add(new NamedCallback("DmgOutMod", RemoveDamageDealtMod));
            }
        }
    }

    public void AddDamageTakenMod() { _dmgInModifier.name = container.AddDamageTakenMod(_dmgInModifier); }
    public void RemoveDamageTakenMod() { container.RemoveDamageTakenMod(_dmgInModifier); }
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
            if (value != null)
            {
                if (onApply.Contains("DmgInMod"))
                    onApply[onApply.IndexOf("DmgInMod")].Set(AddDamageTakenMod);
                else
                    onApply.Add(new NamedCallback("DmgInMod", AddDamageTakenMod));

                if (onExpire.Contains("DmgInMod"))
                    onExpire[onExpire.IndexOf("DmgInMod")].Set(RemoveDamageTakenMod);
                else
                    onExpire.Add(new NamedCallback("DmgInMod", RemoveDamageTakenMod));

                if (onDispel.Contains("DmgInMod"))
                    onDispel[onDispel.IndexOf("DmgInMod")].Set(RemoveDamageTakenMod);
                else
                    onDispel.Add(new NamedCallback("DmgInMod", RemoveDamageTakenMod));
            }
        }
    }

    public int shielding = -1;
    public int currentShield = -1;
    public bool expireOnShieldBroken = true;

    public int DamageShield(float damage)
    {
        int blocked = 0;
        if (currentShield > 0)
        {
            if (damage > currentShield)
                blocked = currentShield;
            else
                blocked = Mathf.RoundToInt(damage);
        }
        currentShield -= blocked;
        if (currentShield == 0 && expireOnShieldBroken)
            onDispel.Invoke();
        return blocked;
    }

    public void IncrementShieldCount() =>  container.ModifyShieldCount(true);

    public void DecrementShieldCount() => container.ModifyShieldCount(false);

    public bool clearTauntedOnExpire = false;

    public void Interrupt()
    {
        container.Interrupt(this, 0.4f, guaranteedInterrupt);
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
        UpdateDescription();
        if (lifetime <= 0)
            onExpire.Invoke();
    }

    public void Expire()
    {
        if (clearTauntedOnExpire)
            container.ClearTaunted();
        container.Remove(instanceUID);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public StatusEffect()
    {
        if (interruptOnApply)
            onApply.Add(new NamedCallback("Interrupt", Interrupt));
        onExpire.Add(new NamedCallback("Expiry", Expire));
        onDispel.Add(new NamedCallback("Expiry", Expire));
    }

    public StatusEffect(bool tickOnTurnStart)
    {
        if (interruptOnApply)
            onApply.Add(new NamedCallback("Interrupt", Interrupt));
        onExpire.Add(new NamedCallback("Expiry", Expire));
        onDispel.Add(new NamedCallback("Expiry", Expire));
        this.tickOnTurnStart = tickOnTurnStart;
        if (!noExpiry)
        {
            NamedCallback lifetime = new NamedCallback("Lifetime", LifetimeTick);
            if (tickOnTurnStart)
                onTurnStart.Add(lifetime);
            else
                onTurnEnd.Add(lifetime);
        }
    }
    
    public StatusEffect(int shieldValue)
    {
        onExpire.Add(new NamedCallback("Expiry", Expire));
        onDispel.Add(new NamedCallback("Expiry", Expire));
        shielding = shieldValue;
        currentShield = shieldValue;
        if (!noExpiry)
        {
            tickOnTurnStart = true;
            NamedCallback lifetime = new NamedCallback("Lifetime", LifetimeTick);
            if (tickOnTurnStart)
                onTurnStart.Add(lifetime);
            else
                onTurnEnd.Add(lifetime);
        }
        onApply.Add(new NamedCallback("ShieldIncrement", IncrementShieldCount));
        onExpire.Add(new NamedCallback("ShieldDecrement", DecrementShieldCount));
        onDispel.Add(new NamedCallback("ShieldDecrement", DecrementShieldCount));
    }

    public bool Matches(string internalName)
    {
        return internalName == this.internalName || internalName == checkAny || harmful ? internalName == checkAnyNegative : internalName == checkAnyPositive;
    }

    public static bool Matches(string refInternalName, string checkInternalName)
    {
        return Matches(refInternalName, false, checkInternalName);
    }
    
    public static bool Matches(string refInternalName, bool refHarmful, string checkInternalName)
    {
        return checkInternalName == refInternalName || checkInternalName == checkAny || refHarmful ? checkInternalName == checkAnyNegative : checkInternalName == checkAnyPositive;
    }

    public void UpdateDescription(int roundTo = 2)
    {
        description = Core.DynamicDescription(container.combatant, this, descriptionRaw, roundTo);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public readonly static string checkAny = "anyEffect";
    public readonly static string checkAnyNegative = "anyEffect_Ngt";
    public readonly static string checkAnyPositive = "anyEffect_Pst";

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

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public static StatusEffect Shield(int shieldValue, int duration = 2)
    {
        return new StatusEffect(shieldValue)
        {
            resistable = false,
            harmful = false,
            tickOnTurnStart = true,
            lifetime = duration
        };
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public static StatusEffect Marked()
    {
        StatusEffect effect = new StatusEffect()
        {

        };
        return effect;
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
