using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.Json;
using ET;
using Luban;
using UnityEngine;
using YooAsset;

internal static class Program
{
    private static int checks;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        ++checks; Console.WriteLine("PASS " + name);
    }
    private static AudioConfig Config(int id, string group = "Sound", int max = 4, int cooldown = 0,
        string overflow = "Reject", float fade = 0f, float volume = 0.8f, string key = null)
    {
        ByteBuf b = new();
        b.WriteInt(id); b.WriteString("clip"); b.WriteString(key ?? "sound" + id); b.WriteString(group);
        b.WriteFloat(volume); b.WriteBool(group == "Music"); b.WriteFloat(1f); b.WriteFloat(1f);
        b.WriteFloat(fade); b.WriteFloat(fade); b.WriteInt(max); b.WriteInt(cooldown); b.WriteInt(128); b.WriteString(overflow);
        return new AudioConfig(b);
    }
    private static AudioComponent Create(params AudioConfig[] configs)
    {
        Time.realtimeSinceStartupAsDouble = 10;
        AudioListener.pause = false; PlayerPrefs.Values.Clear(); PlayerPrefs.Saves = 0;
        YooAssets.Package = new();
        ByteBuf b = new(); b.WriteSize(0);
        AudioConfigCategory.Instance = new AudioConfigCategory(b);
        foreach (AudioConfig c in configs) { AudioConfigCategory.Instance.DataMap.Add(c.Id, c); AudioConfigCategory.Instance.DataList.Add(c); }
        Scene root = new(); root.AddComponent<GlobalComponent>();
        return root.AddComponent<AudioComponent>();
    }
    private static AudioVoice Voice(AudioComponent audio, long id) => audio.Voices[id];
    private static void Tick(AudioComponent audio, float seconds)
    {
        Time.realtimeSinceStartupAsDouble += seconds;
        typeof(AudioComponentSystem).GetMethod("Tick", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, new object[] { audio, seconds });
    }
    private static async Task Main()
    {
        AudioComponent a = Create(Config(0));
        Check(a.ActiveCount() == 0, "Awake does not auto-play samples");
        Check((await a.PlayAsync("missing")).Status == AudioPlayStatus.InvalidConfig, "unknown key rejected");
        Check(!AudioComponentSystem.ValidateConfig(Config(3, volume: float.NaN), out _), "NaN config rejected");
        AudioPlayResult first = await a.PlayAsync("sound0");
        AudioSource source = Voice(a, first.Handle).Source;
        Check(first.Status == AudioPlayStatus.Started && a.ActiveCount() == 1, "key lookup starts one voice");
        a.SetMasterVolume(0.5f); a.SetGroupVolume(AudioGroup.Sound, 0.25f);
        Check(Math.Abs(source.volume - 0.1f) < 0.0001f, "base/master/group gain multiplied once");
        a.SetMuted(true); Check(source.mute, "mute affects current voice");
        a.SetMuted(false); Check(!source.mute && source.volume == 0.1f, "unmute preserves volume");
        a.Pause(first.Handle); Tick(a, 10f);
        Check(a.IsActive(first.Handle), "paused voice not recycled as finished");
        a.PauseGroup(AudioGroup.Sound, true); a.Resume(first.Handle);
        Check(!source.isPlaying, "individual resume respects group pause");
        a.PauseGroup(AudioGroup.Sound, false); Check(source.isPlaying, "group resume restores playback");
        source.isPlaying = false; Tick(a, 0.01f);
        Check(a.ActiveCount() == 0 && YooAssets.Package.Requests[0].Releases == 1 && source.clip == null, "natural finish releases clip and handle");
        AudioPlayResult second = await a.PlayAsync(0);
        Check(ReferenceEquals(source, Voice(a, second.Handle).Source) && second.Handle != first.Handle, "source reused with fresh handle");
        Check(!a.Stop(first.Handle), "stale handle cannot stop new voice");
        a.StopAll(); a.Dispose();
        Check(YooAssets.Package.Requests.All(x => x.Releases == 1), "destroy does not double release");

        a = Create(Config(0, max: 1, cooldown: 100));
        first = await a.PlayAsync(0);
        Check((await a.PlayAsync(0)).Status == AudioPlayStatus.Cooldown, "cooldown blocks repeated request");
        Time.realtimeSinceStartupAsDouble += 1;
        Check((await a.PlayAsync(0)).Status == AudioPlayStatus.CapacityReached, "per-id limit enforced");
        a.Dispose();
        a = Create(Config(0, max: 1, overflow: "ReplaceOldest"));
        first = await a.PlayAsync(0); second = await a.PlayAsync(0);
        Check(!a.IsActive(first.Handle) && a.IsActive(second.Handle) && a.ActiveCount() == 1, "oldest voice replaced at capacity");
        a.Dispose();
        a = Create(Config(0, max: 10)); a.ConfigureLimits(2, 1);
        await a.PlayAsync(0); await a.PlayAsync(0);
        Check((await a.PlayAsync(0)).Status == AudioPlayStatus.CapacityReached, "global voice budget enforced");
        a.StopAll(); Check(a.SourcePool.Count == 1, "source pool bounded"); a.Dispose();

        a = Create(Config(0, fade: 1f));
        first = await a.PlayAsync(0); source = Voice(a, first.Handle).Source;
        Tick(a, 0.5f); Check(Math.Abs(source.volume - 0.4f) < 0.0001f, "fade-in uses elapsed unscaled time");
        a.Stop(first.Handle); Tick(a, 1f); Check(!a.IsActive(first.Handle), "fade-out recycles voice"); a.Dispose();

        a = Create(Config(0)); YooAssets.Package.Deferred = true;
        ETTask<AudioPlayResult> pending = a.PlayAsync(0);
        Check(a.ActiveCount() == 1, "loading reserves voice budget");
        a.StopAll(); YooAssets.Package.Requests[0].Complete();
        Check((await pending).Status == AudioPlayStatus.Cancelled && a.ActiveCount() == 0 && YooAssets.Package.Requests[0].Releases == 1,
            "stop during load prevents late playback and releases handle"); a.Dispose();
        a = Create(Config(0)); YooAssets.Package.Deferred = true;
        pending = a.PlayAsync(0); a.Dispose(); YooAssets.Package.Requests[0].Complete();
        Check((await pending).Status == AudioPlayStatus.Cancelled && YooAssets.Package.Requests[0].Releases == 1,
            "destroy during load safely releases completed request");

        a = Create(Config(0, fade: 1)); YooAssets.Package.Deferred = true;
        pending = a.PlayAsync(0); a.PauseGroup(AudioGroup.Sound, true); YooAssets.Package.Requests[0].Complete();
        first = await pending; source = Voice(a, first.Handle).Source; Tick(a, 3f);
        Check(a.IsActive(first.Handle) && !source.isPlaying, "group pause also applies to late-loaded audio");
        a.StopGroup(AudioGroup.Sound);
        Check(a.ActiveCount() == 0 && YooAssets.Package.Requests[0].Releases == 1, "stopping paused group releases resources"); a.Dispose();
        a = Create(Config(0, fade: 1)); first = await a.PlayAsync(0); AudioListener.pause = true;
        a.Stop(first.Handle); Check(a.ActiveCount() == 0, "stop while listener paused does not leave a stuck fade"); a.Dispose();
        a = Create(Config(0)); YooAssets.Package.Fail = true;
        Check((await a.PlayAsync(0)).Status == AudioPlayStatus.Failed && a.ActiveCount() == 0 && YooAssets.Package.Requests[0].Releases == 1,
            "failed load releases reservation and resource handle"); a.Dispose();

        a = Create(Config(0, "Music", fade: 1), Config(1, "Music", fade: 1), Config(2, "Music", fade: 1));
        first = await a.PlayAsync(0);
        Check((await a.PlayAsync(0)).Handle == first.Handle && a.ActiveCount() == 1, "same music does not restart");
        YooAssets.Package.Deferred = true;
        pending = a.PlayAsync(1); ETTask<AudioPlayResult> latest = a.PlayAsync(2);
        YooAssets.Package.Requests[2].Complete(); second = await latest;
        YooAssets.Package.Requests[1].Complete();
        Check((await pending).Status == AudioPlayStatus.Cancelled && a.IsActive(second.Handle), "out-of-order music loads use latest request");
        Tick(a, 1.1f); Check(a.ActiveCount() == 1 && !a.IsActive(first.Handle), "music crossfade releases previous track");
        a.Dispose();
        a = Create(Config(0, "Music"), Config(1, "Music")); first = await a.PlayAsync(0); YooAssets.Package.Fail = true;
        Check((await a.PlayAsync(1)).Status == AudioPlayStatus.Failed && a.IsActive(first.Handle), "failed replacement keeps previous music");
        a.Dispose();

        a = Create(Config(0), Config(1, key: "sound0"));
        Check((await a.PlayAsync("sound0")).Status == AudioPlayStatus.InvalidConfig, "duplicate keys rejected"); a.Dispose();
        a = Create(Config(0)); a.SetGroupVolume(AudioGroup.Sound, 0.3f); Tick(a, 0.6f);
        Check(PlayerPrefs.Saves == 1, "settings changes saved after debounce");
        a.Dispose(); AudioComponent restored = new Scene().AddComponent<GlobalComponent>().Root().AddComponent<AudioComponent>();
        Check(restored.GetGroupVolume(AudioGroup.Sound) == 0.3f, "settings restored on next manager"); restored.Dispose();

        // 从工作目录寻找仓库，支持从仓库根或测试目录执行。
        DirectoryInfo dir = new(Directory.GetCurrentDirectory());
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ET.sln"))) dir = dir.Parent;
        if (dir == null) throw new Exception("Repository root not found");
        foreach (string mode in new[] { "Client", "Server", "ClientServer" })
        {
            byte[] bytes = File.ReadAllBytes(Path.Combine(dir.FullName, $"Packages/cn.etetet.yiuilubangen/Assets/Config/Binary/{mode}/AudioConfigCategory.bytes"));
            AudioConfigCategory category = new(new ByteBuf(bytes));
            using JsonDocument json = JsonDocument.Parse(File.ReadAllText(Path.Combine(dir.FullName,
                $"Packages/cn.etetet.yiuilubangen/Assets/Config/Json/{mode}/AudioConfigCategory.json")));
            bool equal = category.DataList.Count == json.RootElement.GetArrayLength();
            foreach (JsonElement row in json.RootElement.EnumerateArray())
            {
                AudioConfig config = category.Get(row.GetProperty("Id").GetInt32());
                equal &= AudioComponentSystem.ValidateConfig(config, out _);
                foreach (FieldInfo field in typeof(AudioConfig).GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    object expected = JsonSerializer.Deserialize(row.GetProperty(field.Name).GetRawText(), field.FieldType);
                    equal &= Equals(field.GetValue(config), expected);
                }
            }
            Check(equal,
                mode + " generated binary decodes with current schema");
        }
        Console.WriteLine($"PASS {checks} checks. Unity, Entity lifecycle and YooAsset boundaries use fakes; ETTask/config decoder are production sources.");
    }
}
