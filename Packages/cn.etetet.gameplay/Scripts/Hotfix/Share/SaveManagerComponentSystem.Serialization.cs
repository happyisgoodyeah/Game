using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ET
{
    /// <summary>
    /// 序列化格式
    /// </summary>
    public enum SerializationFormat
    {
        MemoryPack,  // 运行环境：高性能二进制
        MongoDB      // 调试环境：可读JSON
    }
    
    public static partial class SaveManagerComponentSystem
    {
        
        /// <summary>
        /// 获取当前序列化格式（根据编译条件自动选择）
        /// </summary>
        public static SerializationFormat GetCurrentFormat()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // 开发/调试模式使用MongoDB格式便于查看
            return SerializationFormat.MongoDB;
#else
            // 发布版本使用MemoryPack获得最佳性能
            return SerializationFormat.MemoryPack;
#endif
        }
        
        /// <summary>
        /// 序列化存档数据
        /// </summary>
        public static byte[] SerializeSaveData(this SaveManagerComponent self, GameSaveData saveData)
        {
            if (saveData == null)
            {
                Log.Error("存档数据为空");
                return null;
            }

            //调用EntitySystemSingleton准备序列化状态
            EntitySystemSingleton.Instance.Serialize(saveData);

            byte[] serializedData;
            var format = GetCurrentFormat();

            switch (format)
            {
                case SerializationFormat.MemoryPack:
                    //使用MemoryPack进行高性能序列化
                    serializedData = MemoryPackHelper.Serialize(saveData);
                    Log.Info($"使用MemoryPack序列化存档，数据大小: {serializedData.Length} 字节");
                    break;
                    
                case SerializationFormat.MongoDB:
                    //使用MongoDB进行可读序列化（调试用）
                    //serializedData = MongoHelper.Serialize(saveData);
                    string jsonString = MongoHelper.ToJson(saveData);
                    serializedData = Encoding.UTF8.GetBytes(jsonString);
                    Log.Info($"使用MongoDB序列化存档，数据大小: {serializedData.Length} 字节");
                    break;
                    
                default:
                    throw new ArgumentOutOfRangeException($"不支持的序列化格式: {format}");
            }

            return serializedData;
        }
        
        /// <summary>
        /// 反序列化存档数据
        /// </summary>
        public static GameSaveData DeserializeSaveData(this SaveManagerComponent self, byte[] data, SerializationFormat? format = null)
        {
            if (data == null || data.Length == 0)
            {
                Log.Error("存档数据为空");
                return null;
            }

            var actualFormat = format ?? GetCurrentFormat();
            GameSaveData saveData;

            switch (actualFormat)
            {
                case SerializationFormat.MemoryPack:
                    // 使用MemoryPack反序列化
                    saveData = (GameSaveData)MemoryPackHelper.Deserialize(typeof(GameSaveData) , data , 0 , data.Length);
                    Log.Info($"使用MemoryPack反序列化存档");
                    break;
                    
                case SerializationFormat.MongoDB:
                    // 使用MongoDB反序列化
                    //saveData = (GameSaveData)MongoHelper.Deserialize(typeof(GameSaveData), data);
                    string jsonString = Encoding.UTF8.GetString(data);
                    saveData = (GameSaveData)MongoHelper.FromJson(typeof(GameSaveData), jsonString);
                    Log.Info($"使用MongoDB反序列化存档");
                    break;
                    
                default:
                    throw new ArgumentOutOfRangeException($"不支持的序列化格式: {actualFormat}");
            }

            if (saveData == null)
                throw new Exception("存档数据反序列化失败");

            // 步骤2: 调用EntitySystemSingleton恢复实体状态
            EntitySystemSingleton.Instance.Deserialize(saveData);

            return saveData;
        }
        
        /// <summary>
        /// 查找存档文件（支持多种格式）
        /// </summary>
        private static string FindSaveFile(this SaveManagerComponent self, string fileNameWithoutExtension)
        {
            string extension = self.GetSaveFileExtension();
            
            string filePath = Path.Combine(self.SaveDirectory, fileNameWithoutExtension + extension);
            if (File.Exists(filePath))
            {
                return filePath;
            }
            
            // string[] possibleExtensions = { ".sav", ".json", ".dat" };
            //
            // foreach (string extension in possibleExtensions)
            // {
            //     string filePath = Path.Combine(self.SaveDirectory, fileNameWithoutExtension + extension);
            //     if (File.Exists(filePath))
            //     {
            //         return filePath;
            //     }
            // }
            
            return null;
        }
        
        /// <summary>
        /// 自动检测并反序列化存档数据
        /// </summary>
        public static GameSaveData AutoDetectAndDeserialize(this SaveManagerComponent self, string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"存档文件不存在: {filePath}");

            byte[] fileData = File.ReadAllBytes(filePath);
            
            // 尝试检测文件格式
            var format = DetectSerializationFormat(fileData);
            Log.Info($"检测到存档文件格式: {format}");

            return self.DeserializeSaveData(fileData, format);
        }

        /// <summary>
        /// 检测序列化格式
        /// </summary>
        private static SerializationFormat DetectSerializationFormat(byte[] data)
        {
            if (data.Length == 0)
                throw new ArgumentException("数据为空");

            return GetCurrentFormat();
            
            // MemoryPack数据通常以特定的魔术数字开头
            // MongoDB JSON数据通常以 { 开头 (UTF8编码)
/*
            if (data.Length >= 2 && data[0] == 0x7B && data[1] == 0x22) // {" 开头
            {
                return SerializationFormat.MongoDB;
            }
            
            // 默认为MemoryPack格式
            return SerializationFormat.MemoryPack;
*/
        }

        /// <summary>
        /// 获取存档文件扩展名
        /// </summary>
        public static string GetSaveFileExtension(this SaveManagerComponent self)
        {
            var format = GetCurrentFormat();
            return format switch
            {
                SerializationFormat.MemoryPack => ".sav",
                SerializationFormat.MongoDB => ".json",
                _ => ".dat"
            };
        }

        /// <summary>
        /// 计算数据校验和
        /// </summary>
        public static string ComputeChecksum(this SaveManagerComponent self, byte[] data)
        {
            return string.Empty;
            // using var sha256 = SHA256.Create();
            // var hash = sha256.ComputeHash(data);
            // return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}