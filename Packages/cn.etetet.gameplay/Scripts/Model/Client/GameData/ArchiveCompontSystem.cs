using System.Collections.Generic;
using System.IO;
using MemoryPack;

namespace ET
{
    [EntitySystemOf(typeof(ArchiveCompont))]
    [FriendOf(typeof(ArchiveCompont))]
    [FriendOf(typeof(ArchiveData))]
    public static partial class ArchiveCompontSystem
    {
        [EntitySystem]
        private static void Awake(this ET.ArchiveCompont self)
        {
            //self.archiveList = archiveList;
            //self.Init();
            //self.savePath = savePath;
        }

        public static void Init()
        {
        }

        // 保存图鉴数据
        public static void SaveArchive(this ET.ArchiveCompont self)
        {
            try
            {
                // 序列化整个列表
                byte[] bytes = MemoryPackSerializer.Serialize(self.archiveList);
                // 将二进制数据写入文件
                File.WriteAllBytes(self.savePath, bytes);
                Log.Console($"图鉴存档成功！路径：{self.savePath}");
            }
            catch (System.Exception e)
            {
                Log.Error($"图鉴存档失败: {e.Message}");
            }
        }

        // 加载图鉴数据
        public static void LoadArchive(this ET.ArchiveCompont self)
        {
            try
            {
                if (!File.Exists(self.savePath))
                {
                    Log.Console("未找到图鉴存档文件，将创建新存档。");
                    self.archiveList = new List<ArchiveData>(); // 初始化一个新列表
                    return;
                }

                // 从文件读取二进制数据
                byte[] bytes = File.ReadAllBytes(self.savePath);
                // 反序列化回List<ArchiveData>
                var loadedList = MemoryPackSerializer.Deserialize<List<ArchiveData>>(bytes);

                if (loadedList != null)
                {
                    self.archiveList = loadedList;
                    Log.Console("图鉴存档加载成功！");
                }
                else
                {
                    self.archiveList = new List<ArchiveData>();
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"图鉴存档加载失败: {e.Message}");
            }
        }

        // --- 提供给外部的接口，用于操作图鉴数据 ---

        // 解锁一个新图鉴
        /*public void UnlockArchiveItem(int itemId)
        {
            // 检查是否已存在，避免重复添加
            if (!archiveList.Exists(data => data.ItemId == itemId))
            {
                ArchiveData newItem = new ArchiveData
                {
                    ItemId = itemId,
                    IsUnlocked = true,
                    DiscoverTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                archiveList.Add(newItem);
                SaveArchive(); // 解锁后立即保存
                Debug.Log($"已解锁图鉴物品：{itemId}");
            }
        }*/

        // 检查某个物品是否已解锁
        public static bool IsItemUnlocked(this ArchiveCompont self, int itemId)
        {
            //return self.archiveList.Exists(data => data.ItemId == itemId && data.IsUnlocked);
            return false;
        }

        // 获取所有图鉴数据（用于UI显示等）
        public static List<ArchiveData> GetAllArchiveData(this ET.ArchiveCompont self)
        {
            return self.archiveList;
        }
    }
}