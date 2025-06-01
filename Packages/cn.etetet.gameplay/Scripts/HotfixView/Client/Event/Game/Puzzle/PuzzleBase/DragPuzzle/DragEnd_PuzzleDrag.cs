using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(GridView))]
    public class DragEnd_PuzzleDrag : AEvent<Scene, DragEndEvent>
    {
        protected override async ETTask Run(Scene scene, DragEndEvent data)
        {
            //是Puzzle拖拽事件
            if (data.Entity is PuzzleView puzzleView)
            {
                puzzleView.transform.position = data.EndPosition;

                //进行判定是否合法
                EventSystem.Instance.Publish(scene, new DragPuzzleEndEvent() { puzzle = puzzleView.GetParent<Puzzle>() });
            }

            await ETTask.CompletedTask;
        }
    }
}