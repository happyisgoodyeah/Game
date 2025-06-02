using UnityEngine;

namespace ET
{
    public class EntityLink : MonoBehaviour
    {
        public Entity Entity { get; set; }
    
        // 在ET实体创建时调用此方法建立关联
        public static void Link(GameObject gameObject, Entity entity)
        {
            var link = gameObject.GetComponent<EntityLink>();
            if (link == null) 
            {
                link = gameObject.AddComponent<EntityLink>();
            }
            link.Entity = entity;
        }
    }
}
