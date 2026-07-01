using System;
using System.Collections.Generic;

namespace MiMieSaver
{
    /// <summary>
    /// 存档槽索引数据
    /// </summary>
    [Serializable]
    public class SlotsIndexData
    {
        /// <summary>
        /// 当前槽位 ID
        /// </summary>
        public string currentSlotId;

        /// <summary>
        /// 所有槽位
        /// </summary>
        public List<SlotData> slotList { get; set; } = new List<SlotData>();

        /// <summary>
        /// 默认构造
        /// </summary>
        public SlotsIndexData()
        {
            slotList = new List<SlotData>();
        }
    }
}
