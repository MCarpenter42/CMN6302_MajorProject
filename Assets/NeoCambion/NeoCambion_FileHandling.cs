namespace NeoCambion.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using NeoCambion;

    public static class Ext_Stream
    {
        public static string ReadToString(this Stream stream)
        {
            byte[] bytes = new byte[0];
            if (stream != null)
                bytes = new byte[stream.Length];
            if (bytes.Length > 0)
                return bytes.ParseToString();
            else
                return null;
        }
    }

    public static class FileHandler
    {
        #region [ JSON SETTINGS DATA ]

        public static DataContractJsonSerializerSettings jsonSettings = new DataContractJsonSerializerSettings()
        {
            DateTimeFormat = new System.Runtime.Serialization.DateTimeFormat("hh:mm:ss dd/MM/yyyy") { DateTimeStyles = System.Globalization.DateTimeStyles.AssumeLocal },
            EmitTypeInformation = System.Runtime.Serialization.EmitTypeInformation.AsNeeded,
            IgnoreExtensionDataObject = false,
            SerializeReadOnlyTypes = false
        };
        
        #endregion

        public static void Write(byte[] data, string filePathFull, bool overwrite = true)
        {
            if (!File.Exists(filePathFull))
                File.Create(filePathFull);

            if (overwrite)
            {
                File.WriteAllBytes(filePathFull, data);
            }
            else
            {
                byte[] existingData = File.ReadAllBytes(filePathFull);
                byte[] mergedData = new byte[existingData.Length + data.Length];
                int i, l = existingData.Length;
                for (i = 0; i < data.Length; i++)
                {
                    mergedData[l + i] = data[i];
                }
                File.WriteAllBytes(filePathFull, mergedData);
            }
        }
        
        public static void Write(string data, string filePathFull, bool overwrite = true)
        {
            if (!File.Exists(filePathFull))
                File.Create(filePathFull);

            if (overwrite)
            {
                File.WriteAllText(filePathFull, data);
            }
            else
            {
                string existingData = File.ReadAllText(filePathFull);
                File.WriteAllText(filePathFull, existingData + data);
            }
        }
        
        public static void Write(string[] data, string filePathFull, bool overwrite = true)
        {
            if (!File.Exists(filePathFull))
                File.Create(filePathFull);

            if (overwrite)
            {
                File.WriteAllLines(filePathFull, data);
            }
            else
            {
                string[] existingData = File.ReadAllLines(filePathFull);
                string[] mergedData = new string[existingData.Length + data.Length];
                int i, l = existingData.Length;
                for (i = 0; i < data.Length; i++)
                {
                    mergedData[l + i] = data[i];
                }
                File.WriteAllLines(filePathFull, mergedData);
            }
        }

        public static void Write(object data, string filePathFull)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(data.GetType());
            using (FileStream stream = File.Create(filePathFull))
            {
                serializer.WriteObject(stream, data);
                stream.Close();
            }
        }
        
        public static byte[] Read(string filePathFull, out byte[] target)
        {
            if (File.Exists(filePathFull))
                target = File.ReadAllBytes(filePathFull);
            else
                target = new byte[0];
            return target;
        }
        
        public static string ReadString(string filePathFull, out string target)
        {
            if (File.Exists(filePathFull))
                target = File.ReadAllText(filePathFull);
            else
                target = null;
            return target;
        }
        
        public static string[] Read(string filePathFull, out string[] target)
        {
            if (File.Exists(filePathFull))
                target = File.ReadAllLines(filePathFull);
            else
                target = new string[0];
            return target;
        }
        
        public static T ReadAs<T>(string filePathFull)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            T output;
            using (FileStream stream = File.OpenRead(filePathFull))
            {
                object obj = serializer.ReadObject(stream);
                try
                {
                    output = (T)obj;
                }
                catch
                {
                    throw new Exception("Error reading file to object: requested output type of \"" + typeof(T).ToString() + "\" is not compatible with the file contents.");
                }
                stream.Close();
            }
            return output;
        }
    }

    namespace Unity
    {
        using UnityEngine;
        using System.Runtime.Serialization.Formatters.Binary;

        public static class UnityFileHandler
        {
            public static void SaveData(object data, string fileSubPath)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                string path = Application.persistentDataPath;
                if (fileSubPath.Substring(0, 1) != "/")
                {
                    path += "/";
                }
                path += fileSubPath;

                Debug.Log("Saving data to \"" + path + "\"");
                FileStream stream = new FileStream(path, FileMode.Create);

                formatter.Serialize(stream, data);
                stream.Close();
            }

            public static object LoadData(string fileSubPath)
            {
                string path = Application.persistentDataPath;
                if (fileSubPath.Substring(0, 1) != "/")
                {
                    path += "/";
                }
                path += fileSubPath;
                Debug.Log("Loading data from \"" + path + "\"");
                if (File.Exists(path))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    FileStream stream = new FileStream(path, FileMode.Open);

                    object obj = formatter.Deserialize(stream);

                    stream.Close();
                    return obj;
                }
                else
                {
                    Debug.LogError("File not found, returning null");
                    return null;
                }
            }
        }
    }
}