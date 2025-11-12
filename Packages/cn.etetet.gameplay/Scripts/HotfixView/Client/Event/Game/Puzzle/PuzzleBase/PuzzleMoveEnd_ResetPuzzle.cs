using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(PuzzleView))]
    public class PuzzleMoveEnd_ResetPuzzle : AEvent<Scene , PuzzleMoveEndEvent>
    {
        protected override async ETTask Run(Scene scene, PuzzleMoveEndEvent a)
        {
            Puzzle puzzle = a.puzzle;
            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            var grid = puzzle.GetParent<Grid>();
            var gridView = grid.GetComponent<GridView>();
            
            //是否在Grid内
            puzzle.isInGrid = gridView.GetPuzzleInGrid(puzzle);
            
            //startPos endPos重置
            // puzzleView.startPos = Vector3.zero;
            // puzzleView.endPos = Vector3.zero;
            
            scene.GetComponent<ObjectWait>().Notify(new PuzzleMoveEndEvent());
            await ETTask.CompletedTask;
        }
    }    
}