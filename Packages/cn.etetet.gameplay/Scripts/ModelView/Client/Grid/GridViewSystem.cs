using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(GridView))]
    public static partial class GridViewSystem
    {
        [EntitySystem]
        private static void Awake(this ET.GridView self , Transform transform)
        {
            self.transform = transform;
            self.parentTransform = transform.parent;
        }
    }
}