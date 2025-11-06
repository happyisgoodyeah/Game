using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(GridView))]
    [FriendOf(typeof(Slot))]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(PuzzleView))]
    public class Click_PuzzleClick : AEvent<Scene, ClickRotateEvent>
    {
        protected override async ETTask Run(Scene scene, ClickRotateEvent data)
        {
            if (data.RotType == 0)
            {
                //拖拽的时候进行旋转操作
                if (data.Entity is PuzzleView puzzleView)
                {
                    Puzzle puzzle = puzzleView.GetParent<Puzzle>();
                   // puzzleView.Rotate(data.Angle);

                    //todo 不知道是干什么的逻辑  （混乱）没有绑定的slot才允许旋转
                    if (puzzle.bindSlots.Count == 0)
                    {
                        puzzle.RotatePuzzle();

                        EventSystem.Instance.Publish(scene,
                            new ClickPuzzleRotateEvent() { puzzle = puzzleView.GetParent<Puzzle>(), angle = data.Angle });
                    }
                }
            }
            else
            {
                
            }

            await ETTask.CompletedTask;
        }
    }
}