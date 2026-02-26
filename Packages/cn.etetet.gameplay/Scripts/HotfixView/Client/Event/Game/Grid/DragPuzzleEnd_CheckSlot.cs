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

            puzzleView.tweener?.Kill();
            puzzleView.tweener = null;

            //将图层层级降回去
            puzzleView.ChangePuzzleLayOut(1);

            //拖拽结束的位置
            var worldPosition = data.worldPosition;
            FloatVector2 position = new FloatVector2(worldPosition.x, worldPosition.y);

            //尝试放置并绑定拼图
            if (grid.TryPlaceAndBindPuzzle(puzzle, position, out IntVector2 originPosition))
            {
                puzzleView.transform.position = grid.GetSlot(originPosition).GetComponent<SlotView>().transform.position;
                return;
            }

            puzzleView.BackToOriginPosition();

            await ETTask.CompletedTask;
        }
    }
}