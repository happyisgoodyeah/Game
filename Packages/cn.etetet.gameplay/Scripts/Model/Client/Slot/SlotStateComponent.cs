namespace ET
{
    [ComponentOf(typeof(Slot))]
    public class SlotStateComponent : Entity , IAwake<bool>
    {
        public bool AllowPlace;
    }    
}