using DG.Tweening;
using ET.Client;
using UnityEngine;
using YIUIFramework;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(LevelSessionComponent))]
    public class SlotSetPuzzle_CheckOver : AEvent<Scene, SlotSetPuzzle>
    {
        protected override async ETTask Run(Scene scene, SlotSetPuzzle data)
        {
            Slot slot = data.slot;
            if (slot == null || slot.IsDisposed)
            {
                return;
            }
            Grid grid = slot.GetParent<Grid>();
            if (grid == null)
            {
                return;
            }
            LevelSessionComponent session = LevelSessionComponentSystem.GetOrCreate(scene);
            if (session.CurrentGrid.Entity != grid || session.IsLoading || session.IsCompleting || session.IsReturning)
            {
                return;
            }

            if (grid.CheckGameOver())
            {
                session.IsCompleting = true;
                EntityRef<LevelSessionComponent> sessionRef = session;
                EntityRef<Grid> gridRef = grid;
                long version = session.Version;
                Log.Info("当前关卡完成");
                try
                {
                    // 等待最后一次放置的六帧云尘和枕头回弹完成，再执行通关收尾。
                    await scene.Root().GetComponent<TimerComponent>().WaitAsync(450);
                    session = sessionRef;
                    grid = gridRef;
                    if (session == null || grid == null || session.Version != version)
                    {
                        return;
                    }
                    //写序列化文件并保存到本地
                    var saveManager = grid.Root().GetComponent<SaveManagerComponent>();
                    var playerData = saveManager.GetPlayerDataComponent();
                    if (!playerData.CheckLevelPass(grid.Config().Id))
                    {
                        playerData.PassLevel(grid.Config().Id);
                    }
                    SaveResult result = await saveManager.SaveAsync();
                    if (result != SaveResult.Success)
                    {
                        Log.Error($"关卡进度保存失败: {result}");
                    }
                    session = sessionRef;
                    if (session == null || session.Version != version)
                    {
                        return;
                    }
                    //跳转关卡选择界面，清空当前Grid
                    await session.ExitLevelAsync();
                }
                finally
                {
                    session = sessionRef;
                    if (session != null && session.Version == version)
                    {
                        session.IsCompleting = false;
                    }
                }
            }

            await ETTask.CompletedTask;
        }
    }
}
