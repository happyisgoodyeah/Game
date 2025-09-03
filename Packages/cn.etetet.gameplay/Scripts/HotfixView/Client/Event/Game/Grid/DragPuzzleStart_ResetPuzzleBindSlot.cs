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
            puzzle.ResetBindSlots();
            await ETTask.CompletedTask;
        }
    }
}