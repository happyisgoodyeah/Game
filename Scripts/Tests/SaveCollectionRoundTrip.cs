using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ET;
using MemoryPack;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

// 独立测试程序，由验证脚本使用项目实际的 Model 和序列化程序集编译运行。
public static class SaveCollectionRoundTrip
{
    public sealed class JsonFixture
    {
        public SerializableList<long> Levels { get; set; }
        public SerializableList<string> Achievements { get; set; }
    }

    public static int Main(string[] args)
    {
        var paths = File.ReadAllLines(args[0]);
        AppDomain.CurrentDomain.AssemblyResolve += (sender, request) =>
        {
            string name = new AssemblyName(request.Name).Name;
            string path = paths.FirstOrDefault(candidate => Path.GetFileNameWithoutExtension(candidate) == name);
            return path == null ? null : Assembly.LoadFrom(path);
        };
        try
        {
            Run();
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Run()
    {
        var levels = new SerializableList<long> { 1001, 1002, 1013 };
        levels.Capacity = 16;
        var loadedLevels = MemoryPackSerializer.Deserialize<SerializableList<long>>(MemoryPackSerializer.Serialize(levels));
        Require(loadedLevels.SequenceEqual(levels), "MemoryPack lost level IDs");

        var achievements = new SerializableList<string> { "first_win", "拼图完成", null };
        var loadedAchievements = MemoryPackSerializer.Deserialize<SerializableList<string>>(MemoryPackSerializer.Serialize(achievements));
        Require(loadedAchievements.SequenceEqual(achievements), "MemoryPack lost achievement IDs");

        var empty = new SerializableList<long>();
        Require(MemoryPackSerializer.Deserialize<SerializableList<long>>(MemoryPackSerializer.Serialize(empty)).Count == 0,
            "MemoryPack empty list failed");

        // 旧格式只写入对象成员数、Capacity 和 Count，总计 9 字节，没有元素。
        byte[] legacy = { 2, 11, 0, 0, 0, 11, 0, 0, 0 };
        var legacyLevels = MemoryPackSerializer.Deserialize<SerializableList<long>>(legacy);
        Require(legacyLevels.Count == 0 && legacyLevels.Capacity == 11, "Legacy list format no longer readable");

        var fixture = new JsonFixture { Levels = levels, Achievements = achievements };
        var loadedJson = BsonSerializer.Deserialize<JsonFixture>(fixture.ToJson());
        Require(loadedJson.Levels.SequenceEqual(levels), "MongoDB JSON lost level IDs");
        Require(loadedJson.Achievements.SequenceEqual(achievements), "MongoDB JSON lost achievement IDs");
        Console.WriteLine("PASS: MemoryPack levels, achievements, empty lists, legacy format and MongoDB JSON round trips.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }
}
