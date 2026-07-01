using System;
using System.Collections.Generic;
using System.IO;
using Game.Save;
using Google.Protobuf;

namespace MiMieSaver
{
    /// <summary>
    /// 存档管理器 槽位 读写 模块汇总
    /// </summary>
    public class ArchiveMgr : IArchiveMgr
    {

        /// <summary>
        /// 存档根路径
        /// </summary>
        private readonly string rootPath;

        /// <summary>
        /// 存档槽管理器
        /// </summary>
        private readonly SlotsIndexMgr slotIndex;

        /// <summary>
        /// 模块有序列表
        /// </summary>
        private readonly List<IArchiveModule> moduleOrderList = new List<IArchiveModule>();

        /// <summary>
        /// 模块类型字典
        /// </summary>
        private readonly Dictionary<Type, IArchiveModule> modulesByTypeDict = new Dictionary<Type, IArchiveModule>();



        /// <summary>
        /// 存档根路径
        /// </summary>
        public string RootPath => rootPath;

        /// <summary>
        /// 模块数量
        /// </summary>
        public int ModuleCount => moduleOrderList.Count;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rootPath">存档根目录</param>
        public ArchiveMgr(string rootPath)
        {
            this.rootPath = rootPath;
            Directory.CreateDirectory(rootPath);
            slotIndex = new SlotsIndexMgr(rootPath);
        }

        #region 存档槽操作

        /// <summary>
        /// 获取所有槽位索引
        /// </summary>
        public ISlotIndex GetAllSlotIndex() => slotIndex;

        /// <summary>
        /// 创建存档槽
        /// </summary>
        public ISlot CreatSlot(string displayerName) => slotIndex.CreatSlot(displayerName);

        /// <summary>
        /// 切换存档槽
        /// </summary>
        public void SwitchSlot(string slotId) => slotIndex.SwitchSlot(slotId);

        /// <summary>
        /// 删除存档槽
        /// </summary>
        public void DeleteSlot(string slotId) => slotIndex.DeleteSlot(slotId);

        /// <summary>
        /// 清理孤立文件
        /// </summary>
        public void CleanupOrphanedFiles() => slotIndex.CleanupOrphanedFiles();

        /// <summary>
        /// 清理孤立槽位
        /// </summary>
        public void CleanupOrphanedSlots() => slotIndex.CleanupOrphanedSlots();

        /// <summary>
        /// 重命名存档槽
        /// </summary>
        public void RenameSlot(string slotId, string newName) => slotIndex.RenameSlot(slotId, newName);

        #endregion

        #region 对外接口

        /// <summary>
        /// 获取当前存档数据
        /// </summary>
        public SaveData GetArchive()
        {
            var slot = slotIndex.CurrentSlot;
            if (slot == null) return null;

            string path = slotIndex.GetSlotPath(slot.SlotId);
            if (!File.Exists(path)) return null;

            // 通过字节流的方式读取存档数据
            using var stream = File.OpenRead(path);
            return SaveData.Parser.ParseFrom(stream);
        }

        /// <summary>
        /// 保存存档
        /// </summary>
        public void Save()
        {
            // 获取当前激活槽位
            var slot = slotIndex.CurrentSlot;
            if (slot is null) return;

            SaveData saveData;
            try
            {
                // 获取当前活跃的存档数据
                saveData = GetArchive();
            }
            catch
            {
                // 如果获取当前数据失败，则创建新的存档数据
                saveData = new SaveData();
            }

            // 设置存档元信息 这部分是protobuf所需
            saveData.Meta ??= new MetaSave();
            saveData.Meta.LastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // 遍历所有模块 写入存档数据 这里是存档系统的核心
            // 开发者实现接口 注册并且存入moduleOrderList后 这里会自动调用开发者实现的ToArchive方法
            foreach (var module in moduleOrderList)
                module.ToArchive(saveData);

            // 将存档数据写入文件
            string path = slotIndex.GetSlotPath(slot.SlotId);
            File.WriteAllBytes(path, saveData.ToByteArray());

            // 更新存档槽最后保存时间
            slotIndex.UpdateLastSaveTime(slot.SlotId);
        }

        /// <summary>
        /// 加载存档
        /// </summary>
        public void Load()
        {
            var slot = slotIndex.CurrentSlot;
            if (slot is null) return;

            string path = slotIndex.GetSlotPath(slot.SlotId);
            if (!File.Exists(path)) return;

            using var stream = File.OpenRead(path);
            var saveData = SaveData.Parser.ParseFrom(stream);

            foreach (var module in moduleOrderList)
                module.FromArchive(saveData);
        }

        /// <summary>
        /// 判断是否存在存档
        /// </summary>
        public bool HasSaveData()
        {
            var slot = slotIndex.CurrentSlot;
            if (slot == null) return false;
            return File.Exists(slotIndex.GetSlotPath(slot.SlotId));
        }

        #region 模块注册

        /// <summary>
        /// 注册存档模块
        /// </summary>
        public void RegisterModule<T>(T module) where T : class, IArchiveModule
        {
            if (module == null) throw new ArgumentNullException(nameof(module));

            Type key = typeof(T);
            if (modulesByTypeDict.TryGetValue(key, out var existing))
            {
                if (ReferenceEquals(existing, module))
                    return;

                moduleOrderList.Remove(existing);
            }

            modulesByTypeDict[key] = module;
            if (!moduleOrderList.Contains(module))
                moduleOrderList.Add(module);
        }

        /// <summary>
        /// 注销指定类型模块
        /// </summary>
        public bool UnregisterModule<T>() where T : class, IArchiveModule
        {
            Type key = typeof(T);
            if (!modulesByTypeDict.TryGetValue(key, out var module))
                return false;

            modulesByTypeDict.Remove(key);
            moduleOrderList.Remove(module);
            return true;
        }

        #endregion

        #region 模块查询

        /// <summary>
        /// 获取指定模块
        /// </summary>
        public T GetModule<T>() where T : class, IArchiveModule
        {
            return TryGetModule(out T module) ? module : null;
        }

        /// <summary>
        /// 尝试获取指定模块
        /// </summary>
        public bool TryGetModule<T>(out T module) where T : class, IArchiveModule
        {
            if (modulesByTypeDict.TryGetValue(typeof(T), out var found) && found is T castModule)
            {
                module = castModule;
                return true;
            }

            module = null;
            return false;
        }

        /// <summary>
        /// 判断是否存在模块
        /// </summary>
        public bool HasModule<T>() where T : class, IArchiveModule
        {
            return TryGetModule<T>(out _);
        }

        /// <summary>
        /// 获取所有模块
        /// </summary>
        public IReadOnlyList<IArchiveModule> GetModules() => moduleOrderList;

        #endregion

        #endregion
    }
}
