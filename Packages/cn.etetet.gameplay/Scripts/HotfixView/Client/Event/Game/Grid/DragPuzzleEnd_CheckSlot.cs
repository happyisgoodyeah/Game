using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    public class DragPuzzleEnd_CheckSlot : AEvent<Scene, DragPuzzleEndEvent>
    {
        protected override async ETTask Run(Scene scene, DragPuzzleEndEvent data)
        {
            Puzzle puzzle = data.puzzle;
            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            Grid grid = puzzle.GetParent<Grid>();
            GridView gridView = grid.GetComponent<GridView>();

            var index = gridView.CheckPuzzleInSlot(puzzleView);
            if (index.Item1) //合法
            {
                index.Item2.SetPuzzle(puzzle);
                
                //位置绑定
                puzzleView.transform.position = index.Item2.transform.position;
                
            }
            else //不合法
            {
                puzzleView.BackToOriginPosition();
                
            }

            await ETTask.CompletedTask;
        }
    }
}