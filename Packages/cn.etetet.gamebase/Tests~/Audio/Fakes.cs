// 隔离回归使用的边界替身。业务实现、ETTask 和 Luban 解码器直接链接生产源码。
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ET
{
    public class ComponentOfAttribute : Attribute { public ComponentOfAttribute(Type type) {} }
    public class ChildOfAttribute : Attribute { public ChildOfAttribute(Type type) {} }
    public class EntitySystemOfAttribute : Attribute { public EntitySystemOfAttribute(Type type) {} }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FriendOfAttribute : Attribute { public FriendOfAttribute(Type type) {} }
    public class EntitySystemAttribute : Attribute {}
    public class StaticFieldAttribute : Attribute {}
    public class EnableClassAttribute : Attribute {}
    public class ConfigAttribute : Attribute {}
    public interface ILubanConfig {}
    public interface IAwake {}
    public interface IUpdate {}
    public interface IDestroy {}
    public class Singleton<T> { public static T Instance; protected void EndInit() {} }
    public static class LubanLog { public static void Error(object category, int id) {} }
    public class FakeLog { public void Error(string text) {} }
    public class Fiber { public FakeLog Log = new(); }
    public class Entity
    {
        private static long next;
        public long Id = ++next;
        public long InstanceId = ++next;
        public bool IsDisposed => InstanceId == 0;
        public Entity Parent;
        public Dictionary<long, Entity> Children = new();
        public Dictionary<Type, Entity> Components = new();
        public Scene Root() => this is Scene scene ? scene : Parent.Root();
        public Fiber Fiber() => new();
        public T GetComponent<T>() where T : Entity => Components.TryGetValue(typeof(T), out Entity value) ? (T)value : null;
        public T AddComponent<T>() where T : Entity, new()
        {
            T item = new() { Parent = this }; Components.Add(typeof(T), item); Lifecycle(item, "Awake"); return item;
        }
        public T AddChild<T>() where T : Entity, new()
        {
            T item = new() { Parent = this }; Children.Add(item.Id, item); Lifecycle(item, "Awake"); return item;
        }
        public void Dispose()
        {
            if (IsDisposed) return;
            InstanceId = 0;
            foreach (Entity child in new List<Entity>(Children.Values)) child.Dispose();
            foreach (Entity child in new List<Entity>(Components.Values)) child.Dispose();
            Lifecycle(this, "Destroy");
            if (Parent != null && !Parent.IsDisposed) { Parent.Children.Remove(Id); Parent.Components.Remove(GetType()); }
        }
        private static void Lifecycle(Entity entity, string name)
        {
            Type system = typeof(Entity).Assembly.GetType("ET." + entity.GetType().Name + "System");
            system?.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, new object[] { entity });
        }
    }
    public class Scene : Entity {}
    public class GlobalComponent : Entity { public UnityEngine.Transform Audio = new UnityEngine.GameObject("Audio").transform; }
    public struct EntityRef<T> where T : Entity
    {
        private T entity;
        private long instance;
        public static implicit operator EntityRef<T>(T value) => new() { entity = value, instance = value.InstanceId };
        public static implicit operator T(EntityRef<T> value) => value.entity != null && value.entity.InstanceId == value.instance ? value.entity : null;
    }
}

namespace UnityEngine
{
    public class Object { public static void Destroy(Object value) { if (value is GameObject go) go.Destroyed = true; } }
    public class Transform : Object { public GameObject gameObject; public void SetParent(Transform parent, bool world) {} }
    public class GameObject : Object
    {
        public bool Destroyed, Active = true;
        public Transform transform;
        public GameObject(string name) { transform = new() { gameObject = this }; }
        public T AddComponent<T>() where T : AudioSource, new() => new() { gameObject = this };
        public void SetActive(bool active) => Active = active;
    }
    public class AudioClip : Object {}
    public class AudioSource : Object
    {
        public GameObject gameObject;
        public AudioClip clip;
        public bool playOnAwake, loop, mute, isPlaying;
        public float spatialBlend, dopplerLevel, pitch, volume;
        public int priority;
        private bool paused;
        public void Play() { isPlaying = true; paused = false; }
        public void Stop() { isPlaying = false; paused = false; }
        public void Pause() { paused = isPlaying || paused; isPlaying = false; }
        public void UnPause() { if (paused) isPlaying = true; paused = false; }
    }
    public static class AudioListener { public static bool pause; }
    public static class Time { public static double realtimeSinceStartupAsDouble; public static float unscaledDeltaTime; }
    public static class Random { public static float Range(float min, float max) => (min + max) / 2f; }
    public static class Mathf
    {
        public static float Clamp01(float value) => Math.Clamp(value, 0f, 1f);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
    }
    public static class PlayerPrefs
    {
        public static Dictionary<string, float> Values = new();
        public static int Saves;
        public static float GetFloat(string key, float fallback) => Values.GetValueOrDefault(key, fallback);
        public static int GetInt(string key, int fallback) => (int)GetFloat(key, fallback);
        public static void SetFloat(string key, float value) => Values[key] = value;
        public static void SetInt(string key, int value) => Values[key] = value;
        public static void Save() => ++Saves;
    }
}

namespace YooAsset
{
    public enum EOperationStatus { None, Succeed, Failed }
    public class AssetHandle
    {
        public TaskCompletionSource Completion = new();
        public Task Task => Completion.Task;
        public EOperationStatus Status;
        public UnityEngine.Object AssetObject;
        public string LastError = "test missing asset";
        public int Releases;
        public void Release() { if (++Releases != 1) throw new Exception("double release"); }
        public void Complete(bool success = true)
        {
            Status = success ? EOperationStatus.Succeed : EOperationStatus.Failed;
            AssetObject = success ? new UnityEngine.AudioClip() : null;
            Completion.SetResult();
        }
    }
    public class ResourcePackage
    {
        public bool Deferred, Fail;
        public List<AssetHandle> Requests = new();
        public AssetHandle LoadAssetAsync<T>(string path)
        {
            AssetHandle handle = new(); Requests.Add(handle);
            if (!Deferred) handle.Complete(!Fail);
            return handle;
        }
    }
    public static class YooAssets
    {
        public static ResourcePackage Package = new();
        public static ResourcePackage GetPackage(string name) => Package;
    }
}
