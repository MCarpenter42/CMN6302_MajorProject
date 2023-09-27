namespace NeoCambion.Interpolation
{
    using System;
    using System.Collections;

    public enum InterpType { Linear, CosCurve, CosSpeedUp, CosSlowDown, SmoothedLinear };

    public static class InterpDelta
    {
        private const float fPI = 3.1415926535897931f;
        private const double PI = 3.1415926535897931;

        #region < Calculation Wrappers >

        private static double Clamp(this double d, double min, double max)
        {
            if (d < min)
                return min;
            else if (d > max)
                return max;
            else
                return d;
        }
        private static float Clamp(this float f, float min, float max)
        {
            if (f < min)
                return min;
            else if (f > max)
                return max;
            else
                return f;
        }
        private static double Cos(this double d) => Math.Cos(d);
        private static float Cos(this float f) => (float)Math.Cos(f);
        private static double Acos(this double d) => Math.Acos(d);
        private static float Acos(this float f) => (float)Math.Acos(f);
        private static double Sin(this double d) => Math.Sin(d);
        private static float Sin(this float f) => (float)Math.Sin(f);
        private static double Asin(this double d) => Math.Asin(d);
        private static float Asin(this float f) => (float)Math.Asin(f);
        private static double Sqrt(this double d) => Math.Sqrt(d);
        private static float Sqrt(this float f) => (float)Math.Sqrt(f);
        private static double Pow(this double d, double p) => Math.Pow(d, p);
        private static float Pow(this float f, float p) => (float)Math.Pow(f, p);

        #endregion

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public static double CosCurve(double rawDelta)
        {
            double rad = rawDelta * PI;
            double cos = -rad.Cos();
            double output = (cos + 1d) * 0.5d;
            return output;
        }
        public static float CosCurve(float rawDelta)
        {
            float rad = rawDelta * fPI;
            float cos = -Cos(rad);
            float output = (cos + 1f) * 0.5f;
            return output;
        }

        public static double CosSpeedUp(double rawDelta)
        {
            double rad = rawDelta.Clamp(0d, 1d) - 2d;
            rad *= PI / 2d;
            double cos = rad.Cos();
            return 1d + cos;
        }
        public static float CosSpeedUp(float rawDelta)
        {
            float rad = rawDelta.Clamp(0f, 1f) - 2f;
            rad *= fPI / 2f;
            float cos = rad.Cos();
            return 1f + cos;
        }
        public static double CosSlowDown(double rawDelta)
        {
            double rad = rawDelta.Clamp(0d, 1d) - 1d;
            rad *= PI / 2d;
            double cos = rad.Cos();
            return cos;
        }
        public static float CosSlowDown(float rawDelta)
        {
            float rad = rawDelta.Clamp(0f, 1f) - 1f;
            rad *= fPI / 2f;
            float cos = rad.Cos();
            return cos;
        }

        public static double CosHill(double rawDelta)
        {
            double rad = rawDelta * PI * 2d;
            double cos = -rad.Cos();
            double output = (cos + 1d) * 0.5d;
            return output;
        }
        public static float CosHill(float rawDelta)
        {
            float rad = rawDelta * fPI * 2f;
            float cos = -rad.Cos();
            float output = (cos + 1f) * 0.5f;
            return output;
        }
        public static double CosValley(double rawDelta)
        {
            double rad = rawDelta * PI * 2d;
            double cos = rad.Cos();
            double output = (cos + 1d) * 0.5d;
            return output;
        }
        public static float CosValley(float rawDelta)
        {
            float rad = rawDelta * fPI * 2f;
            float cos = rad.Cos();
            float output = (cos + 1f) * 0.5f;
            return output;
        }

        public static double SmoothedLinear(double rawDelta, double smoothing0to1 = 0.5d)
        {
            double output = 0d;

            double n = 0.25d + 0.75d * smoothing0to1.Clamp(0d, 1d);
            double p1 = n.Sqrt() / 2d + 1d;
            double p2 = n.Sqrt() / 2d;

            double piDivN = PI / n;

            if (rawDelta < n / 2d)
            {
                output = (n.Pow(p1) * (1d - (piDivN * rawDelta).Cos())) / 2d;
            }
            else if (rawDelta > 1d - n / 2d)
            {
                output = 1d - (n.Pow(p1) * (1d + (piDivN * rawDelta + PI - piDivN).Cos())) / 2d;
            }
            else if (rawDelta >= n / 2d && rawDelta <= 1d - n / 2d)
            {
                output = (n.Pow(p2) * PI) / 2d * (PI / 2d).Sin() * (rawDelta - 0d) + 0d;
            }
            else if (rawDelta == 0.5d)
            {
                output = 0.5d;
            }

            return output;
        }
        public static float SmoothedLinear(float rawDelta, float smoothing0to1 = 0.5f)
        {
            float output = 0f;

            float n = 0.25f + 0.75f * smoothing0to1.Clamp(0f, 1f);
            float p1 = n.Sqrt() / 2f + 1f;
            float p2 = n.Sqrt() / 2f;

            float piDivN = fPI / n;

            if (rawDelta < n / 2f)
            {
                output = (n.Pow(p1) * (1f - (piDivN * rawDelta).Cos())) / 2f;
            }
            else if (rawDelta > 1f - n / 2f)
            {
                output = 1f - (n.Pow(p1) * (1f + (piDivN * rawDelta + fPI - piDivN).Cos())) / 2f;
            }
            else if (rawDelta >= n / 2f && rawDelta <= 1f - n / 2f)
            {
                output = (n.Pow(p2) * fPI) / 2f * (fPI / 2f).Sin() * (rawDelta - 0.5f) + 0.5f;
            }
            else if (rawDelta == 0.5f)
            {
                output = 0.5f;
            }

            return output;
        }

        public static class Inverse
        {
            public static double CosCurve(double rawDelta)
            {
                double cos = rawDelta.Clamp(0d, 1d) * 2d - 1d;
                double rad = (-cos).Acos();
                double output = rad / PI;
                return output;
            }
            public static float CosCurve(float rawDelta)
            {
                float cos = rawDelta.Clamp(0f, 1f) * 2f - 1f;
                float rad = (-cos).Acos();
                float output = rad / fPI;
                return output;
            }

            public static double CosSpeedUp(double rawDelta)
            {
                double cos = (rawDelta - 1d).Clamp(-1d, 1d);
                double rad = (-cos).Acos() / (PI / 2d);
                double output = rad + 2d;
                return output;
            }
            public static float CosSpeedUp(float rawDelta)
            {
                float cos = (rawDelta - 1f).Clamp(-1f, 1f);
                float rad = (-cos).Acos() / (fPI / 2f);
                float output = rad + 2f;
                return output;
            }
            public static double CosSlowDown(double rawDelta)
            {
                double cos = rawDelta.Clamp(-1d, 1d);
                double rad = (-cos).Acos() / (PI / 2d);
                return rad + 1d;
            }
            public static float CosSlowDown(float rawDelta, bool reverse = false)
            {
                float cos = rawDelta.Clamp(-1f, 1f);
                float rad = (-cos).Acos() / (fPI / 2f);
                return rad + 1f;
            }

            public static double SmoothedLinear(double rawDelta, double smoothing0to1 = 0.5d)
            {
                return InterpDelta.SmoothedLinear(1d - rawDelta, smoothing0to1);
            }
            public static float SmoothedLinear(float rawDelta, float smoothing0to1 = 0.5f)
            {
                return InterpDelta.SmoothedLinear(1f - rawDelta, smoothing0to1);
            }
        }

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public static double Lerp(this double a, double b, double delta) => a + (b - a) * delta;
        public static double Interp(this double a, double b, double rawDelta, InterpType type, bool inverse = false) => a.Interp(b, rawDelta, type, 0.5d, inverse);
        public static double Interp(this double a, double b, double rawDelta, InterpType type, double factor, bool inverse = false)
        {
            if (!inverse)
            {
                return type switch
                {
                    InterpType.CosCurve => a.Lerp(b, CosCurve(rawDelta)),
                    InterpType.CosSpeedUp => a.Lerp(b, CosSpeedUp(rawDelta)),
                    InterpType.CosSlowDown => a.Lerp(b, CosSlowDown(rawDelta)),
                    InterpType.SmoothedLinear => a.Lerp(b, SmoothedLinear(rawDelta, factor)),
                    _ => a.Lerp(b, rawDelta),
                };
            }
            else
            {
                return type switch
                {
                    InterpType.CosCurve => a.Lerp(b, Inverse.CosCurve(rawDelta)),
                    InterpType.CosSpeedUp => a.Lerp(b, Inverse.CosSpeedUp(rawDelta)),
                    InterpType.CosSlowDown => a.Lerp(b, Inverse.CosSlowDown(rawDelta)),
                    InterpType.SmoothedLinear => a.Lerp(b, Inverse.SmoothedLinear(rawDelta, factor)),
                    _ => a.Lerp(b, 1d - rawDelta),
                };
            }
        }

        public static float Lerp(this float a, float b, float delta) => a + (b - a) * delta;
        public static float Interp(this float a, float b, float rawDelta, InterpType type, bool inverse = false) => a.Interp(b, rawDelta, type, 0.5f, inverse);
        public static float Interp(this float a, float b, float rawDelta, InterpType type, float factor, bool inverse = false)
        {
            if (!inverse)
            {
                return type switch
                {
                    InterpType.CosCurve => a.Lerp(b, CosCurve(rawDelta)),
                    InterpType.CosSpeedUp => a.Lerp(b, CosSpeedUp(rawDelta)),
                    InterpType.CosSlowDown => a.Lerp(b, CosSlowDown(rawDelta)),
                    InterpType.SmoothedLinear => a.Lerp(b, SmoothedLinear(rawDelta, factor)),
                    _ => a.Lerp(b, rawDelta),
                };
            }
            else
            {
                return type switch
                {
                    InterpType.CosCurve => a.Lerp(b, Inverse.CosCurve(rawDelta)),
                    InterpType.CosSpeedUp => a.Lerp(b, Inverse.CosSpeedUp(rawDelta)),
                    InterpType.CosSlowDown => a.Lerp(b, Inverse.CosSlowDown(rawDelta)),
                    InterpType.SmoothedLinear => a.Lerp(b, Inverse.SmoothedLinear(rawDelta, factor)),
                    _ => a.Lerp(b, 1f - rawDelta),
                };
            }
        }
    }
    
    namespace Vectors
    {
        public static class VectorInterpDelta
        {
            
        }
    }

    namespace Unity
    {
        using UnityEngine;
        using static NeoCambion.Interpolation.InterpDelta;

        public static class UnityInterpDelta
        {
            public static Vector2 Lerp(this Vector2 a, Vector2 b, float delta) => new Vector2(a.x + (b.x - a.x) * delta, a.y + (b.y - a.y) * delta);
            public static Vector2 Interp(this Vector2 a, Vector2 b, float rawDelta, InterpType type, bool inverse = false) => a.Interp(b, rawDelta, type, 0.5f, inverse);
            public static Vector2 Interp(this Vector2 a, Vector2 b, float rawDelta, InterpType type, float factor, bool inverse = false)
            {
                if (!inverse)
                {
                    return type switch
                    {
                        InterpType.CosCurve => a.Lerp(b, CosCurve(rawDelta)),
                        InterpType.CosSpeedUp => a.Lerp(b, CosSpeedUp(rawDelta)),
                        InterpType.CosSlowDown => a.Lerp(b, CosSlowDown(rawDelta)),
                        InterpType.SmoothedLinear => a.Lerp(b, SmoothedLinear(rawDelta, factor)),
                        _ => a.Lerp(b, rawDelta),
                    };
                }
                else
                {
                    return type switch
                    {
                        InterpType.CosCurve => a.Lerp(b, Inverse.CosCurve(rawDelta)),
                        InterpType.CosSpeedUp => a.Lerp(b, Inverse.CosSpeedUp(rawDelta)),
                        InterpType.CosSlowDown => a.Lerp(b, Inverse.CosSlowDown(rawDelta)),
                        InterpType.SmoothedLinear => a.Lerp(b, Inverse.SmoothedLinear(rawDelta, factor)),
                        _ => a.Lerp(b, -rawDelta),
                    };
                }
            }

            public static Vector3 Lerp(this Vector3 a, Vector3 b, float delta) => new Vector3(a.x + (b.x - a.x) * delta, a.y + (b.y - a.y) * delta, a.z + (b.z - a.z) * delta);
            public static Vector3 Interp(this Vector3 a, Vector3 b, float rawDelta, InterpType type, bool inverse = false) => a.Interp(b, rawDelta, type, 0.5f, inverse);
            public static Vector3 Interp(this Vector3 a, Vector3 b, float rawDelta, InterpType type, float factor, bool inverse = false)
            {
                if (!inverse)
                {
                    return type switch
                    {
                        InterpType.CosCurve => a.Lerp(b, CosCurve(rawDelta)),
                        InterpType.CosSpeedUp => a.Lerp(b, CosSpeedUp(rawDelta)),
                        InterpType.CosSlowDown => a.Lerp(b, CosSlowDown(rawDelta)),
                        InterpType.SmoothedLinear => a.Lerp(b, SmoothedLinear(rawDelta, factor)),
                        _ => a.Lerp(b, rawDelta),
                    };
                }
                else
                {
                    return type switch
                    {
                        InterpType.CosCurve => a.Lerp(b, Inverse.CosCurve(rawDelta)),
                        InterpType.CosSpeedUp => a.Lerp(b, Inverse.CosSpeedUp(rawDelta)),
                        InterpType.CosSlowDown => a.Lerp(b, Inverse.CosSlowDown(rawDelta)),
                        InterpType.SmoothedLinear => a.Lerp(b, Inverse.SmoothedLinear(rawDelta, factor)),
                        _ => a.Lerp(b, -rawDelta),
                    };
                }
            }

            public static Vector4 Lerp(this Vector4 a, Vector4 b, float delta) => new Vector4(a.x + (b.x - a.x) * delta, a.y + (b.y - a.y) * delta, a.z + (b.z - a.z) * delta, a.w + (b.w - a.w) * delta);
            public static Vector4 Interp(this Vector4 a, Vector4 b, float rawDelta, InterpType type, bool inverse = false) => a.Interp(b, rawDelta, type, 0.5f, inverse);
            public static Vector4 Interp(this Vector4 a, Vector4 b, float rawDelta, InterpType type, float factor, bool inverse = false)
            {
                if (!inverse)
                {
                    return type switch
                    {
                        InterpType.CosCurve => a.Lerp(b, CosCurve(rawDelta)),
                        InterpType.CosSpeedUp => a.Lerp(b, CosSpeedUp(rawDelta)),
                        InterpType.CosSlowDown => a.Lerp(b, CosSlowDown(rawDelta)),
                        InterpType.SmoothedLinear => a.Lerp(b, SmoothedLinear(rawDelta, factor)),
                        _ => a.Lerp(b, rawDelta),
                    };
                }
                else
                {
                    return type switch
                    {
                        InterpType.CosCurve => a.Lerp(b, Inverse.CosCurve(rawDelta)),
                        InterpType.CosSpeedUp => a.Lerp(b, Inverse.CosSpeedUp(rawDelta)),
                        InterpType.CosSlowDown => a.Lerp(b, Inverse.CosSlowDown(rawDelta)),
                        InterpType.SmoothedLinear => a.Lerp(b, Inverse.SmoothedLinear(rawDelta, factor)),
                        _ => a.Lerp(b, -rawDelta),
                    };
                }
            }

            public static Color Lerp(this Color a, Color b, float delta) => new Color(a.r + (b.r - a.r) * delta, a.g + (b.g - a.g) * delta, a.b + (b.b - a.b) * delta, a.a + (b.a - a.a) * delta);
            public static Color Interp(this Color a, Color b, float rawDelta, InterpType type, bool inverse = false) => a.Interp(b, rawDelta, type, 0.5f, inverse);
            public static Color Interp(this Color a, Color b, float rawDelta, InterpType type, float factor, bool inverse = false)
            {
                if (!inverse)
                {
                    return type switch
                    {
                        InterpType.CosCurve => a.Lerp(b, CosCurve(rawDelta)),
                        InterpType.CosSpeedUp => a.Lerp(b, CosSpeedUp(rawDelta)),
                        InterpType.CosSlowDown => a.Lerp(b, CosSlowDown(rawDelta)),
                        InterpType.SmoothedLinear => a.Lerp(b, SmoothedLinear(rawDelta, factor)),
                        _ => a.Lerp(b, rawDelta),
                    };
                }
                else
                {
                    return type switch
                    {
                        InterpType.CosCurve => a.Lerp(b, Inverse.CosCurve(rawDelta)),
                        InterpType.CosSpeedUp => a.Lerp(b, Inverse.CosSpeedUp(rawDelta)),
                        InterpType.CosSlowDown => a.Lerp(b, Inverse.CosSlowDown(rawDelta)),
                        InterpType.SmoothedLinear => a.Lerp(b, Inverse.SmoothedLinear(rawDelta, factor)),
                        _ => a.Lerp(b, -rawDelta),
                    };
                }
            }
        }

        public static class ITime
        {
            public static float Time(bool realtime = false) => realtime ? UnityEngine.Time.unscaledTime : UnityEngine.Time.time;

            public static float DeltaTime(bool realtime = false) => realtime ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;

            public static float FixedDeltaTime(bool realtime = false) => realtime ? UnityEngine.Time.fixedUnscaledDeltaTime : UnityEngine.Time.fixedDeltaTime;

            public static IEnumerator Wait(float time, bool realtime = false)
            {
                if (realtime)
                    yield return new WaitForSecondsRealtime(time);
                else
                    yield return new WaitForSeconds(time);
            }
        }
    }
}
