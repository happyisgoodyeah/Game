using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(Slot))]
    [FriendOf(typeof(Puzzle))]
    public class DragPuzzleStart_ResetPuzzleBindSlot : AEvent<Scene, DragPuzzleStartEvent>
    {
        protected override async ETTask Run(Scene scene, DragPuzzleStartEvent data)
        {
            Puzzle puzzle = data.puzzle;
            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();

            if (puzzle.moveMode == PuzzleMoveModeType.Normal)
            {
                puzzleView.transform.position = data.worldPosition;
            }

            //拖拽开始的图层层级提高一级，暂时这么写
            //puzzleView.transform.position = new Vector3(puzzleView.transform.position.x, puzzleView.transform.position.y, 10);
            puzzleView.ChangePuzzleLayOut(10);

            puzzle.ResetBindSlots();
            await ETTask.CompletedTask;
        }
    }
}