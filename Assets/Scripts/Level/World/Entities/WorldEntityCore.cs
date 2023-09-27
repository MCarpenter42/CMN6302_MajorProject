using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using NeoCambion;
using NeoCambion.Maths;
using NeoCambion.Unity;

#if UNITY_EDITOR
using UnityEditor;
using NeoCambion.Unity.Editor;

[CustomEditor(typeof(WorldEntityCore), true)]
[CanEditMultipleObjects]
public class WorldEntityCoreEditor : Editor
{
    WorldEntityCore targ { get { return target as WorldEntityCore; } }
    public Rect rect, toggleRect;
    string[] propertiesInBaseClass;

    void OnEnable()
    {
        FieldInfo[] fields = typeof(WorldEntityCore).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        propertiesInBaseClass = new string[fields.Length];
        for (int i = 0; i < fields.Length; i++)
        {
            propertiesInBaseClass[i] = fields[i].Name;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorElements.SectionHeader("Entity Core");
        int w = 16;
        rect = EditorElements.ControlRect();
        toggleRect = new Rect(rect) { width = w, x = rect.x + 1 };
        targ.disabled = EditorGUI.Toggle(toggleRect, targ.disabled);
        rect.x += w + 2; rect.width -= w;
        EditorGUI.LabelField(rect, "Disabled");
        EditorGUILayout.Space(1);
        Transform pivot = EditorGUILayout.ObjectField("Pivot", targ.pivot, typeof(Transform), true) as Transform;
        if (pivot != targ.pivot)
        {
            targ.pivot = pivot;
            targ.UpdateModelObject(false);
        }
        EntityModel model = EditorGUILayout.ObjectField("Model", targ.model, typeof(EntityModel), true) as EntityModel;
        if (model != targ.model)
        {
            targ.model = model;
            targ.UpdateModelObject(true);
        }
        targ.maxSpeed = EditorGUILayout.FloatField("Maximum Speed", targ.maxSpeed);
        DrawPropertiesExcluding(serializedObject, propertiesInBaseClass);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif

public class WorldEntityCore : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [Header("Entity Core")]
    public Transform pivot;
    public EntityModel model;
    protected GameObject modelObj;
    protected Rigidbody _rb;
    public Rigidbody rb
    {
        get
        {
            if (_rb == null)
                _rb = gameObject.GetOrAddComponent<Rigidbody>();
            return _rb;
        }
    }

    #endregion

    #region [ PROPERTIES ]

    public bool disabled = false;

    private bool hasNavMeshAgent;

    public float maxSpeed = 1.0f;
    [HideInInspector] public Vector3 velScale;
    [HideInInspector] public float facing = 0.0f;

    #endregion

    #region [ COROUTINES ]

    protected Coroutine c_modelRot = null;

    #endregion

/* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    #region [ BUILT-IN UNITY FUNCTIONS ]

    protected virtual void FixedUpdate()
    {
        if (!disabled && !hasNavMeshAgent)
        {
            if (velScale.magnitude > 0.0f)
                Move(velScale);
            else
                rb.velocity = Vector3.zero;
        }
    }

    #endregion

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */
    
    public virtual void UpdateModelObject(bool replaceModel)
    {
        if (pivot == null)
        {
            if (modelObj != null)
            {
                if (Application.isPlaying)
                    Destroy(modelObj);
#if UNITY_EDITOR
                else if (EditorApplication.isPlaying)
                    Destroy(modelObj);
#endif
                else
                    DestroyImmediate(modelObj);
            }
        }
        else
        {
            if (modelObj != null)
                modelObj.tag = "EntityModel";
            List<GameObject> modelObjs = pivot.gameObject.GetChildrenWithTag("EntityModel");
            if (modelObj != null && modelObjs.Contains(model.gameObject))
                modelObjs.Remove(model.gameObject);
            if (replaceModel)
            {
                Vector3 pos = Vector3.zero;
                Quaternion rot = Quaternion.identity;
                if (modelObjs.Count > 0)
                {
                    pos = modelObjs[0].transform.localPosition;
                    rot = modelObjs[0].transform.localRotation;
                    for (int i = modelObjs.Count - 1; i >= 0; i--)
                    {
                        if (Application.isPlaying)
                            Destroy(modelObjs[i]);
#if UNITY_EDITOR
                        else if (EditorApplication.isPlaying)
                            Destroy(modelObjs[i]);
#endif
                        else
                            DestroyImmediate(modelObjs[i]);
                    }
                }
                if (model != null)
                {
                    modelObj = Instantiate(model.gameObject, pivot);
                    modelObj.name = "Model";
                    modelObj.transform.localPosition = pos;
                    modelObj.transform.localRotation = rot;
                }
            }
            else
            {
                if (modelObj == null)
                {
                    if (model != null)
                    {
                        modelObj = Instantiate(model.gameObject, pivot);
                        modelObj.name = "Model";
                        modelObj.transform.localPosition = Vector3.zero;
                        modelObj.transform.localRotation = Quaternion.identity;
                    }
                }
                else
                {
                    if (modelObjs.Count > 1)
                    {
                        modelObjs.RemoveAt(modelObjs.IndexOf(modelObj));
                        for (int i = modelObjs.Count - 1; i >= 0; i--)
                        {
                            if (Application.isPlaying)
                                Destroy(modelObjs[i]);
#if UNITY_EDITOR
                            else if (EditorApplication.isPlaying)
                                Destroy(modelObjs[i]);
#endif
                            else
                                DestroyImmediate(modelObjs[i]);
                        }
                    }
                    modelObj.transform.SetParent(pivot);
                }
            }
        }
    }

    /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

    public virtual void Move(Vector3 velocity, bool turnToFace = true)
    {
        if (!disabled)
        {
            rb.velocity = velocity * maxSpeed;
            if (turnToFace)
            {
                Vector2 vDir = new Vector2(velocity.x, velocity.z);
                float dir = vDir.Angle2D();
                RotateTo(dir);
            }
        }
    }

    public virtual void SetRot(float orientation)
    {
        if (!disabled)
        {
            facing = orientation.WrapClamp(0.0f, 360.0f);
            Vector3 rot = pivot.eulerAngles;
            rot.y = facing;
            pivot.eulerAngles = rot;
        }
    }

    public virtual void Rotate(float angle)
    {
        SetRot(facing + angle);
    }

    protected virtual void RotateTo(float angle, float duration = 0.2f)
    {
        if (facing != angle && !disabled)
        {
            float fPrev = facing;
            facing = angle;
            if (c_modelRot != null)
                StopCoroutine(c_modelRot);
            c_modelRot = StartCoroutine(IRotateTo(fPrev, angle, duration));
        }
    }

    protected virtual IEnumerator IRotateTo(float start, float end, float duration)
    {
        float rotDiff = (end - start).WrapClamp(-180.0f, 180.0f);
        float time = 0.0f;
        float delta;
        while (time <= duration)
        {
            yield return null;
            time += Time.deltaTime;
            delta = time / duration;
            SetRot(start + rotDiff * delta);
        }
        SetRot(end);
    }
}
