using UnityEngine;

namespace ET
{
    [EntitySystemOf(typeof(SpriteRenderComponent))]
    public static partial class SpriteRenderComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.SpriteRenderComponent self, UnityEngine.GameObject gameObject)
        {
            self.SpriteRenderer = gameObject.transform.GetComponent<SpriteRenderer>();
            if (self.SpriteRenderer != null)
            {
                self.SpriteSize = self.SpriteRenderer.bounds.size;
            }
        }
    }    
}