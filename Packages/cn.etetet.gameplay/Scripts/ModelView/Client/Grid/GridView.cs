using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Grid))]
    public class GridView : Entity , IAwake<Transform>
    {
        public Transform transform { get; set; }
        
        public Transform parentTransform { get; set; }

        public Transform slotTransform { get; set; }

        public Transform puzzleTransform { get; set; }
    }
}
