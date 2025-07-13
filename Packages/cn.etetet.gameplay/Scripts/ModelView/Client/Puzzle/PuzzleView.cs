using UnityEngine;

namespace ET
{
    [ComponentOf(typeof(Puzzle))]
    public class PuzzleView : Entity, IAwake<Transform>
    {
        public Transform transform { get; set; }

        public Transform parentTransform { get; set; }

        public Vector3 originPosition { get; set; }
    }
}