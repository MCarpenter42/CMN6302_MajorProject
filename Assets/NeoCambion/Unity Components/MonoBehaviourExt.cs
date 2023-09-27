namespace NeoCambion.Unity
{
    using UnityEngine;
    using UnityEditor;

#if UNITY_EDITOR
    [CanEditMultipleObjects]
#endif
    public class MonoBehaviourExt : MonoBehaviour
    {
        public static GameObject NewObject(Vector3 position, params System.Type[] components) => NewGameObject.Get(position, components);
        public static GameObject NewObject(Quaternion rotation, params System.Type[] components) => NewGameObject.Get(rotation, components);
        public static GameObject NewObject(Vector3 position, Quaternion rotation, params System.Type[] components) => NewGameObject.Get(position, rotation, components);
        public static GameObject NewObject(Transform parent, params System.Type[] components) => NewGameObject.Get(parent, components);
        public static GameObject NewObject(Transform parent, Vector3 position, params System.Type[] components) => NewGameObject.Get(parent, position, components);
        public static GameObject NewObject(Transform parent, Quaternion rotation, params System.Type[] components) => NewGameObject.Get(parent, rotation, components);
        public static GameObject NewObject(Transform parent, Vector3 position, Quaternion rotation, params System.Type[] components) => NewGameObject.Get(parent, position, rotation, components);
        public static GameObject NewObject(string name, Vector3 position, params System.Type[] components) => NewGameObject.Get(name, position, components);
        public static GameObject NewObject(string name, Quaternion rotation, params System.Type[] components) => NewGameObject.Get(name, rotation, components);
        public static GameObject NewObject(string name, Vector3 position, Quaternion rotation, params System.Type[] components) => NewGameObject.Get(name, position, rotation, components);
        public static GameObject NewObject(string name, Transform parent, params System.Type[] components) => NewGameObject.Get(name, parent, components);
        public static GameObject NewObject(string name, Transform parent, Vector3 position, params System.Type[] components) => NewGameObject.Get(name, parent, position, components);
        public static GameObject NewObject(string name, Transform parent, Quaternion rotation, params System.Type[] components) => NewGameObject.Get(name, parent, rotation, components);
        public static GameObject NewObject(string name, Transform parent, Vector3 position, Quaternion rotation, params System.Type[] components) => NewGameObject.Get(name, parent, position, rotation, components);

        /* - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - */

        public static int RandomInclusive(int minInclusive, int maxInclusive) => Random.Range(minInclusive, maxInclusive + 1);
    }
}