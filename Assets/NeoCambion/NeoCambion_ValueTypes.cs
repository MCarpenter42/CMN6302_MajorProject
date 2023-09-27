namespace NeoCambion
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    [Serializable]
    public struct HexByte
    {
        public readonly byte byteValue;
        public readonly char[] charValues { get { return byteValue.ParseToHexChars(); } }
        public readonly string stringValue { get { return byteValue.ParseToHexString(); } }

        public HexByte(byte byteValue)
        {
            this.byteValue = byteValue;
        }

        public HexByte(char hex1, char hex0)
        {
            byteValue = Ext_Char.ParseHexToByte(hex1, hex0);
        }
    }

    public interface NumericRange<T>
    {
        public T Lower { get; set; }
        public T Upper { get; set; }
        public T Range { get; }
    }

    [Serializable]
    public struct FloatRange : NumericRange<float>
    {
        public float lower;
        public float Lower { get { return lower; } set { lower = value; } }
        public float upper;
        public float Upper { get { return upper; } set { upper = value; } }

        public float Range => Upper - Lower;

        public FloatRange(float lower, float upper)
        {
            if (lower > upper)
            {
                this.upper = lower;
                this.lower = upper;
            }
            else
            {
                this.lower = lower;
                this.upper = upper;
            }
        }

        public static FloatRange Nil => new FloatRange(0, 0);
        public static FloatRange Max => new FloatRange(float.MinValue, float.MaxValue);
    }
    
    [Serializable]
    public struct IntRange : NumericRange<int>
    {
        public int lower;
        public int Lower { get { return lower; } set { lower = value; } }
        public int upper;
        public int Upper { get { return upper; } set { upper = value; } }

        public int Range => Upper - Lower;

        public IntRange(int lower, int upper)
        {
            if (lower > upper)
            {
                this.upper = lower;
                this.lower = upper;
            }
            else
            {
                this.lower = lower;
                this.upper = upper;
            }
        }

        public static IntRange Nil => new IntRange(0, 0);
        public static IntRange Max => new IntRange(int.MinValue, int.MaxValue);
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    // WORK IN PROGRESS
    public class Numeric
    {
        public enum Type
        {
            Sbyte,
            Byte,
            Short,
            Ushort,
            Int,
            Uint,
            Long,
            Ulong,
            Nint,
            Nuint,

            Float,
            Double
        }

        public double value;
        public readonly Type type;

        public Numeric(sbyte value) { this.value = value; type = Type.Sbyte; }
        public Numeric(byte value) { this.value = value; type = Type.Byte; }
        public Numeric(short value) { this.value = value; type = Type.Short; }
        public Numeric(ushort value) { this.value = value; type = Type.Ushort; }
        public Numeric(int value) { this.value = value; type = Type.Int; }
        public Numeric(uint value) { this.value = value; type = Type.Uint; }
        public Numeric(long value) { this.value = value; type = Type.Long; }
        public Numeric(ulong value) { this.value = value; type = Type.Ulong; }
        public Numeric(nint value) { this.value = value; type = Type.Nint; }
        public Numeric(nuint value) { this.value = value; type = Type.Nuint; }
        public Numeric(float value) { this.value = value; type = Type.Float; }
        public Numeric(double value) { this.value = value; type = Type.Double; }


    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    namespace Unity
    {
        using UnityEngine;

        [Serializable]
        public struct CameraState
        {
            public CameraClearFlags clearFlags;
            public Color backgroundColor;
            public int cullingMask;
            public CameraViewState view;
            public bool usePhysicalProperties;
            public float depth;
            public RenderingPath renderingPath;
            public RenderTexture targetTexture;
            public bool useOcclusionCulling;
            public bool allowHDR;
            public bool allowMSAA;
            public bool allowDynamicResolution;
            public int targetDisplay;

            public CameraState(Camera stateSource)
            {
                if (stateSource == null)
                {
                    clearFlags = default_clearFlags;
                    backgroundColor = default_backgroundColor;
                    cullingMask = default_cullingMask;
                    view = new CameraViewState(null);
                    usePhysicalProperties = default_usePhysicalProperties;
                    depth = default_depth;
                    renderingPath = default_renderingPath;
                    targetTexture = default_targetTexture;
                    useOcclusionCulling = default_useOcclusionCulling;
                    allowHDR = default_allowHDR;
                    allowMSAA = default_allowMSAA;
                    allowDynamicResolution = default_allowDynamicResolution;
                    targetDisplay = default_targetDisplay;
                }
                else
                {
                    clearFlags = stateSource.clearFlags;
                    backgroundColor = stateSource.backgroundColor;
                    cullingMask = stateSource.cullingMask;
                    view = new CameraViewState(stateSource);
                    usePhysicalProperties = stateSource.usePhysicalProperties;
                    depth = stateSource.depth;
                    renderingPath = stateSource.renderingPath;
                    targetTexture = stateSource.targetTexture;
                    useOcclusionCulling = stateSource.useOcclusionCulling;
                    allowHDR = stateSource.allowHDR;
                    allowMSAA = stateSource.allowMSAA;
                    allowDynamicResolution = stateSource.allowDynamicResolution;
                    targetDisplay = stateSource.targetDisplay;
                }
            }

            public static CameraState Default
            {
                get
                {
                    return new CameraState()
                    {
                        clearFlags = default_clearFlags,
                        backgroundColor = default_backgroundColor,
                        cullingMask = default_cullingMask,
                        view = CameraViewState.Default,
                        usePhysicalProperties = default_usePhysicalProperties,
                        depth = default_depth,
                        renderingPath = default_renderingPath,
                        targetTexture = default_targetTexture,
                        useOcclusionCulling = default_useOcclusionCulling,
                        allowHDR = default_allowHDR,
                        allowMSAA = default_allowMSAA,
                        allowDynamicResolution = default_allowDynamicResolution,
                        targetDisplay = default_targetDisplay
                    };
                }
            }

            // DEFAULT ATTRIBUTE VALUES
            public static CameraClearFlags default_clearFlags = CameraClearFlags.Skybox;
            public static Color default_backgroundColor = new Color(0.1921569f, 0.3019608f, 0.4745098f, 0.0000000f);
            public static int default_cullingMask = 55;
            public static bool default_usePhysicalProperties = false;
            public static float default_depth = 0;
            public static RenderingPath default_renderingPath = RenderingPath.UsePlayerSettings;
            public static RenderTexture default_targetTexture = null;
            public static bool default_useOcclusionCulling = true;
            public static bool default_allowHDR = true;
            public static bool default_allowMSAA = true;
            public static bool default_allowDynamicResolution = false;
            public static int default_targetDisplay = 0;

            public void Apply(Camera target)
            {
                target.clearFlags = clearFlags;
                target.backgroundColor = backgroundColor;
                target.cullingMask = cullingMask;
                view.Apply(target);
                target.usePhysicalProperties = usePhysicalProperties;
                target.depth = depth;
                target.renderingPath = renderingPath;
                target.targetTexture = targetTexture;
                target.useOcclusionCulling = useOcclusionCulling;
                target.allowHDR = allowHDR;
                target.allowMSAA = allowMSAA;
                target.allowDynamicResolution = allowDynamicResolution;
                target.targetDisplay = targetDisplay;
            }
        }

        [Serializable]
        public enum CameraProjection { Custom = -1, Perspective, Orthographic }
        [Serializable]
        public struct CameraViewState
        {
            private Matrix4x4 projection;
            public CameraProjection projectionType;
            public Matrix4x4 projectionMatrix
            {
                get
                {
                    switch (projectionType)
                    {
                        case CameraProjection.Perspective:
                            return autoPerspective;
                        case CameraProjection.Orthographic:
                            return autoOrthographic;
                        default:
                            return projection;
                    }
                }
                set
                {
                    projection = value;
                }
            }
            public float fieldOfView;
            public float nearClipPlane;
            public float farClipPlane;
            public Rect viewportRect;
            public float aspect;
            public float orthographicSize;

            public CameraViewState(Camera stateSource, CameraProjection projectionType = CameraProjection.Custom)
            {
                if (stateSource == null)
                {
                    fieldOfView = default_fieldOfView;
                    nearClipPlane = default_nearClipPlane;
                    farClipPlane = default_farClipPlane;
                    viewportRect = default_viewportRect;
                    aspect = default_aspect;
                    orthographicSize = default_orthographicSize;
                    if (projectionType == CameraProjection.Orthographic)
                    {
                        projection = Orthographic(orthographicSize, default_aspect, default_nearClipPlane, default_farClipPlane);
                        this.projectionType = CameraProjection.Orthographic;
                    }
                    else
                    {
                        projection = Matrix4x4.Perspective(default_fieldOfView, default_aspect, default_nearClipPlane, default_farClipPlane);
                        this.projectionType = CameraProjection.Perspective;
                    }
                }
                else
                {
                    fieldOfView = stateSource.fieldOfView;
                    nearClipPlane = stateSource.nearClipPlane;
                    farClipPlane = stateSource.farClipPlane;
                    viewportRect = stateSource.rect;
                    aspect = stateSource.aspect;
                    orthographicSize = stateSource.orthographicSize;
                    if (projectionType == CameraProjection.Perspective)
                    {
                        projection = Matrix4x4.Perspective(default_fieldOfView, default_aspect, default_nearClipPlane, default_farClipPlane);
                        this.projectionType = CameraProjection.Perspective;
                    }
                    else if (projectionType == CameraProjection.Orthographic)
                    {
                        projection = Orthographic(orthographicSize, default_aspect, default_nearClipPlane, default_farClipPlane);
                        this.projectionType = CameraProjection.Orthographic;
                    }
                    else
                    {
                        projection = stateSource.projectionMatrix;
                        if (projection == Matrix4x4.Perspective(fieldOfView, aspect, nearClipPlane, farClipPlane))
                            this.projectionType = CameraProjection.Perspective;
                        else if (projection == Orthographic(orthographicSize, aspect, nearClipPlane, farClipPlane))
                            this.projectionType = CameraProjection.Orthographic;
                        else
                            this.projectionType = CameraProjection.Custom;
                    }

                }
            }

            public static CameraViewState Default
            {
                get
                {
                    return new CameraViewState()
                    {
                        projection = Matrix4x4.Perspective(default_fieldOfView, default_aspect, default_nearClipPlane, default_farClipPlane),
                        fieldOfView = default_fieldOfView,
                        nearClipPlane = default_nearClipPlane,
                        farClipPlane = default_farClipPlane,
                        viewportRect = default_viewportRect,
                        aspect = default_aspect,
                        orthographicSize = default_orthographicSize
                    };
                }
            }

            // DEFAULT ATTRIBUTE VALUES
            public static float default_fieldOfView = 60;
            public static float default_nearClipPlane = 0.3f;
            public static float default_farClipPlane = 1000;
            public static Rect default_viewportRect = new Rect(0, 0, 1, 1);
            public static float default_aspect = 16f / 9f;
            public static float default_orthographicSize = 5;

            public Matrix4x4 autoPerspective => Matrix4x4.Perspective(fieldOfView, aspect, nearClipPlane, farClipPlane);
            public Matrix4x4 autoOrthographic => Orthographic(orthographicSize, aspect, nearClipPlane, farClipPlane);
            private static Matrix4x4 Orthographic(float orthoSize, float aspect, float zNear, float zFar)
            {
                float horiz = orthoSize * aspect / 2f;
                float vert = orthoSize / 2f;
                return Matrix4x4.Ortho(-horiz, horiz, -vert, vert, zNear, zFar);
            }

            public void Apply(Camera target)
            {
                target.projectionMatrix = projectionMatrix;

            }
        }

#if UNITY_EDITOR
        namespace Editor
        {
            using UnityEditor;

            [CustomPropertyDrawer(typeof(FloatRange))]
            public class FloatRangeDrawer : PropertyDrawer
            {
                float wArrow = 28f, wFloatField;
                Rect minRect, arrowRect, maxRect;
                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    EditorGUI.BeginProperty(position, label, property);
                    {
                        position = EditorElements.PrefixLabel(position, label);
                        position.x += 2;
                        if ((position.width -= 2) > 120)
                            position.width = 120;
                        wFloatField = (position.width - wArrow) / 2f;

                        minRect = new Rect(position);
                        minRect.width = wFloatField;

                        EditorGUI.PropertyField(minRect, property.FindPropertyRelative("lower"), GUIContent.none);

                        arrowRect = new Rect(position);
                        arrowRect.width = wArrow;
                        arrowRect.x += wFloatField;
                        arrowRect.height -= 2f;

                        EditorGUI.LabelField(arrowRect, new GUIContent("-->"), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter }); ;

                        maxRect = new Rect(position);
                        maxRect.width = wFloatField;
                        maxRect.x += wFloatField + wArrow;

                        EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("upper"), GUIContent.none);

                    }
                    EditorGUI.EndProperty();
                }
            }
        }
#endif
    }
}