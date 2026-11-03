using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public partial class AudioComponent : Entity, IAwake, IUpdate, IDestroy
    {
        public Transform AudioRoot;
        public ResourcePackage Package;
        public Dictionary<long, EntityRef<AudioVoice>> Voices = new();
        public Stack<AudioSource> SourcePool = new();
        public List<long> ScratchIds = new();
        public Dictionary<string, int> ConfigIds = new();
        public Dictionary<int, double> LastRequests = new();
        public float[] GroupVolumes = new float[4];
        public bool[] GroupMutes = new bool[4];
        public bool[] GroupPauses = new bool[4];
        public float MasterVolume;
        public bool Muted;
        public bool PreferencesDirty;
        public double SavePreferencesAt;
        public int MaxVoices;
        public int MaxPooledSources;
    }
    
}
