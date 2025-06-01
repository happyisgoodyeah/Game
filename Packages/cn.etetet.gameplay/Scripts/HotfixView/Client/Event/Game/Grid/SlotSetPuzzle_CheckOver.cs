using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    public class SlotSetPuzzle_CheckOver : AEvent<Scene, SlotSetPuzzle>
    {
        protected override async ETTask Run(Scene scene, SlotSetPuzzle data)
        {
            Slot slot = data.slot;
            Grid grid = slot.GetParent<Grid>();
            
            grid.CheckGameOver();
            await ETTask.CompletedTask;
        }
    }
}