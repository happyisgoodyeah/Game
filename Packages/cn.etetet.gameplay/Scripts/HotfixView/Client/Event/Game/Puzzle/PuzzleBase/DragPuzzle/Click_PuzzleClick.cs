using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(Puzzle))]
    public class Click_PuzzleClick : AEvent<Scene, ClickRotateEvent>
    {
        protected override async ETTask Run(Scene scene, ClickRotateEvent data)
        {
            if (data.Entity is PuzzleView puzzleView)
            {
                Puzzle puzzle = puzzleView.GetParent<Puzzle>();
                
                //没有绑定的slot才允许旋转
                if (puzzle.bindSlots.Count == 0)
                {
                    puzzle.RotatePuzzle();
                
                    EventSystem.Instance.Publish(scene, new ClickPuzzleRotateEvent() { puzzle = puzzleView.GetParent<Puzzle>(), angle = data.Angle });    
                }
            }

            await ETTask.CompletedTask;
        }
    }
}