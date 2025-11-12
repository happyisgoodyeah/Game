using System;

namespace ET
{
    /// <summary>
    /// 存档数据组件接口 - 标记需要序列化的组件
    /// 所有需要序列化的存档数据组件都应实现此接口
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
    
    /// <summary>
    /// 存档验证接口
    /// 需要验证的数据组件实现此接口
    /// </summary>
    public interface ISaveDataValidator
    {
        /// <summary>
        /// 验证数据完整性
        /// </summary>
        /// <returns></returns>
        bool Validate();
    }
}
