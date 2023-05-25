namespace NeoCambion
{
    using System;
    using System.Diagnostics;

    namespace Random
    {
        // https://en.wikipedia.org/wiki/Xorshift#xorshift.2A
        public class RandomXOR
        {
            public struct State
            {
                private ulong l0;
                private ulong l1;
                public ulong[] currentValues { get { return new ulong[2] { l0, l1 }; } }
                public string initSeed;
                public bool hexInitSeed;

                public State(string seed)
                {
                    ulong[] vals = FromSeed(seed);
                    l0 = vals[0];
                    l1 = vals[1];
                    initSeed = null;
                    hexInitSeed = seed.IsHexidecimal();
                }
            
                public State(string seed, bool hexInitSeed)
                {
                    ulong[] vals = FromSeed(seed, hexInitSeed);
                    l0 = vals[0];
                    l1 = vals[1];
                    initSeed = null;
                    this.hexInitSeed = hexInitSeed;
                }
            
                public State(string state, string seed = null)
                {
                    ulong[] vals = FromSeed(state);
                    l0 = vals[0];
                    l1 = vals[1];
                    initSeed = seed;
                    hexInitSeed = seed == null ? false : seed.IsHexidecimal();
                }
            
                public State(string state, string seed, bool hexInitSeed)
                {
                    ulong[] vals = FromSeed(state);
                    l0 = vals[0];
                    l1 = vals[1];
                    initSeed = seed;
                    this.hexInitSeed = hexInitSeed;
                }
            
                public State(ulong[] state, string seed = null)
                {
                    l0 = state[0];
                    l1 = state[1];
                    initSeed = seed;
                    hexInitSeed = seed == null ? false : seed.IsHexidecimal();
                }
                
                public State(ulong[] state, string seed, bool hexInitSeed)
                {
                    l0 = state[0];
                    l1 = state[1];
                    initSeed = seed;
                    this.hexInitSeed = hexInitSeed;
                }

                public static ulong[] FromSeed(string seed, bool forceHexSeed = false)
                {
                    ulong[] vals = new ulong[2];
                    if (forceHexSeed || (seed.Length == 32 && seed.IsHexidecimal()))
                    {
                        if (seed.Length > 32)
                            seed = seed.Substring(0, 32);
                        else if (seed.Length < 32)
                            seed = seed.PadRight(32, '0');
                        char[] seedChars = seed.ToCharArray();
                        byte[] asBytesA = new byte[8];
                        byte[] asBytesB = new byte[8];
                        for (int i = 0; i < 8; i++)
                        {
                            int a = 2 * i;
                            asBytesA[i] = Ext_Char.ParseHexToByte(new char[] { seedChars[a], seedChars[a + 1] });
                            asBytesB[i] = Ext_Char.ParseHexToByte(new char[] { seedChars[a + 8], seedChars[a + 9] });
                        }
                        vals[0] = asBytesA.ToULong();
                        vals[1] = asBytesB.ToULong();
                    }
                    else
                    {
                        if (seed.Length > 8)
                            seed = seed.Substring(0, 8);
                        else if (seed.Length < 8)
                            seed = seed.PadRight(8, ' ');
                        byte[] asBytes = seed.ToBytes();
                        vals[0] = new byte[] { asBytes[0], asBytes[1], asBytes[2], asBytes[3], asBytes[4], asBytes[5], asBytes[6], asBytes[7] }.ToULong();
                        vals[1] = new byte[] { asBytes[8], asBytes[9], asBytes[10], asBytes[11], asBytes[12], asBytes[13], asBytes[14], asBytes[15] }.ToULong();
                    }
                    return vals;
                }
                
                public static string ToSeed(int[] values, bool hexSeed = false)
                {
                    string str = "";
                    byte[] asBytes;
                    for (int i = 0; i < 4 && i < values.Length; i++)
                    {
                        asBytes = values[i].ToBytes();
                        if (hexSeed)
                            str += asBytes.ParseToHexString();
                        else
                            str += asBytes.ParseToString();
                    }
                    return str;
                }
            
                public static string ToSeed(ulong[] values, bool hexSeed = false)
                {
                    string str = "";
                    byte[] asBytes;
                    for (int i = 0; i < 2 && i < values.Length; i++)
                    {
                        asBytes = values[i].ToBytes();
                        if (hexSeed)
                            str += asBytes.ParseToHexString();
                        else
                            str += asBytes.ParseToString();
                    }
                    return str;
                }
                
                public string GetStateSeed(bool hexSeed = false)
                {
                    return ToSeed(new ulong[] { l0, l1 }, hexSeed);
                }
            }

            public RandomXOR(State initState)
            {
                _state = initState.currentValues;
                if (initState.initSeed == null)
                    initSeed = State.ToSeed(_state);
                else
                    initSeed = initState.initSeed;
            }
        
            public RandomXOR(string initSeed)
            {
                _state = State.FromSeed(initSeed);
                this.initSeed = initSeed;
            }

            public RandomXOR(long initSeedLong)
            {
                ulong val = (ulong)initSeedLong;
                _state[0] = val;
                _state[1] = ~val;
                initSeed = State.ToSeed(_state);
            }
        
            public RandomXOR(int initSeedInt)
            {
                ulong val = (ulong)initSeedInt;
                _state[0] = val;
                _state[1] = ~val;
                initSeed = State.ToSeed(_state);
            }

            /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

            public string initSeed = null;
            public bool hexInitSeed = false;
            private ulong[] _state = new ulong[2];
            public State state { get { return GetState(); } set { SetState(value); } }

            public bool unInitialised { get { return initSeed == null || (_state[0] == 0UL && _state[0] == 0UL); } }

            /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

            public ulong Next()
            {
                if (unInitialised)
                    SetSeed();

                ulong t = _state[0];
                ulong s = _state[1];
                ulong s2 = _state[1];
                _state[0] = s;
                t ^= t << 23;
                t ^= t >> 18;
                t ^= s ^ (s2 >> 5);
                _state[1] = t;
                return t + s;
            }

            public float Range(float minInclusive, float maxInclusive)
            {
                ulong r = Next();
                int rInt = r.ToBytes().ToInt();
                float x = BitConverter.Int32BitsToSingle((127 << 23) | ((rInt >> 9) & 8388607)) - 1.0f;
                float range = maxInclusive - minInclusive;
                range = BitConverter.Int32BitsToSingle((range.ToBytes().ToInt()) + 1);
                return minInclusive + x * range;
            }

            public long Range(long minInclusive, long maxExclusive)
            {
                if (maxExclusive < minInclusive)
                {
                    long x = minInclusive;
                    minInclusive = maxExclusive;
                    maxExclusive = x;
                }
                return minInclusive + (long)(Next() % (ulong)(maxExclusive - minInclusive));
            }
        
            public int Range(int minInclusive, int maxExclusive)
            {
                if (maxExclusive < minInclusive)
                {
                    int x = minInclusive;
                    minInclusive = maxExclusive;
                    maxExclusive = x;
                }
                return minInclusive + (int)(Next() % (ulong)(maxExclusive - minInclusive));
            }

            /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

            public static string RandomSeed()
            {
                long seedLong = DateTime.Now.Ticks;
                ulong val = (ulong)seedLong;
                ulong[] vals = new ulong[2] { val, ~val };
                return State.ToSeed(vals);
            }

            public void SetSeed()
            {
                SetSeed(RandomSeed());
            }

            public void SetSeed(string initSeed, bool hexSeed = false)
            {
                _state = State.FromSeed(initSeed, hexSeed);
                this.initSeed = initSeed;
                hexInitSeed = hexSeed;
            }

            public void SetSeed(long initSeedLong)
            {
                ulong val = (ulong)initSeedLong;
                _state[0] = val;
                _state[1] = ~val;
                initSeed = State.ToSeed(_state);
                hexInitSeed = false;
            }
        
            public void SetSeed(int initSeedInt)
            {
                ulong val = (ulong)initSeedInt;
                _state[0] = val;
                _state[1] = ~val;
                initSeed = State.ToSeed(_state);
                hexInitSeed = false;
            }

            public void SetState(State newState)
            {
                initSeed = newState.initSeed;
                _state = newState.currentValues;
            }

            public State GetState()
            {
                return new State(_state, initSeed, hexInitSeed);
            }
        }

        namespace Unity
        {
            using UnityEngine;

            public static class UnityRandom
            {
                #region [ BASE STATIC PROPERTIES ]

                public static Vector2 insideUnitCircle = UnityEngine.Random.insideUnitCircle;
                public static Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
                public static Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
                public static Quaternion rotation = UnityEngine.Random.rotation;
                public static Quaternion rotationUniform = UnityEngine.Random.rotationUniform;
                public static UnityEngine.Random.State state = UnityEngine.Random.state;
                public static float value = UnityEngine.Random.value;

                #endregion

                #region [ BASE STATIC METHODS ]

                public static Color ColorHSV()
                {
                    return UnityEngine.Random.ColorHSV();
                }
                
                public static Color ColorHSV(float hueMin, float hueMax)
                {
                    return UnityEngine.Random.ColorHSV(hueMin, hueMax);
                }
                
                public static Color ColorHSV(float hueMin, float hueMax, float saturationMin, float saturationMax)
                {
                    return UnityEngine.Random.ColorHSV(hueMin, hueMax, saturationMin, saturationMax);
                }
                
                public static Color ColorHSV(float hueMin, float hueMax, float saturationMin, float saturationMax, float valueMin, float valueMax)
                {
                    return UnityEngine.Random.ColorHSV(hueMin, hueMax, saturationMin, saturationMax, valueMin, valueMax);
                }
                
                public static Color ColorHSV(float hueMin, float hueMax, float saturationMin, float saturationMax, float valueMin, float valueMax, float alphaMin, float alphaMax)
                {
                    return UnityEngine.Random.ColorHSV(hueMin, hueMax, saturationMin, saturationMax, valueMin, valueMax, alphaMin, alphaMax);
                }

                public static void InitState(int seed)
                {
                    UnityEngine.Random.InitState(seed);
                }

                public static float Range(float minInclusive, float maxInclusive)
                {
                    return UnityEngine.Random.Range(minInclusive, maxInclusive);
                }
                
                public static int Range(int minInclusive, int maxExclusive)
                {
                    return UnityEngine.Random.Range(minInclusive, maxExclusive);
                }

                #endregion

                /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

                public static int NewRandomSeed()
                {
                    int seed = (int)DateTime.Now.Ticks;
                    InitState(seed);
                    return seed;
                }
            }

            public static class UnityRandomUtility
            {
                #region [ EXTRACTING STATE DATA ]
                // https://answers.unity.com/questions/1670397/how-do-you-get-a-seed-value-from-unityenginerandom.html



                /*private int[] IntsFromRandomState(UnityEngine.Random.State state)
                {

                }

                private UnityEngine.Random.State RandomStateFromInts(int[] int4)
                {

                }*/

                #endregion
            }
        }
    }
}