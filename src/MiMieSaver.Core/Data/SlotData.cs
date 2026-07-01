using System;

namespace MiMieSaver
{
    /// <summary>
    /// 单个存档槽数据
    /// </summary>
    [Serializable]
    public class SlotData : ISlot
    {
        /// <summary>
        /// 存档槽唯一 ID
        /// </summary>
        public string SlotId { get; set; }

        /// <summary>
        /// 存档槽显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 创建时间戳
        /// </summary>
        public long CreateTime { get; set; }

        /// <summary>
        /// 最后保存时间戳
        /// </summary>
        public long LastSaveTime { get; set; }

        /// <summary>
        /// 默认构造
        /// </summary>
        public SlotData()
        {
            SlotId = Guid.NewGuid().ToString();
            DisplayName = "New Slot";
            CreateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            LastSaveTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// 指定显示名构造
        /// </summary>
        public SlotData(string displayName) : this()
        {
            DisplayName = displayName;
        }
    }
}
