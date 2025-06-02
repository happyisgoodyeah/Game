using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Slot))]
    public class SlotView : Entity , IAwake<Transform>
    {
        public Transform transform { get; set; }
        
        public Transform parentTransform { get; set; }
    }    
}