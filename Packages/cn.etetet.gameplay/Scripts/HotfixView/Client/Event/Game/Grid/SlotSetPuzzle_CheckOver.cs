using DG.Tweening;
using ET.Client;
using UnityEngine;
using YIUIFramework;

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
                //跳转关卡选择界面，清空当前Grid
                await YIUIMgrComponent.Inst.Root.OpenPanelAsync<GameMainPanelPanelComponent, EGameMainPanelPanelViewEnum>(EGameMainPanelPanelViewEnum.SelectLevelView);
                grid.Dispose();
            }

            await ETTask.CompletedTask;
        }
    }
}