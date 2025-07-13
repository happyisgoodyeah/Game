using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(GridView))]
    public class DragStart_PuzzleDrag : AEvent<Scene , DragStartEvent>
    {
        protected override async ETTask Run(Scene scene, DragStartEvent data)
        {
            //是Puzzle拖拽事件
            if (data.Entity is PuzzleView puzzleView)
            {
                puzzleView.transform.position = data.StartPosition;
                
                //进行判定是否合法
                EventSystem.Instance.Publish(scene, new DragPuzzleStartEvent() { puzzle = puzzleView.GetParent<Puzzle>() });
            }

            await ETTask.CompletedTask;
        }
    }
}
