using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(GridView))]
    [FriendOf(typeof(SlotView))]
    public class PuzzleColliderExitTrigger_PuzzleChangeMoveMode : AEvent<Scene , ColliderTriggerExitEventMono>
    {
        protected override async ETTask Run(Scene scene, ColliderTriggerExitEventMono data)
        {
            var bindEntity = data.bindEntity;
            var triggerEntity = data.triggerEntity;
            if (bindEntity is PuzzleView puzzleView && triggerEntity is GridView gridView)
            {
                // Puzzle puzzle = puzzleView.GetParent<Puzzle>();
                // Grid grid = gridView.GetParent<Grid>();
                //
                // //改为普通移动模式
                // if (puzzle.moveMode == PuzzleMoveModeType.Adsorption)
                // {
                //     puzzle.ChangeMoveMode(PuzzleMoveModeType.Normal);
                // }
            }
            await ETTask.CompletedTask;
        }
    }    
}