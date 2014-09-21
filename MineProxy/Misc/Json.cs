using System;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace MineProxy
{
    public static class Json
    {
        public static void Save(string path, object data)
        {
            string tmp = Path.GetTempFileName();
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(tmp, json, Encoding.UTF8);
            File.Copy(tmp, path, true);
            File.Delete(tmp);
        }

        public static void Save<T>(string path, T data)
        {
            string tmp = Path.GetTempFileName();
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(tmp, json, Encoding.UTF8);
            File.Copy(tmp, path, true);
            File.Delete(tmp);
        }

        public static T Load<T>(string path) where T:new()
        {
            if (File.Exists(path) == false)
            {
                Console.WriteLine("File not found \"" + path + "\"");
                return new T();
            }
            string json = File.ReadAllText(path, Encoding.UTF8);
            T o = JsonConvert.DeserializeObject<T>(json);
            return o;
        }

        public static byte[] Serialize(object data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            return Encoding.UTF8.GetBytes(json);
        }

        public static byte[] Serialize<T>(T data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            return Encoding.UTF8.GetBytes(json);
        }

        public static T Deserialize<T>(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            T o = JsonConvert.DeserializeObject<T>(json);
            return o;
        }
    }
}

