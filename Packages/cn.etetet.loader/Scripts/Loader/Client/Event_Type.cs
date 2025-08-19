using ET;
using UnityEngine;

/// <summary>
/// 碰撞进入
/// </summary>
public struct ColliderTriggerEnterEventMono
{
    public Entity bindEntity;
    public Entity triggerEntity;
    public Collider2D collider;
}

/// <summary>
/// 碰撞持续
/// </summary>
public struct ColliderTriggerStayEventMono
{
    public Entity bindEntity;
    public Entity triggerEntity;
    public Collider2D collider;
}

/// <summary>
/// 碰撞退出
/// </summary>
public struct ColliderTriggerExitEventMono
{
    public Entity bindEntity;
    public Entity triggerEntity;
    public Collider2D collider;
}