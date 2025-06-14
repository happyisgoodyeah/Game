using UnityEngine;

namespace ET
{
    [ComponentOf()]
    public partial class SpriteRenderComponent : Entity , IAwake<GameObject>
    {
        /// <summary>
        /// SpriteRender
        /// </summary>
        public SpriteRenderer SpriteRenderer { get; set; }

        /// <summary>
        /// SpriteSize
        /// </summary>
        public Vector2 SpriteSize { get; set; }
    }    
}