using ET.Client;
using UnityEngine;

namespace ET
{
    [Event(SceneType.StateSync)]
    [FriendOf(typeof(GridView))]
    [FriendOf(typeof(Grid))]
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
                     var closePosition = puzzleView.transform.GetComponent<PolygonCollider2D>().ClosestPoint(data.CurrentPosition);
                     var distance = Vector2.Distance(data.CurrentPosition , slotView.transform.position);
                     var dis = Mathf.Sqrt(r / 2 * r / 2 + r / 2 * r / 2);
                     
                     //左右0.5r 上下1.5r 则退出吸附 转为普通移动模式
                     var grid = puzzle.GetParent<Grid>();
                     var gridSize = grid.gridSize;
                     var gridSizeX = gridSize.X * grid.cellSize;
                     var gridSizeY = gridSize.Y * grid.cellSize;
                     var xNotCan = (data.CurrentPosition.x - gridSizeX / 2f > r / 2 || data.CurrentPosition.x + gridSizeX / 2f < -r / 2);
                     var yNotCan = (data.CurrentPosition.y - gridSizeY / 2f > r / 2 || data.CurrentPosition.y + gridSizeY / 2f < -r / 2);

                     if (xNotCan || yNotCan)
                     {
                         puzzle.ChangeMoveMode(PuzzleMoveModeType.Normal);
                         return;
                     }

                     if (!puzzleView.transform.GetComponent<PolygonCollider2D>().bounds.Contains(data.CurrentPosition))
                     {
                         //获取对应方向
                         var dir = slotView.GetSnapDirection(data.CurrentPosition , r / 2 , true);
                         if (dir == SnapDirection.UpLeft || dir == SnapDirection.DownLeft) dir = SnapDirection.Left;
                         if (dir == SnapDirection.UpRight || dir == SnapDirection.DownRight) dir = SnapDirection.Right;
                         var dirOffset = slot.GetDirOffset(dir);
                         puzzleView.transform.position += new Vector3(dirOffset.x * r , dirOffset.y * r , 0);
                     }
                        
                     //正方形 x y 大小相同
                     // if (distance >= dis)
                     // {
                     //     //获取对应方向
                     //     var dir = slotView.GetSnapDirection(data.CurrentPosition , r / 2);
                     //     var dirOffset = slot.GetDirOffset(dir);
                     //     puzzleView.transform.position += new Vector3(dirOffset.x * r , dirOffset.y * r , 0);
                     // }
                }
            }

            await ETTask.CompletedTask;
        }
    }
}