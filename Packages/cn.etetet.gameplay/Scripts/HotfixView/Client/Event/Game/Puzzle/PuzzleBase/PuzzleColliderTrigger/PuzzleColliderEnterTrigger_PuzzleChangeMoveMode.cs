using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(GridView))]
    [FriendOf(typeof(SlotView))]
    public class PuzzleColliderEnterTrigger_PuzzleChangeMoveMode : AEvent<Scene , ColliderTriggerEnterEventMono>
    {
        protected override async ETTask Run(Scene scene, ColliderTriggerEnterEventMono data)
        {
            var bindEntity = data.bindEntity;
            var triggerEntity = data.triggerEntity;
            if (bindEntity is PuzzleView puzzleView && triggerEntity is GridView gridView)
            {
                Puzzle puzzle = puzzleView.GetParent<Puzzle>();
                Grid grid = gridView.GetParent<Grid>();
                
                //改为吸附模式 先对齐对应的slot
                if (puzzle.moveMode == PuzzleMoveModeType.Normal)
                {
                    var collider = data.collider;
                    
                    //碰撞点
                    Vector3 closePosition = collider.ClosestPoint(puzzleView.transform.position);
                    closePosition = new Vector3(closePosition.x > 0 ? closePosition.x - 0.01f : closePosition.x + 0.01f,
                        closePosition.y > 0 ? closePosition.y - 0.01f : closePosition.y + 0.01f, 0);
                    
                    var index = grid.WorldToGridPosition(new FloatVector2(closePosition.x , closePosition.y));
                    puzzle.ChangeMoveMode(PuzzleMoveModeType.Adsorption);
                    var slot = grid.GetSlot(index);
                    var slotView = slot.GetComponent<SlotView>();
                    var slotClosePosition = slotView.transform.GetComponent<BoxCollider2D>().ClosestPoint(puzzleView.transform.position);
                    var direction = slotView.GetSnapDirection(slotClosePosition.x , slotClosePosition.y);
                    var directionOffset = slotView.GetParent<Slot>().GetResetOffset(direction);
                    
                }
            }
            await ETTask.CompletedTask;
        }
    }    
}