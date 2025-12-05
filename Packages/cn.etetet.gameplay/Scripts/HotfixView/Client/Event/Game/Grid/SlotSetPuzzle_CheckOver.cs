using DG.Tweening;
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

            if (grid.CheckGameOver())
            {
                Log.Error("当前关卡完成");
                //写序列化文件并保存到本地
                var savemanager = grid.Root().GetComponent<SaveManagerComponent>();
                savemanager.GetPlayerDataComponent().PassLevel(grid.Config().Id);
                await savemanager.SaveAsync();
            }

            await ETTask.CompletedTask;
        }
    }
}