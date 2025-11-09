using System;

namespace ET
{
    /// <summary>
    /// 存档数据组件接口 - 标记需要序列化的组件
    /// </summary>
    public interface ISaveDataComponent
    {
        /// <summary>
        /// 获取组件类型标识
        /// </summary>

        string ComponentType { get; }
        
        /// <summary>
        /// 最后修改时间
        /// </summary>
        DateTime LastModified { get; set; }
        
        /// <summary>
        /// 数据版本
        /// </summary>
        string DataVersion { get; }
    }
}
