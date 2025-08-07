using System;
using System.Collections.Generic;

namespace ET
{
    [EntitySystemOf(typeof(TriggerColliderComponent))]
    public static partial class TriggerColliderComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.TriggerColliderComponent self, ET.Entity bindEntity, UnityEngine.GameObject bindEntityObj)
        {
            self.onTriggerEventMono = bindEntityObj.GetComponent<OnTriggerEventMono>();
            if (self.onTriggerEventMono == null)
            {
                self.onTriggerEventMono = bindEntityObj.AddComponent<OnTriggerEventMono>();
            }
            self.onTriggerEventMono.SetEntity(bindEntity);
        }

        /// <summary>
        /// 设置标签筛选列表
        /// </summary>
        public static void SetTagList(this ET.TriggerColliderComponent self , List<string> tagList)
        {
            self.onTriggerEventMono.SetTag(tagList);
        }
        
        /// <summary>
        /// 设置碰撞进入回调
        /// </summary>
        /// <param name="action"></param>
        public static void SetTriggerEnterAction(this ET.TriggerColliderComponent self , Action action)
        {
            self.onTriggerEventMono.SetTriggerEnterAction(action);
        }
        
        /// <summary>
        /// 设置碰撞持续回调
        /// </summary>
        /// <param name="action"></param>
        public static void SetTriggerStayAction(this ET.TriggerColliderComponent self , Action action)
        {
            self.onTriggerEventMono.SetTriggerStayAction(action);
        }
        
        /// <summary>
        /// 设置碰撞退出回调
        /// </summary>
        /// <param name="action"></param>
        public static void SetTriggerExitAction(this ET.TriggerColliderComponent self , Action action)
        {
            self.onTriggerEventMono.SetTriggerExitAction(action);
        }
    }
}