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
            // 计算到各边的距离
            float dx = Mathf.Min(Mathf.Abs(judgePosition.x - r), Mathf.Abs(judgePosition.x + r));
            float dy = Mathf.Min(Mathf.Abs(judgePosition.y - r), Mathf.Abs(judgePosition.y + r));
        
            // 确定最近点在哪个边界上
            if (dx < dy)
            {
                // 更靠近左右边界
                if (Mathf.Abs(judgePosition.x - r) < Mathf.Abs(judgePosition.x + r))
                    return new Vector2(r, Mathf.Clamp(judgePosition.y, -r, r)); // 右边界
                else
                    return new Vector2(-r, Mathf.Clamp(judgePosition.y, -r, r)); // 左边界
            }
            else
            {
                // 更靠近上下边界
                if (Mathf.Abs(judgePosition.y - r) < Mathf.Abs(judgePosition.y + r))
                    return new Vector2(Mathf.Clamp(judgePosition.x, -r, r), r); // 上边界
                else
                    return new Vector2(Mathf.Clamp(judgePosition.x, -r, r), -r); // 下边界
            }
        }
        
        /// <summary>
        /// 获取一个点相对于原点的方向
        /// </summary>
        /// <returns></returns>
        public static SnapDirection GetSnapDirection(Vector3 originPosition , Vector2 judgePosition, float r , bool isDistinction = false)
        {
            //将点转换到以A为中心的坐标系
            Vector2 relativePoint = judgePosition - new Vector2(originPosition.x , originPosition.y);
            
            if (GetOnePointInPointRange(originPosition , r , r , judgePosition))
            {
                return SnapDirection.None;
            }
            
            var closePoint = GetClosestPointOnSquare(relativePoint , r);
            
            // 计算法线角度
            float angle = Mathf.Atan2(closePoint.y, closePoint.x) * Mathf.Rad2Deg;
        
            // 标准化角度
            if (angle < 0) angle += 360;
        
            // 角度分区定义（以正右方为0度，逆时针旋转）
            const float cornerRange = 22.5f; // 角区域占22.5度
            const float edgeRange = 45f;     // 边区域占45度
        
            // 调整角度，使0度指向正右方
            angle = (angle + 360) % 360;
        
            // 右上角区域
            if (1.5f * angle > cornerRange && angle <= 2 * cornerRange)
            {
                return isDistinction ? SnapDirection.Right : SnapDirection.UpRight;    
            }
            
            // 右上角区域
            if (angle > 2 * cornerRange && angle <= 2.5f * cornerRange)
            {
                return isDistinction ? SnapDirection.Up : SnapDirection.UpRight;    
            }
        
            // 上边缘区域
            if (angle > 2.5f * cornerRange && angle <= 5.5f * cornerRange)
                return SnapDirection.Up;
        
            // 左上角区域
            if (angle > 5.5f * cornerRange && angle <= 6f * cornerRange)
            {
                return isDistinction ? SnapDirection.Up : SnapDirection.UpLeft;    
            }
            
            // 左上角区域
            if (angle > 6f * cornerRange && angle <= 6.5f * cornerRange)
            {
                return isDistinction ? SnapDirection.Left : SnapDirection.UpLeft;    
            }
        
            // 左边缘区域
            if (angle > 6.5f * cornerRange && angle <= 9.5f * cornerRange)
                return SnapDirection.Left;
        
            // 左下角区域
            if (angle > 9.5f * cornerRange && angle <= 10f * cornerRange)
            {
                return isDistinction ? SnapDirection.Left : SnapDirection.DownLeft;    
            }
            
            // 左下角区域
            if (angle > 10f * cornerRange && angle <= 10.5f * cornerRange)
            {
                return isDistinction ? SnapDirection.Down : SnapDirection.DownLeft;    
            }
        
            // 下边缘区域
            if (angle > 10.5f * cornerRange && angle <= 11f * cornerRange)
                return SnapDirection.Down;
        
            // 右下角区域
            if (angle > 11f * cornerRange && angle <= 11.5f * cornerRange)
            {
                return isDistinction ? SnapDirection.Down : SnapDirection.DownRight;   
            }
            
            // 右下角区域
            if (angle > 11.5f * cornerRange && angle <= 12f * cornerRange)
            {
                return isDistinction ? SnapDirection.Right : SnapDirection.DownRight;   
            }
            
            // 右边缘区域
            return SnapDirection.Right;
        }
    }
}