using System.Collections.Generic;
using DG.Tweening;
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

            puzzleView.tweener?.Kill();
            puzzleView.tweener = null;
            // puzzle.ChangeMoveMode(PuzzleMoveModeType.Normal);

            //将图层层级降回去
            puzzleView.ChangePuzzleLayOut(1);

            //拖拽结束的位置
            var worldPosition = data.worldPosition;
            FloatVector2 position = new FloatVector2(worldPosition.x, worldPosition.y);

            //拖拽结束的位置位于grid中
            if (grid.ContainsPosition(position))
            {
                IntVector2 originPosition = grid.WorldToGridPosition(position);
                //能够放置拼图
                if (grid.CanPlacePuzzle(puzzle, originPosition))
                {
                    puzzle.ResetBindSlots();

                    List<IntVector2> positionList = grid.GetCoveredPositions(puzzle, originPosition);
                    foreach (IntVector2 slotPosition in positionList)
                    {
                        Slot slot = grid.GetSlot(slotPosition);
                        slot.SetPuzzle(puzzle);
                        puzzle.bindSlots.Add(slot);
                    }

                    puzzleView.transform.position = grid.GetSlot(originPosition).GetComponent<SlotView>().transform.position;
                    return;
                }
            }

            puzzleView.BackToOriginPosition();

            await ETTask.CompletedTask;
        }
    }
}