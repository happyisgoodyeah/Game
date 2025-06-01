using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(GridView))]
    public class DragUpdate_PuzzleDrag : AEvent<Scene, DragUpdateEvent>
    {
        protected override async ETTask Run(Scene scene, DragUpdateEvent data)
        {
            //是Puzzle拖拽事件
            if (data.Entity is PuzzleView puzzleView)
            {
                puzzleView.transform.position = data.CurrentPosition;
            }

            await ETTask.CompletedTask;
        }
    }
}