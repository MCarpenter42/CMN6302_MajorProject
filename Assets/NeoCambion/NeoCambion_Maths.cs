namespace NeoCambion
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    namespace Maths
    {
        public static class Ext_Mathf
        {
            public static float ToRad(this float degrees)
            {
                return degrees * Mathf.PI / 180.0f;
            }

            public static float ToDeg(this float radians)
            {
                return radians * 180.0f / Mathf.PI;
            }

            public static int Clamp(this int value, int min, int max)
            {
                if (value < min)
                {
                    return min;
                }
                else if (value > max)
                {
                    return max;
                }
                else
                {
                    return value;
                }
            }

            public static float WrapClamp(this float value, float min, float max)
            {
                float range = max - min;
                if (value < min)
                {
                    float diff = min - value;
                    int mult = (int)((diff - (diff % range)) / range) + 1;
                    return value + (float)mult * range;
                }
                else if (value > max)
                {
                    float diff = value - max;
                    int mult = (int)((diff - (diff % range)) / range) + 1;
                    return value - (float)mult * range;
                }
                else
                {
                    return value;
                }
            }

            public static int WrapClamp(this int value, int min, int max)
            {
                int range = max - min + 1;
                if (value < min)
                {
                    int diff = min - value;
                    int mult = diff > range ? ((diff - (diff % range)) / range) + 1 : 1;
                    return value + mult * range;
                }
                else if (value > max)
                {
                    int diff = value - max;
                    int mult = diff > range ? ((diff - (diff % range)) / range) + 1 : 1;
                    return value - mult * range;
                }
                else
                {
                    return value;
                }
            }
        }

        public static class BoolConversion
        {
            public static int ToInt(this bool intBool)
            {
                if (intBool)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            public static int ToInt(this bool intBool, int trueVal, int falseVal)
            {
                if (intBool)
                {
                    return trueVal;
                }
                else
                {
                    return falseVal;
                }
            }

            public static bool ToBool(this int boolInt)
            {
                if (boolInt > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static class VectorMaths
        {
            public static float Angle2D(this Vector2 vect)
            {
                vect.Normalize();
                float angle = Vector2.Angle(Vector2.up, vect);
                if (vect.x >= 0.0f)
                {
                    return angle;
                }
                else
                {
                    return -angle;
                }
            }
            
            public static float Angle2D(this Vector3 vect, DualAxis axes = DualAxis.XZ)
            {
                Vector2 vect2;
                switch (axes)
                {
                    case DualAxis.XY:
                        vect2 = new Vector2(vect.x, vect.y);
                        break;
                    default:
                    case DualAxis.XZ:
                        vect2 = new Vector2(vect.x, vect.z);
                        break;
                    case DualAxis.YZ:
                        vect2 = new Vector2(vect.y, vect.z);
                        break;
                }
                vect2.Normalize();
                float angle = Vector2.Angle(Vector2.up, vect2);
                if (vect2.x >= 0.0f)
                {
                    return angle;
                }
                else
                {
                    return -angle;
                }
            }

            public static float AngleFromAxis(this Vector3 vect, DualAxis plane, bool measureFromSecond)
            {
                Vector2 vectConv;
                switch (plane)
                {
                    case DualAxis.XY:
                        if (measureFromSecond)
                        {
                            vectConv.x = vect.y;
                            vectConv.y = vect.x;
                        }
                        else
                        {
                            vectConv.x = vect.x;
                            vectConv.y = vect.y;
                        }
                        break;

                    default:
                    case DualAxis.XZ:
                        if (measureFromSecond)
                        {
                            vectConv.x = vect.z;
                            vectConv.y = vect.x;
                        }
                        else
                        {
                            vectConv.x = vect.x;
                            vectConv.y = vect.z;
                        }
                        break;

                    case DualAxis.YZ:
                        if (measureFromSecond)
                        {
                            vectConv.x = vect.z;
                            vectConv.y = vect.y;
                        }
                        else
                        {
                            vectConv.x = vect.y;
                            vectConv.y = vect.z;
                        }
                        break;
                }
                vectConv.Normalize();
                float angle = Vector2.Angle(Vector2.up, vectConv);
                if (vectConv.x >= 0.0f)
                {
                    return angle;
                }
                else
                {
                    return -angle;
                }
            }

            public static Vector3 Rotate(this Vector3 vect, Vector3 rot)
            {
                float cosA = Mathf.Cos(rot.z);
                float sinA = Mathf.Sin(rot.z);
                float cosB = Mathf.Cos(rot.y);
                float sinB = Mathf.Sin(rot.y);
                float cosC = Mathf.Cos(rot.x);
                float sinC = Mathf.Sin(rot.x);

                float Rxx = cosA * cosB;
                float Rxy = cosA * sinB * sinC - sinA * cosC;
                float Rxz = cosA * sinB * cosC + sinA * sinC;
                float Ryx = sinA * cosB;
                float Ryy = sinA * sinB * sinC + cosA * cosC;
                float Ryz = sinA * sinB * cosC - cosA * sinC;
                float Rzx = -sinB;
                float Rzy = cosB * sinC;
                float Rzz = cosB * cosC;

                float x = vect.x * Rxx + vect.y * Rxy + vect.z * Rxz;
                float y = vect.x * Ryx + vect.y * Ryy + vect.z * Ryz;
                float z = vect.x * Rzx + vect.y * Rzy + vect.z * Rzz;

                vect = new Vector3(x, y, z);

                return vect;
            }

            public static Vector3 Rotate(this Vector3 vect, Vector3 rot, Vector3 centre)
            {
                Vector3 vectRel = vect - centre;
                vectRel = vectRel.Rotate(rot);
                vect = vectRel + centre;
                return vect;
            }

            public static Vector3 Scale(this Vector3 vect, float scale)
            {
                vect *= scale;
                return vect;
            }

            public static Vector3 Scale(this Vector3 vect, Vector3 scale)
            {
                vect.x *= scale.x;
                vect.y *= scale.y;
                vect.z *= scale.z;
                return vect;
            }

            public static Vector3 Scale(this Vector3 vect, float scale, Vector3 centre)
            {
                Vector3 vectRel = vect - centre;
                vectRel = vectRel.Scale(scale);
                vect = vectRel + centre;
                return vect;
            }

            public static Vector3 Scale(this Vector3 vect, Vector3 scale, Vector3 centre)
            {
                Vector3 vectRel = vect - centre;
                /*vectRel = */vectRel.Scale(scale);
                vect = vectRel + centre;
                return vect;
            }
        }
    }
}