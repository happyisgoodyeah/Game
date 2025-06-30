using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(Slot))]
    public class DragPuzzleEnd_CheckSlot : AEvent<Scene, DragPuzzleEndEvent>
    {
        protected override async ETTask Run(Scene scene, DragPuzzleEndEvent data)
        {
            Puzzle puzzle = data.puzzle;
            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            Grid grid = puzzle.GetParent<Grid>();
            GridView gridView = grid.GetComponent<GridView>();

            var index = puzzle.GetComponent<PuzzleView>().CheckAllSlot();
            if (index.isPass) //合法
            {   
                for (int i = 0; i < index.slots.Count; i++)
                {
                    index.slots[i].puzzleRef = puzzle;
                }
                //位置绑定
                puzzleView.transform.position = index.slots[0].GetComponent<SlotView>().transform.position;
            }
            else //不合法
            {
                puzzleView.BackToOriginPosition();
                
            }

            await ETTask.CompletedTask;
        }
    }
}