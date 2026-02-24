using System;
using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(SaveValidationDispatcherComponent))]
    [FriendOf(typeof(SaveValidationDispatcherComponent))]
    public static partial class SaveValidationDispatcherComponentSystem
    {
        [EntitySystem]
        private static void Awake(this SaveValidationDispatcherComponent self)
        {
            self.SaveValidationHandlers = new Dictionary<Type, ISaveValidationHandler>();
            self.LoadValidationHandlers();
        }

        /// <summary>
        /// 加载所有验证处理器
        /// </summary>
        public static void LoadValidationHandlers(this SaveValidationDispatcherComponent self)
        {
            var validationTypes = CodeTypes.Instance.GetTypes(typeof(SaveValidationAttribute));

            foreach (Type type in validationTypes)
            {
                //返回应用于类型的指定类型的自定义属性数组
                var attributes = type.GetCustomAttributes(typeof(SaveValidationAttribute), false);
                if(attributes.Length == 0) continue;

                SaveValidationAttribute attribute = (SaveValidationAttribute)attributes[0];
                
                //动态创建接口实例
                ISaveValidationHandler handler = Activator.CreateInstance(type) as ISaveValidationHandler;
                
                if (handler != null)
                {
                    self.SaveValidationHandlers[attribute.ComponentType] = handler;
                    Log.Info($"注册存档验证处理器: {type.Name} -> {attribute.ComponentType.Name}");
                }
            }
        }

        /// <summary>
        /// 验证实体数据
        /// </summary>
        /// <param name="self"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool ValidateEntity(this SaveValidationDispatcherComponent self, Entity entity)
        {
            if(entity == null)return false;
            
            Type entityType = entity.GetType();

            if (self.SaveValidationHandlers.TryGetValue(entityType, out var handle))
            {
                try
                {
                    return handle.Handle(entity);
                }
                catch (Exception e)
                {
                    Log.Error($"验证实体时发生异常: {entityType.Name}, {e}");
                    return false;
                }
            }
            
            // 如果没有找到特定的验证处理器，检查是否实现了ISaveDataValidator接口
            if (entity is ISaveDataValidator validator)
            {
                try
                {
                    return validator.Validate();
                }
                catch (Exception e)
                {
                    Log.Error($"验证ISaveDataValidator时发生异常: {entityType.Name}, {e}");
                    return false;
                }
            }

            // 如果既没有验证处理器也没有实现验证接口，默认返回true
            return true;
        }

        public static bool ValidateSaveData(this SaveValidationDispatcherComponent self, GameSaveData gameSaveData)
        {
            if (gameSaveData == null)
            {
                Log.Error("存档数据为空");
                return false;
            }
            
            // 验证所有组件
            foreach (var component in gameSaveData.Components.Values)
            {
                if (!self.ValidateEntity(component))
                {
                    Log.Error($"组件验证失败: {component.GetType().Name}");
                    return false;
                }
            }
            
            Log.Info("存档数据验证通过");
            return true;
        }
    }
}
