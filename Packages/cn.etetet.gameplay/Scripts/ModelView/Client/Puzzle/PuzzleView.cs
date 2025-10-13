using DG.Tweening;
using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Puzzle))]
    public class PuzzleView : Entity, IAwake<Transform>
    {
        public Transform transform { get; set; }

        /// <summary>
        /// 移动起始startPos
        /// </summary>
        public Vector3 startPos;
        
        /// <summary>
        /// 移动目标endPos
        /// </summary>
        public Vector3 endPos;

        public Transform parentTransform { get; set; }

        public Vector3 originPosition { get; set; }
        
        public Tweener tweener { get; set; }
    }
}