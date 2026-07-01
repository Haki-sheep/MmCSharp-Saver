using Game.Save;

namespace MiMieSaver
{
    /// <summary>
    /// 存档模块接口 外部开发者需要实现此接口来处理存档数据
    /// 注意SaveData是Protobuf生成的类，需要使用Protobuf的API来读写数据
    /// </summary>
    public interface IArchiveModule
    {
        #region 属性

        /// <summary>
        /// 模块名称
        /// </summary>
        string ModuleName { get; }

        #endregion

        #region 存档读写

        /// <summary>
        /// 创建新存档时的初始化
        /// </summary>
        void CreateArchive(SaveData saveData);

        /// <summary>
        /// 从存档读取数据
        /// </summary>
        void FromArchive(SaveData saveData);

        /// <summary>
        /// 写入存档数据
        /// </summary>
        void ToArchive(SaveData saveData);

        #endregion
    }
}
