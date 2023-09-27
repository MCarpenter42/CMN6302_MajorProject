namespace NeoCambion.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Json;

    using NeoCambion;
    using NeoCambion.Collections;

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
        public static char Separator => Path.DirectorySeparatorChar;

        #region [ JSON SETTINGS DATA ]

        public static DataContractJsonSerializerSettings jsonSettings = new DataContractJsonSerializerSettings()
        {
            DateTimeFormat = new System.Runtime.Serialization.DateTimeFormat("hh:mm:ss dd/MM/yyyy") { DateTimeStyles = System.Globalization.DateTimeStyles.AssumeLocal },
            EmitTypeInformation = System.Runtime.Serialization.EmitTypeInformation.AsNeeded,
            IgnoreExtensionDataObject = false,
            SerializeReadOnlyTypes = false
        };
        
        #endregion

        public static void ValidateFile(string filePathFull)
        {
            if (!File.Exists(filePathFull))
            {
                try
                {
                    FileStream fs = new FileStream(filePathFull, FileMode.Create);
                    fs.Close();
                }
                catch (Exception caught)
                {
                    throw new Exception("Attempted to validate file at: " + filePathFull + "\n" + caught.Message, caught.InnerException);
                }
            }
        }

        public static void Write(byte[] data, string filePathFull, bool overwrite = true)
        {
            ValidateFile(filePathFull);
            if (overwrite)
                File.WriteAllBytes(filePathFull, data);
            else
                File.WriteAllBytes(filePathFull, File.ReadAllBytes(filePathFull).Combine(data));
        }
        public static void Write(string data, string filePathFull, bool overwrite = true)
        {
            ValidateFile(filePathFull);
            if (overwrite)
                File.WriteAllText(filePathFull, data);
            else
            {
                string existingData = File.ReadAllText(filePathFull);
                File.WriteAllText(filePathFull, existingData + data);
            }
        }
        public static void Write(string[] data, string filePathFull, bool overwrite = true)
        {
            ValidateFile(filePathFull);
            if (overwrite)
                File.WriteAllLines(filePathFull, data);
            else
                File.WriteAllLines(filePathFull, File.ReadAllLines(filePathFull).Combine(data));
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
        
        public static byte[] ReadBytes(string filePathFull)
        {
            if (File.Exists(filePathFull))
                return File.ReadAllBytes(filePathFull);
            else
                return new byte[0];
        }
        public static byte[] ReadBytes(string filePathFull, out byte[] target)
        {
            if (File.Exists(filePathFull))
                target = File.ReadAllBytes(filePathFull);
            else
                target = new byte[0];
            return target;
        }

        public static string ReadString(string filePathFull)
        {
            if (File.Exists(filePathFull))
                return File.ReadAllText(filePathFull);
            else
                return null;
        }
        public static string ReadString(string filePathFull, out string target)
        {
            if (File.Exists(filePathFull))
                target = File.ReadAllText(filePathFull);
            else
                target = null;
            return target;
        }

        public static string[] ReadLines(string filePathFull)
        {
            if (File.Exists(filePathFull))
                return File.ReadAllLines(filePathFull);
            else
                return new string[0];
        }
        public static string[] ReadLines(string filePathFull, out string[] target)
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

        private static bool CanDelete(string filePathFull)
        {
            if (File.Exists(filePathFull))
                return !File.GetAttributes(filePathFull).HasAnyFlag(FileAttributes.ReadOnly, FileAttributes.System);
            return false;
        }
        public static int Delete(params string[] filePaths)
        {
            int deleted = 0;
            foreach (string path in filePaths)
            {
                if (CanDelete(path))
                {
                    File.Delete(path);
                    deleted++;
                }
            }
            return deleted;
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