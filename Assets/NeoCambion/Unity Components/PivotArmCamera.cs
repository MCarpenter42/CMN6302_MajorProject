namespace NeoCambion.Unity
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    using NeoCambion;
    using NeoCambion.Unity.Editor;
    using NeoCambion.Maths;
    using static CombatManager;

    [ExecuteInEditMode]
    public class PivotArmCamera : MonoBehaviourExt
    {
        public Transform pivot;
        public Transform cameraTransform;
        public Camera cam;

        public bool allowAnchorRotation = false;

        public Vector3 position { get { return transform.position; } set { transform.position = value; } }
        public Vector3 localPosition { get { return transform.localPosition; } set { transform.localPosition = value; } }
        public Vector3 localScale { get { return transform.localScale; } set { transform.localScale = value; } }

        public Quaternion rotation { get { return pivot.rotation; } set { pivot.rotation = value; _pivotRotation = pivot.localRotation; } }
        public Quaternion localRotation { get { return pivot.localRotation; } set { pivot.localRotation = value; _pivotRotation = pivot.localRotation; } }
        public Vector3 eulerAngles { get { return pivot.eulerAngles; } set { pivot.eulerAngles = value; _pivotRotation = pivot.localRotation; } }
        public Vector3 localEulerAngles { get { return pivot.localEulerAngles; } set { pivot.localEulerAngles = value; _pivotRotation = pivot.localRotation; } }
        public Vector3 rot3 { get { return pivot.eulerAngles; } set { pivot.eulerAngles = value; _pivotRotation = pivot.localRotation; } }
        public Vector3 localRot3 { get { return pivot.localEulerAngles; } set { pivot.localEulerAngles = value; _pivotRotation = pivot.localRotation; } }
        private Quaternion _pivotRotation = Quaternion.identity;

        public float camOffset { get { return -cameraTransform.localPosition.z; } set { cameraTransform.localPosition = new Vector3(0f, 0f, -value); _camOffset = value; } }
        public Quaternion camRotation { get { return cameraTransform.localRotation; } set { cameraTransform.localRotation = value; _camRotation = cameraTransform.localRotation; } }
        public Vector3 camEulerAngles { get { return cameraTransform.localEulerAngles; } set { cameraTransform.localEulerAngles = value; _camRotation = cameraTransform.localRotation; } }
        private float _camOffset = 0f;
        private Quaternion _camRotation = Quaternion.identity;

        public CameraAngle currentViewAngle => new CameraAngle(position, eulerAngles, camOffset);

        void Awake()
        {
            if (pivot == null)
            {
                if (transform.childCount == 0)
                {
                    pivot = NewObject("Pivot", transform).transform;
                    pivot.localPosition = Vector3.zero;
                    pivot.localRotation = _pivotRotation;
                }
                else
                {
                    pivot = transform.GetChild(0);
                    pivot.gameObject.name = "Pivot";
                    pivot.localPosition = Vector3.zero;
                    pivot.localRotation = _pivotRotation;
                }
            }

            if (cameraTransform == null)
            {
                if (pivot.childCount == 0)
                {
                    cameraTransform = NewObject("Camera", pivot).GetOrAddComponent<Camera>().transform;
                    cameraTransform.localPosition = new Vector3(0f, 0f, -_camOffset);
                    cameraTransform.localRotation = _camRotation;
                }
                else
                {
                    cameraTransform = pivot.GetComponentInChildren<Camera>().transform;
                    if (cameraTransform == null)
                    {
                        cameraTransform = pivot.GetChild(0);
                        cameraTransform.gameObject.name = "Camera";
                        cameraTransform.localPosition = new Vector3(0f, 0f, -_camOffset);
                        cameraTransform.localRotation = _camRotation;
                    }
                }
            }

            if (cam == null)
            {
                cam = cameraTransform.gameObject.GetOrAddComponent<Camera>();
            }
        }

        void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                TransferRotation();
#endif
        }

        void OnValidate()
        {
            if (!Application.isPlaying)
                TransferRotation();
        }

        private void TransferRotation()
        {
            if (!allowAnchorRotation && transform.rotation != Quaternion.identity)
            {
                Vector3 rot = transform.eulerAngles;
                transform.eulerAngles = Vector3.zero;
                pivot.Rotate(rot);
            }
        }

        public void SetViewAngle(CameraAngle cameraAngle)
        {
            position = cameraAngle.position;
            eulerAngles = cameraAngle.rotation;
            camOffset = cameraAngle.distance;
        }
    }

    public struct CameraAngle
    {
        public Vector3 position;
        public Vector3 rotation;
        public float distance;

        public CameraAngle(Vector3 position, Vector3 rotation, float distance)
        {
            this.position = position;
            this.rotation.x = rotation.x.WrapClamp(-180f, 180f);
            this.rotation.y = rotation.y.WrapClamp(0, 360f);
            this.rotation.z = rotation.z.WrapClamp(-180f, 180f);
            this.distance = distance;
        }
        public CameraAngle(CameraAngle template, Vector3 rotOffset)
        {
            position = template.position;
            rotation.x = (template.rotation.x + rotOffset.x).WrapClamp(-180f, 180f);
            rotation.y = (template.rotation.y + rotOffset.y).WrapClamp(0, 360f);
            rotation.z = (template.rotation.z + rotOffset.z).WrapClamp(-180f, 180f);
            distance = template.distance;
        }
    }

    [CustomEditor(typeof(PivotArmCamera))]
    [CanEditMultipleObjects]
    public class PivotArmCameraEditor : UnityEditor.Editor
    {
        PivotArmCamera targ { get { return target as PivotArmCamera; } }
        float checkboxSize = 16;

        public override void OnInspectorGUI()
        {
            EditorElements.BeginHorizVert(EditorStyles.inspectorDefaultMargins, EditorStyles.inspectorFullWidthMargins);
            {
                Rect rect = EditorElements.ControlRect(), boxRect = new Rect(rect);
                boxRect.width = checkboxSize;
                targ.allowAnchorRotation = EditorGUI.Toggle(boxRect, targ.allowAnchorRotation);
                rect.x += checkboxSize;
                rect.width -= checkboxSize;
                EditorGUI.LabelField(rect, new GUIContent("Allow Anchor Rotation"));
                EditorGUILayout.Space(1);
                targ.localEulerAngles = EditorGUILayout.Vector3Field(new GUIContent("Pivot Rotation"), targ.localEulerAngles);
                EditorGUILayout.Space(1);
                targ.camOffset = EditorGUILayout.FloatField(new GUIContent("Camera Offset"), targ.camOffset);
                EditorGUILayout.Space(1);
                targ.camEulerAngles = EditorGUILayout.Vector3Field(new GUIContent("Camera Rotation"), targ.camEulerAngles);
                EditorGUILayout.Space(1);
            }
            EditorElements.EndHorizVert();
        }
    }
}

