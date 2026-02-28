using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public class OnTriggerEventMono : MonoBehaviour
    {
        /// <summary>
        /// 绑定的entity
        /// </summary>
        private Entity entity;

        /// <summary>
        /// 标签列表
        /// </summary>
        private List<string> tagList;

        /// <summary>
        /// 碰撞进入回调
        /// </summary>
        public Action triggerEnterAction;

        /// <summary>
        /// 碰撞持续回调
        /// </summary>
        public Action triggerStayAction;

        /// <summary>
        /// 碰撞退出回调
        /// </summary>
        public Action triggerExitAction;

        public void Awake()
        {
        }

        public void Start()
        {
        }

        /// <summary>
        /// 设置碰撞筛选标签
        /// </summary>
        /// <param name="colliderTag"></param>
        public void SetTag(List<string> colliderTag)
        {
            this.tagList = colliderTag;
        }

        /// <summary>
        /// 设置绑定的entity
        /// </summary>
        /// <param name="bindEntity"></param>
        public void SetEntity(Entity bindEntity)
        {
            this.entity = bindEntity;
        }

        /// <summary>
        /// 设置碰撞进入回调
        /// </summary>
        /// <param name="action"></param>
        public void SetTriggerEnterAction(Action action)
        {
            this.triggerEnterAction = action;
        }

        /// <summary>
        /// 设置碰撞持续回调
        /// </summary>
        /// <param name="action"></param>
        public void SetTriggerStayAction(Action action)
        {
            this.triggerStayAction = action;
        }

        /// <summary>
        /// 设置碰撞退出回调
        /// </summary>
        /// <param name="action"></param>
        public void SetTriggerExitAction(Action action)
        {
            this.triggerExitAction = action;
        }

        /// <summary>
        /// 碰撞进入
        /// </summary>
        /// <param name="other"></param>
        public void OnTriggerEnter2D(Collider2D other)
        {
            if (this.tagList == null)
            {
                return;
            }
            if (this.tagList.Contains(other.tag))
            {
                if (other.GetComponent<GameObjectEntityRef>() == null)
                {
                    Log.Error($"No GameObjectEntityRef in {other}");
                    return;
                }
                
                Log.Error($"触发Trigger2DEnter事件 触发obj --- {other.gameObject}");
                EventSystem.Instance.Publish(this.entity.Scene(),
                    new ColliderTriggerEnterEventMono
                            { 
                                bindEntity = this.entity, 
                                triggerEntity = other.gameObject.GetComponent<GameObjectEntityRef>().Entity,
                                collider = other,
                            });
                this.triggerEnterAction?.Invoke();
            }
        }

        /// <summary>
        /// 碰撞持续
        /// </summary>
        /// <param name="other"></param>
        public void OnTriggerStay2D(Collider2D other)
        {
            if (this.tagList == null)
            {
                return;
            }
            if (this.tagList.Contains(other.tag))
            {
                if (other.GetComponent<GameObjectEntityRef>() == null)
                {
                    Log.Error($"No GameObjectEntityRef in {other}");
                    return;
                }
                EventSystem.Instance.Publish(this.entity.Scene(),
                    new ColliderTriggerStayEventMono
                    {
                        bindEntity = this.entity, 
                        triggerEntity = other.gameObject.GetComponent<GameObjectEntityRef>().Entity,
                        collider = other,
                    });
                this.triggerStayAction?.Invoke();
            }
        }

        /// <summary>
        /// 碰撞退出
        /// </summary>
        /// <param name="other"></param>
        public void OnTriggerExit2D(Collider2D other)
        {
            if (this.tagList == null)
            {
                return;
            }
            if (other.GetComponent<GameObjectEntityRef>() == null)
            {
                Log.Error($"No GameObjectEntityRef in {other}");
                return;
            }
            if (this.tagList.Contains(other.tag))
            {
                //Log.Error($"触发Trigger2DExit事件 触发obj --- {other.gameObject}");
                if (this.entity == null)
                {
                    return;
                }
                EventSystem.Instance.Publish(this.entity.Scene(),
                    new ColliderTriggerExitEventMono
                    {
                        bindEntity = this.entity, 
                        triggerEntity = other.gameObject.GetComponent<GameObjectEntityRef>().Entity,
                        collider = other,
                    });
                this.triggerExitAction?.Invoke();
            }
        }
    }
}