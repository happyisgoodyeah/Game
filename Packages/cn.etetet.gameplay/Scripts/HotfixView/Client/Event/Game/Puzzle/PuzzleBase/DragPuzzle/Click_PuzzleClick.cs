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
            if (data.Entity is PuzzleView puzzleView)
            {
                puzzleView.transform.Rotate(Vector3.forward, data.Angle, Space.Self);
                
                EventSystem.Instance.Publish(scene, new ClickPuzzleRotateEvent() { puzzle = puzzleView.GetParent<Puzzle>(), angle = data.Angle });
            }

            await ETTask.CompletedTask;
        }
    }
}