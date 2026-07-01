using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace MiMieSaver
{
    /// <summary>
    /// 存档槽目录管理器
    /// </summary>
    public class SlotsIndexMgr : ISlotIndex
    {
        /// <summary>
        /// 槽位索引文件路径
        /// </summary>
        private readonly string path;

        /// <summary>
        /// 槽位索引数据
        /// </summary>
        private SlotsIndexData data;
    

        /// <summary>
        /// 当前槽位 ID
        /// </summary>
        public string CurrentSlotId => data.currentSlotId;

        /// <summary>
        /// 当前槽位
        /// </summary>
        public ISlot CurrentSlot => data.slotList.FirstOrDefault(s => s.SlotId == data.currentSlotId);

        /// <summary>
        /// 所有槽位
        /// </summary>
        public IReadOnlyList<ISlot> Slots => data.slotList;

        /// <summary>
        /// 槽位数量
        /// </summary>
        public int Count => data.slotList.Count;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rootPath">存档根目录</param>
        public SlotsIndexMgr(string rootPath)
        {
            path = Path.Combine(rootPath, "slotsIndex.json");
            Directory.CreateDirectory(rootPath);
            Load();
        }

        #region 槽位 CRUD

        /// <summary>
        /// 创建一个新的存档槽
        /// </summary>
        public ISlot CreatSlot(string displayerName)
        {
            if (data.slotList == null)
                data.slotList = new List<SlotData>();

            var slot = new SlotData(displayerName);
            data.slotList.Add(slot);
            data.currentSlotId = slot.SlotId;
            Save();
            return slot;
        }

        /// <summary>
        /// 切换当前使用的存档槽
        /// </summary>
        public void SwitchSlot(string slotId)
        {
            if (data.slotList == null) return;
            data.currentSlotId = slotId;
            Save();
        }

        /// <summary>
        /// 删除指定的存档槽及其 dat 文件
        /// </summary>
        public void DeleteSlot(string slotId)
        {
            if (data.slotList == null) return;
            var slot = data.slotList.Find(s => s.SlotId == slotId);
            if (slot is null) return;

            data.slotList.Remove(slot);

            if (data.currentSlotId == slotId)
            {
                data.currentSlotId = data.slotList.Count > 0
                    ? data.slotList[0].SlotId
                    : null;
            }

            Save();

            string filePath = GetSlotPath(slotId);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        /// <summary>
        /// 重命名指定的存档槽
        /// </summary>
        public void RenameSlot(string slotId, string newName)
        {
            if (data.slotList == null) return;
            var slot = data.slotList.Find(s => s.SlotId == slotId);
            if (slot != null)
            {
                slot.DisplayName = newName;
                Save();
            }
        }

        /// <summary>
        /// 更新指定的存档槽最后保存时间
        /// </summary>
        public void UpdateLastSaveTime(string slotId)
        {
            if (data.slotList == null) return;
            var slot = data.slotList.Find(s => s.SlotId == slotId);
            if (slot != null)
            {
                slot.LastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Save();
            }
        }

        /// <summary>
        /// 获取指定的存档槽 dat 路径
        /// </summary>
        public string GetSlotPath(string slotId)
        {
            string dir = Path.GetDirectoryName(path);
            return Path.Combine(dir, $"{slotId}_SaveData.dat");
        }

        #endregion

        #region 孤立文件清理

        /// <summary>
        /// 清理孤立存档槽 有索引无 dat
        /// </summary>
        public void CleanupOrphanedSlots()
        {
            if (data.slotList == null) return;
            var orphaned = data.slotList.Where(s => !File.Exists(GetSlotPath(s.SlotId))).ToList();
            if (orphaned.Count == 0) return;

            for (int i = orphaned.Count - 1; i >= 0; i--)
            {
                var slot = orphaned[i];
                data.slotList.Remove(slot);

                if (data.currentSlotId == slot.SlotId)
                {
                    data.currentSlotId = data.slotList.Count > 0
                        ? data.slotList[0].SlotId
                        : null;
                }
            }

            Save();
        }

        /// <summary>
        /// 清理孤立存档文件 有 dat 无索引
        /// </summary>
        public void CleanupOrphanedFiles()
        {
            if (data.slotList == null) return;
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) return;

            string[] datFiles = Directory.GetFiles(dir, "*_SaveData.dat");
            var validNames = data.slotList.Select(s => $"{s.SlotId}_SaveData.dat").ToHashSet();

            foreach (string file in datFiles)
            {
                if (!validNames.Contains(Path.GetFileName(file)))
                    File.Delete(file);
            }
        }

        #endregion

        #region 读写工具函数

        /// <summary>
        /// 从磁盘加载索引
        /// </summary>
        private void Load()
        {
            // 如果文件存在，则读取文件内容
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                var loaded = JsonConvert.DeserializeObject<SlotsIndexData>(json);
                data = loaded ?? new SlotsIndexData();
            }
            // 如果文件不存在，则创建新的槽位索引数据
            else
            {
                data = new SlotsIndexData();
            }

            // 如果槽位列表为空，则创建新的槽位列表
            if (data.slotList == null)
                data.slotList = new List<SlotData>();

            // 清理孤立存档槽
            CleanupOrphanedSlots();
        }

        /// <summary>
        /// 写入索引到磁盘
        /// </summary>
        private void Save()
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        #endregion
    }
}
