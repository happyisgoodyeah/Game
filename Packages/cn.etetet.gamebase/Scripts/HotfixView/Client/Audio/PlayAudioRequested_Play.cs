namespace ET.Client
{
    [Event(SceneType.StateSync)]
    public class PlayAudioRequested_Play : AEvent<Scene, PlayAudioRequested>
    {
        protected override async ETTask Run(Scene scene, PlayAudioRequested args)
        {
            AudioComponent audio = scene.Root().GetComponent<AudioComponent>();
            if (audio != null) await audio.PlayAsync(args.Key);
        }
    }
}
