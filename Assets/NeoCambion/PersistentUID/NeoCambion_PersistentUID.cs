namespace NeoCambion.Unity.PersistentUID
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEditor;

    using NeoCambion.Collections;

    public abstract class PersistentUID : MonoBehaviour
    {
        public static bool displayAsHex = true;
        public int UID = 0;
        public string hexUID
        {
            get
            {
                return UID.ParseToHexString();
            }
            set
            {
                if (value.IsHexadecimal())
                    UID = value.HexStringToInt();
            }
        }

        public void ForceUpdateUID()
        {
            PrefabUID[] uidComponents = PersistentUID_Utility.GetPrefabsWithUID("");
            int[] UIDs = new int[uidComponents.Length];
            for (int i = 0; i < uidComponents.Length; i++)
            {
                UIDs[i] = uidComponents[i].UID;
            }
            int newUID;
            for (int j = 0; j < 20; j++)
            {
                newUID = Random.Range(int.MinValue, int.MaxValue);
                if (!UIDs.Contains(newUID))
                {
                    UID = newUID;
                    break;
                }
            }
            PrefabUtility.SavePrefabAsset(gameObject);
        }
    }

    public static class PersistentUID_Utility
    {
        public static GameObject[] GetObjectsWithUID()
        {
            ObjectUID[] uidComponents = Object.FindObjectsOfType<ObjectUID>();
            GameObject[] objs = new GameObject[uidComponents.Length];
            for (int i = 0; i < uidComponents.Length; i++)
            {
                objs[i] = uidComponents[i].gameObject;
            }
            return objs;
        }

#if UNITY_EDITOR
        public static PrefabUID[] GetPrefabsWithUID(string pathInResources = "")
        {
            return Resources.LoadAll<PrefabUID>(pathInResources);
        }
        
        public static string[] GetPrefabPathsWithUID(string pathInResources = "")
        {
            PrefabUID[] uidComponents = GetPrefabsWithUID(pathInResources);
            string[] paths = new string[uidComponents.Length];
            string path;
            for (int i = 0; i < uidComponents.Length; i++)
            {
                path = AssetDatabase.GetAssetPath(uidComponents[i].gameObject);
                path = path.Substring(path.IndexOf("/") + 1);
                path = path.Substring(path.IndexOf("/") + 1);
                paths[i] = path;
            }
            return paths;
        }

        public static string[] AssignPrefabUIDs(string pathInResources = "")
        {
            PrefabUID[] uidComponents = GetPrefabsWithUID(pathInResources);
            int[] UIDs = new int[uidComponents.Length];
            int newUID;
            string[] paths = new string[uidComponents.Length];
            for (int i = 0; i < uidComponents.Length; i++)
            {
                UIDs[i] = uidComponents[i].UID;
                paths[i] = AssetDatabase.GetAssetPath(uidComponents[i].gameObject);
                if (UIDs[i] == 0)
                {
                    for (int j = 0; j < 20; j++)
                    {
                        newUID = Random.Range(int.MinValue, int.MaxValue);
                        if (!UIDs.Contains(newUID))
                        {
                            UIDs[i] = newUID;
                            uidComponents[i].UID = newUID;
                            break;
                        }
                    }
                    PrefabUtility.SavePrefabAsset(uidComponents[i].gameObject);
                }
            }
            return paths;
        }
    }
#endif
}