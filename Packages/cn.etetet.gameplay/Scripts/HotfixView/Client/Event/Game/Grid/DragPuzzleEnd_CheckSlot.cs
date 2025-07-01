using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(Slot))]
    [FriendOf(typeof(Puzzle))]
    public class DragPuzzleEnd_CheckSlot : AEvent<Scene, DragPuzzleEndEvent>
    {
        protected override async ETTask Run(Scene scene, DragPuzzleEndEvent data)
        {
            Puzzle puzzle = data.puzzle;
            PuzzleView puzzleView = puzzle.GetComponent<PuzzleView>();
            Grid grid = puzzle.GetParent<Grid>();
            GridView gridView = grid.GetComponent<GridView>();
            
            //拖拽结束的位置
            var worldPosition = data.worldPosition;
            FloatVector2 position = new FloatVector2(worldPosition.x, worldPosition.y);
            
            //拖拽结束的位置位于grid中
            if (grid.ContainsPosition(position))
            {
                var originPosition = grid.WorldToGridPosition(position);
                //能够放置拼图
                if (grid.CanPlacePuzzle(puzzle, originPosition))
                {
                    puzzle.ResetBindSlots();
                    
                    var positionList = grid.GetCoveredPositions(puzzle, originPosition);
                    foreach (var slotPosition in positionList)
                    {
                        var slot = grid.GetSlot(slotPosition);
                        slot.puzzleRef = puzzle;
                        puzzle.bindSlots.Add(slot);
                    }
                }
                puzzleView.transform.position = grid.GetSlot(originPosition).GetComponent<SlotView>().transform.position;
            }
            else
            {
                puzzleView.BackToOriginPosition();
            }
            
            await ETTask.CompletedTask;
        }
    }
}