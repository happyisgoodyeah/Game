using System;
using System.Collections.Generic;

namespace ET
{
    [AttributeUsage(AttributeTargets.Class , AllowMultiple = false)]
    public class SaveValidationAttribute : BaseAttribute
    {
        /// <summary>
        /// 组件类型
        /// </summary>
        public Type ComponentType { get; set; }
        
        public SaveValidationAttribute(Type componentType)
        {
            this.ComponentType = componentType;
        }
    }
    
    
    /// <summary>
    /// 存档验证Handle分发组件
    /// 负责分发验证请求到各个验证处理器
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class SaveValidationDispatcherComponent : Entity , IAwake
    {
        [StaticField]
        public Dictionary<Type, ISaveValidationHandler> SaveValidationHandlers;
    }    
}