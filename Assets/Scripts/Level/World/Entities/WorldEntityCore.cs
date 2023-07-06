using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

using NeoCambion;
using NeoCambion.Maths;
using NeoCambion.Unity;

[CustomEditor(typeof(WorldEntityCore), true)]
[CanEditMultipleObjects]
public class WorldEntityCoreEditor : Editor
{
    SerializedProperty pivot;
    SerializedProperty model;
    SerializedProperty maxSpeed;

    string[] propertiesInBaseClass;

    void OnEnable()
    {
        pivot = serializedObject.FindProperty("pivot");
        model = serializedObject.FindProperty("model");
        maxSpeed = serializedObject.FindProperty("maxSpeed");

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
        SerializedProperty _pivot = pivot;
        EditorGUILayout.PropertyField(_pivot, new GUIContent("Pivot"));
        if (_pivot != pivot)
        {
            pivot = _pivot;
            ((WorldEntityCore)target).UpdateModelObject(false);
        }
        SerializedProperty _model = model;
        EditorGUILayout.PropertyField(_model, new GUIContent("Model"));
        {
            model = _model;
            ((WorldEntityCore)target).UpdateModelObject(true);
        }
        EditorGUILayout.PropertyField(maxSpeed, new GUIContent("Maximum Speed"));
        DrawPropertiesExcluding(serializedObject, propertiesInBaseClass);
        //Debug.Log(propertiesInBaseClass[0]);
        serializedObject.ApplyModifiedProperties();
    }
}

public class WorldEntityCore : Core
{
    #region [ OBJECTS / COMPONENTS ]

    [Header("Entity Core")]
    [SerializeField] protected Transform pivot;
    [SerializeField] protected EntityModel model;
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

    protected virtual void Awake()
    {
        hasNavMeshAgent = GetComponent<NavMeshAgent>() != null;
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {
        if (!hasNavMeshAgent)
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
            if (modelObjs.Contains(model.gameObject))
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
        rb.velocity = velocity * maxSpeed;
        //transform.position += velocity * maxSpeed * Time.fixedDeltaTime;
        if (turnToFace)
        {
            Vector2 vDir = new Vector2(velocity.x, velocity.z);
            float dir = vDir.Angle2D();
            RotateTo(dir);
        }
    }

    public virtual void SetRot(float orientation)
    {
        facing = orientation.WrapClamp(0.0f, 360.0f);
        Vector3 rot = pivot.eulerAngles;
        rot.y = facing;
        pivot.eulerAngles = rot;
    }

    public virtual void Rotate(float angle)
    {
        SetRot(facing + angle);
    }

    protected virtual void RotateTo(float angle, float duration = 0.2f)
    {
        if (facing != angle)
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
