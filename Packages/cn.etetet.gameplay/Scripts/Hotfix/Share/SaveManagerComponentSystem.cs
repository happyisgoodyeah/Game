using System;
using System.IO;

namespace ET
{
    /// <summary>
    /// 保存结果
    /// </summary>
    public enum SaveResult
    {
        //成功
        Success,
        //失败
        Failed,
        //验证失败
        ValidationFailed,
        //空间不足
        InsufficientSpace
    }
    
    [EntitySystemOf(typeof(SaveManagerComponent))]
    public static partial class SaveManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.SaveManagerComponent self)
        {
            self.Init().NoContext();
        }

        [EntitySystem]
        private static void Destroy(this ET.SaveManagerComponent self)
        {
            self.CurrentSaveData?.Dispose();
            self.CurrentSaveData = null;
        }

        /// <summary>
        /// 初始化 放到这里是因为需要异步
        /// </summary>
        /// <param name="self"></param>
        public static async ETTask Init(this ET.SaveManagerComponent self)
        {
            //加载存储目录
            await EventSystem.Instance.PublishAsync(self.Root() , new SaveManagerComponentLoadPath(){SaveManagerComponent = self});
            
            Log.Info($"存档管理器初始化完成，目录: {self.SaveDirectory}");
            Log.Info($"当前序列化格式: {GetCurrentFormat()}");

            if (await self.LoadAsync("test") == null)
            {
                self.CurrentSaveData = await self.CreateNewSave("test");
            }
            
            self.CurrentSaveData.GetPlayerData().UnlockedLevels.Add("1");
            await self.SaveAsync(self.CurrentSaveData);
        }
        
        /// <summary>
        /// 创建新存档
        /// </summary>
        /// <param name="self"></param>
        /// <param name="playerId">玩家ID</param>
        /// <param name="saveSlot">存档位</param>
        /// <returns></returns>
        public static async ETTask<GameSaveData> CreateNewSave(this ET.SaveManagerComponent self, string playerId, string saveSlot = "default")
        {
            //确保路径加载
            if (self.SaveDirectory.IsNullOrEmpty())
            {
                await self.WaitUntil(()=> !self.SaveDirectory.IsNullOrEmpty());
            }
            
            var saveData = self.AddChild<GameSaveData,string,string,string>(playerId , saveSlot , self.GameVersion);
            
            Log.Info($"创建新存档: PlayerId={playerId}, Slot={saveSlot}");
            return saveData;
        }

        /// <summary>
        /// 异步保存存档
        /// </summary>
        /// <param name="self"></param>
        /// <param name="saveData"></param>
        /// <param name="customPath"></param>
        /// <returns></returns>
        public static async ETTask<SaveResult> SaveAsync(this SaveManagerComponent self, GameSaveData saveData, string customPath = null)
        {
            //确保路径加载
            if (self.SaveDirectory.IsNullOrEmpty())
            {
                await self.WaitUntil(()=> !self.SaveDirectory.IsNullOrEmpty());
            }
            
            try
            {
                if (saveData == null)
                {
                    Log.Error("保存的存档数据为空");
                    return SaveResult.Failed;
                }
                
                // 验证存档数据
                if (!saveData.Validate())
                {
                    Log.Error("存档数据验证失败");
                    return SaveResult.ValidationFailed;
                }
                
                // 更新存档信息
                saveData.UpdateSaveTime();
                
                byte[] serializeSaveData = self.SerializeSaveData(saveData);
                
                //更新存档头信息
                var header = saveData.GetHeader();
                header.SaveSize = serializeSaveData.Length;
                header.Checksum = self.ComputeChecksum(serializeSaveData);

                //写入路径
                string fileName = $"save_{header.PlayerId}_{header.SaveSlot}{self.GetSaveFileExtension()}";
                string filePath = customPath ?? Path.Combine(self.SaveDirectory, fileName);

                //异步写入文件
                await File.WriteAllBytesAsync(filePath, serializeSaveData);

                //开发模式下保存另一种格式用于调试
                await self.SaveDebugFormat(filePath , saveData);
                
                Log.Info($"存档保存成功: {filePath}, Size: {header.SaveSize} bytes, Checksum: {header.Checksum}");
                
                return SaveResult.Success;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return SaveResult.Failed;
            }
        }

        /// <summary>
        /// 异步加载存档
        /// </summary>
        public static async ETTask<GameSaveData> LoadAsync(this SaveManagerComponent self, string playerId, string saveSlot = "default")
        {
            try
            {
                string fileName = $"save_{playerId}_{saveSlot}";
                string filePath = self.FindSaveFile(fileName);
                
                if (string.IsNullOrEmpty(filePath))
                {
                    Log.Warning($"未找到存档文件: {fileName}");
                    return null;
                }

                Log.Info($"开始加载存档: {filePath}");
                
                // 自动检测格式并加载
                GameSaveData saveData = self.AutoDetectAndDeserialize(filePath);
                
                if (saveData == null)
                {
                    Log.Error("存档数据反序列化失败");
                    return null;
                }
                
                // 验证数据完整性
                byte[] fileData = await File.ReadAllBytesAsync(filePath);
                var computedChecksum = self.ComputeChecksum(fileData);
                var header = saveData.GetHeader();
                
                if (header.Checksum != computedChecksum)
                {
                    Log.Error("存档数据校验失败，可能已损坏");
                    saveData.Dispose();
                    return null;
                }

                if (!saveData.Validate())
                {
                    Log.Error("存档数据验证失败");
                    saveData.Dispose();
                    return null;
                }
                
                // 设置父级关系
                saveData.SetParent(self);
                self.CurrentSaveData = saveData;
                
                Log.Info($"存档加载成功: {filePath}, Player: {header.PlayerId}, Version: {header.SaveVersion}");
                return saveData;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }

        /// <summary>
        /// 保存调试格式（开发模式下）
        /// </summary>
        private static async ETTask SaveDebugFormat(this SaveManagerComponent self, string originalPath , GameSaveData saveData)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            try
            {
                var currentFormat = GetCurrentFormat();
                var debugFormat = currentFormat == SerializationFormat.MemoryPack 
                        ? SerializationFormat.MongoDB 
                        : SerializationFormat.MemoryPack;

                byte[] debugData;
                switch (debugFormat)
                {
                    case SerializationFormat.MemoryPack:
                        EntitySystemSingleton.Instance.Serialize(saveData);
                        debugData = MemoryPackHelper.Serialize(saveData);
                        break;
                    case SerializationFormat.MongoDB:
                        EntitySystemSingleton.Instance.Serialize(saveData);
                        debugData = MongoHelper.Serialize(saveData);
                        break;
                    default:
                        return;
                }

                string debugExtension = debugFormat == SerializationFormat.MemoryPack ? ".sav" : ".json";
                string debugPath = Path.ChangeExtension(originalPath, debugExtension);
                await File.WriteAllBytesAsync(debugPath, debugData);
                
                Log.Info($"调试格式存档已保存: {debugPath}");
            }
            catch (Exception e)
            {
                Log.Warning($"保存调试格式存档失败: {e}");
            }
#endif
            await ETTask.CompletedTask;
        }
    }
}