using UnityEngine;

namespace ET
{
    [ComponentOf()]
    public class TriggerColliderComponent : Entity , IAwake<Entity , GameObject>
    {
        /// <summary>
        /// mono脚本
        /// </summary>
        public OnTriggerEventMono onTriggerEventMono;
    }    
}