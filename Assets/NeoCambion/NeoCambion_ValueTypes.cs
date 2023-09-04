namespace NeoCambion
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    [System.Serializable]
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

    [System.Serializable]
    public struct FloatRange
    {
        private float _lower;
        public float lower
        {
            get { return _lower; }
            set
            {
                if (value > _upper)
                {
                    _lower = _upper;
                    _upper = value;
                }
                else
                {
                    _lower = value;
                }
                range = _upper - _lower;
            }
        }
        private float _upper;
        public float upper
        {
            get { return _lower; }
            set
            {
                if (value < _lower)
                {
                    _upper = _lower;
                    _lower = value;
                }
                else
                {
                    _upper = value;
                }
                range = _upper - _lower;
            }
        }

        public float range { get; private set; }

        public FloatRange(float lower, float upper)
        {
            if (lower > upper)
            {
                _upper = lower;
                _lower = upper;
            }
            else
            {
                _lower = lower;
                _upper = upper;
            }
            range = _upper - _lower;
        }

        public static FloatRange Nil { get { return new FloatRange(0, 0); } }
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

        [System.Serializable]
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

        [System.Serializable]
        public enum CameraProjection { Custom = -1, Perspective, Orthographic }
        [System.Serializable]
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
    }
}