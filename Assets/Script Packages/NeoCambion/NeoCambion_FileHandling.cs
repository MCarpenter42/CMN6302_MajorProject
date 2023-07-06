namespace NeoCambion.IO
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using NeoCambion;

    public static class FileHandler
    {
        public enum FileFormat { Binary, String, MultiLine }

        #region [ JSON SETTINGS DATA ]

        public static DataContractJsonSerializerSettings jsonSettings = new DataContractJsonSerializerSettings()
        {
            DateTimeFormat = new System.Runtime.Serialization.DateTimeFormat("hh:mm:ss dd/MM/yyyy") { DateTimeStyles = System.Globalization.DateTimeStyles.AssumeLocal },
            EmitTypeInformation = System.Runtime.Serialization.EmitTypeInformation.AsNeeded,
            IgnoreExtensionDataObject = false,

        };
        
        #endregion

        public static string ToJson(this object obj)
        {
            string jsonString = "";
            return jsonString;
        }
        
        public static string[] ToJsonMultiline(this object obj)
        {
            List<string> lines = new List<string>() { "{" };

            lines.Add("}");
            return lines.ToArray();
        }
        
        public static T ToObject<T>(this string jsonString)
        {

            return default;
        }

        public static void Write(string[] data, string filePathFull, FileFormat format = FileFormat.Binary, string delimiter = "")
        {
            if (!File.Exists(filePathFull))
                File.Create(filePathFull);

            switch (format)
            {
                default:
                case FileFormat.Binary:
                    File.WriteAllBytes(filePathFull, data.ToSingleString(delimiter).ToBytes());
                    break;

                case FileFormat.String:
                    File.WriteAllText(filePathFull, data.ToSingleString(delimiter));
                    break;

                case FileFormat.MultiLine:
                    File.WriteAllLines(filePathFull, data);
                    break;
            }
        }
        
        public static void Write(string data, string filePathFull, FileFormat format = FileFormat.Binary)
        {
            Write(new string[] { data }, filePathFull, format == FileFormat.MultiLine ? FileFormat.String : format);
        }

        public static void Write(object data, string filePathFull, FileFormat format = FileFormat.Binary)
        {
            Write(data.ToJson(), filePathFull, format);
        }
        
        public static string[] Read(string filePathFull, FileFormat format = FileFormat.Binary, string delimiter = "")
        {
            if (File.Exists(filePathFull))
            {
                switch (format)
                {
                    default:
                    case FileFormat.Binary:
                        return new string[] { File.ReadAllBytes(filePathFull).ToString() };

                    case FileFormat.String:
                        if (delimiter.Length > 0)
                            return File.ReadAllText(filePathFull).Split(delimiter);
                        else
                            return new string[] { File.ReadAllText(filePathFull) };

                    case FileFormat.MultiLine:
                        return File.ReadAllLines(filePathFull);
                }
            }
            else
            {
                return new string[0];
            }
        }
        
        public static T ReadAs<T>(string filePathFull, FileFormat format = FileFormat.Binary)
        {
            return Read(filePathFull, format).ToSingleString().ToObject<T>();
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