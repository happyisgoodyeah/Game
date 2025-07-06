using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(GridView))]
    public class Click_PuzzleClick : AEvent<Scene, ClickRotateEvent>
    {
        protected override async ETTask Run(Scene scene, ClickRotateEvent data)
        {
            //是Puzzle拖拽事件
            if (data.Entity is PuzzleView puzzleView)
            {
                puzzleView.transform.Rotate(Vector3.forward, data.Angle, Space.Self);

                //进行判定是否合法
                EventSystem.Instance.Publish(scene, new ClickPuzzleRotateEvent() { puzzle = puzzleView.GetParent<Puzzle>(), angle = data.Angle });
            }

            await ETTask.CompletedTask;
        }
    }
}