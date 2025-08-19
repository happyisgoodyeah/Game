using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(GridView))]
    [FriendOf(typeof(Puzzle))]
    [FriendOf(typeof(PuzzleView))]
    public class DragUpdate_PuzzleDrag : AEvent<Scene, DragUpdateEvent>
    {
        protected override async ETTask Run(Scene scene, DragUpdateEvent data)
        {
            //是Puzzle拖拽事件
            if (data.Entity is PuzzleView puzzleView)
            {
                Puzzle puzzle = puzzleView.GetParent<Puzzle>();
                
                if (puzzle.moveMode == PuzzleMoveModeType.Normal)
                {
                    puzzleView.transform.position = data.CurrentPosition;    
                }
                else if(puzzle.moveMode == PuzzleMoveModeType.Adsorption) //吸附模式
                { 
                     //如果鼠标位置超出了当前圆点slot坐标slot半径 则向对应方向整体移动
                     var pos1 = puzzleView.transform.position;
                     var slot = puzzle.slots[0].Entity;
                     var slotView = slot.GetComponent<SlotView>();
                     var r = slotView.GetComponent<SpriteRenderComponent>().SpriteSize.x;
                     //var closePosition = puzzleView.transform.GetComponent<PolygonCollider2D>().ClosestPoint(data.CurrentPosition);
                     var distance = Vector2.Distance(data.CurrentPosition , slotView.transform.position);
                     var dis = Mathf.Sqrt(r / 2 * r / 2 + r / 2 * r / 2);
                     //正方形 x y 大小相同
                     if (distance >= dis)
                     {
                         //获取对应方向
                         var dir = slotView.GetSnapDirection(data.CurrentPosition , r / 2);
                         var dirOffset = slot.GetDirOffset(dir);
                         puzzleView.transform.position += new Vector3(dirOffset.x * r , dirOffset.y * r , 0);
                     }
                }
            }

            await ETTask.CompletedTask;
        }
    }
}