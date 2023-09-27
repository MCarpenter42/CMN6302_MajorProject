namespace NeoCambion.Unity
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [RequireComponent(typeof(SpriteMask))]
    public class WorldSpaceMask : MonoBehaviourExt
    {
        public SpriteMask mask { get { return GetComponent<SpriteMask>(); } }
        public Sprite sprite { get { return GetComponent<SpriteMask>().sprite; } set { GetComponent<SpriteMask>().sprite = value; } }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WorldSpaceMask))]
    public class WorldSpaceMaskEditor : UnityEditor.Editor
    {
        private WorldSpaceMask targ { get { return target as WorldSpaceMask; } }

        public override void OnInspectorGUI()
        {

        }
    }
#endif
}