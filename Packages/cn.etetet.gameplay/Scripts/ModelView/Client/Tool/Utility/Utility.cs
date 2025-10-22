using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [EnableClass]
    public static partial class Utility
    {
        /// <summary>
        /// 判断一个点是否在一个点为原点 xy大小的矩形中
        /// </summary>
        /// <returns></returns>
        public static bool GetOnePointInPointRange(Vector3 originPoint, float x, float y, Vector3 judgePoint)
        {
            var right = originPoint.x + x;
            var left = originPoint.x - x;
            var up = originPoint.y + y;
            var down = originPoint.y - y;
            if ((judgePoint.x <= right && judgePoint.x >= left) &&
                (judgePoint.y <= up && judgePoint.y >= down))
            {
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// 获取一个点到其r*r矩形最近点的坐标
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Vector2 GetClosestPointOnSquare(Vector2 judgePosition , float r)
        {
            Vector2 final = judgePosition;
            if (judgePosition.x < -r)
            {
                final.x = -r;
            }
            if (judgePosition.x > r)
            {
                final.x = r;
            }
            
            if (judgePosition.y < -r)
            {
                final.x = -r;
            }
            if (judgePosition.x > r)
            {
                final.x = r;
            }

            return final;
        }

        /// <summary>
        /// 给定坐标以及对应最大X，Y值 返回所允许的方向
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="maxX"></param>
        /// <param name="maxY"></param>
        /// <returns></returns>
        public static SnapDirection GetAllowDirection(int x, int y , int maxX , int maxY)
        {
            if (x == 0 && y == 0)
            {
                return SnapDirection.UpLeft;
            }
            if (x == 0 && y == maxY - 1)
            {
                return SnapDirection.UpRight;
            }
            if (x == maxX - 1 && y == 0)
            {
                return SnapDirection.DownLeft;
            }
            if (x == maxX - 1 && y == maxY - 1)
            {
                return SnapDirection.DownRight;
            }

            return SnapDirection.None;
        }
        
        /// <summary>
        /// 获取一个点相对于原点的方向
        /// </summary>
        /// <param name="originPosition">原点方向</param>
        /// <param name="judgePosition">判定的点方向</param>
        /// <param name="isDistinction">是否合并四角方向为上下左右</param>
        /// <param name="limitDirections">限制四角方向只能返回数组内的值</param>
        /// <returns></returns>
        public static SnapDirection GetSnapDirection(Vector3 originPosition , Vector2 judgePosition , bool isDistinction = false , List<SnapDirection> limitDirections = null)
        {
            //将点转换到以A为中心的坐标系
            Vector2 relativePoint = judgePosition - new Vector2(originPosition.x , originPosition.y);
            
            // if (GetOnePointInPointRange(originPosition , r , r , judgePosition))
            // {
            //     return SnapDirection.None;
            // }
            
            //var closePoint = GetClosestPointOnSquare(relativePoint , r);
            
            // 计算法线角度
            float angle = Mathf.Atan2(relativePoint.y, relativePoint.x) * Mathf.Rad2Deg;
        
            // 标准化角度
            if (angle < 0) angle += 360;
        
            // 角度分区定义（以正右方为0度，逆时针旋转）
            const float cornerRange = 22.5f; // 角区域占22.5度
            const float edgeRange = 45f;     // 边区域占45度
        
            // 调整角度，使0度指向正右方
            angle = (angle + 360) % 360;
        
            // 右上角区域
            if (angle >= 1f * cornerRange && angle <= 2f * cornerRange)
            {
                return isDistinction ? SnapDirection.Right : limitDirections.Contains(SnapDirection.UpRight) ? SnapDirection.UpRight : SnapDirection.Right;    
            }
            
            // 右上角区域
            if (angle >= 2 * cornerRange && angle <= 3f * cornerRange)
            {
                return isDistinction ? SnapDirection.Up : limitDirections.Contains(SnapDirection.UpRight) ? SnapDirection.UpRight : SnapDirection.Up;    
            }
        
            // 上边缘区域
            if (angle >= 3f * cornerRange && angle <= 5f * cornerRange)
                return SnapDirection.Up;
        
            // 左上角区域
            if (angle >= 5f * cornerRange && angle <= 6f * cornerRange)
            {
                return isDistinction ? SnapDirection.Up : limitDirections.Contains(SnapDirection.UpLeft) ? SnapDirection.UpLeft : SnapDirection.Up;    
            }
            
            // 左上角区域
            if (angle >= 6f * cornerRange && angle <= 7f * cornerRange)
            {
                return isDistinction ? SnapDirection.Left : limitDirections.Contains(SnapDirection.UpLeft) ? SnapDirection.UpLeft : SnapDirection.Left;    
            }
        
            // 左边缘区域
            if (angle >= 7f * cornerRange && angle <= 9f * cornerRange)
                return SnapDirection.Left;
        
            // 左下角区域
            if (angle >= 9f * cornerRange && angle <= 10f * cornerRange)
            {
                return isDistinction ? SnapDirection.Left : limitDirections.Contains(SnapDirection.DownLeft) ? SnapDirection.DownLeft : SnapDirection.Left;    
            }
            
            // 左下角区域
            if (angle >= 10f * cornerRange && angle <= 11f * cornerRange)
            {
                return isDistinction ? SnapDirection.Down : limitDirections.Contains(SnapDirection.DownLeft) ? SnapDirection.DownLeft : SnapDirection.Down;    
            }
        
            // 下边缘区域
            if (angle >= 11f * cornerRange && angle <= 13f * cornerRange)
                return SnapDirection.Down;
        
            // 右下角区域
            if (angle >= 13f * cornerRange && angle <= 14f * cornerRange)
            {
                return isDistinction ? SnapDirection.Down : limitDirections.Contains(SnapDirection.DownRight) ? SnapDirection.DownRight : SnapDirection.Down;   
            }
            
            // 右下角区域
            if (angle >= 14f * cornerRange && angle <= 15f * cornerRange)
            {
                return isDistinction ? SnapDirection.Right : limitDirections.Contains(SnapDirection.DownRight) ? SnapDirection.DownRight : SnapDirection.Right;   
            }
            
            // 右边缘区域
            return SnapDirection.Right;
        }
    }
}