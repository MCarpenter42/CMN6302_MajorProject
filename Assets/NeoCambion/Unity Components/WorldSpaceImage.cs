namespace NeoCambion.Unity
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [RequireComponent(typeof(SpriteRenderer))]
    public class WorldSpaceImage : MonoBehaviourExt
    {
        public SpriteRenderer spriteRenderer { get { return GetComponent<SpriteRenderer>(); } }
        public Sprite sprite { get { return GetComponent<SpriteRenderer>().sprite; } set { GetComponent<SpriteRenderer>().sprite = value; } }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WorldSpaceImage))]
    public class WorldSpaceImageEditor : UnityEditor.Editor
    {
        private WorldSpaceImage targ { get { return target as WorldSpaceImage; } }

        public override void OnInspectorGUI ()
        {

        }
    }
#endif
}