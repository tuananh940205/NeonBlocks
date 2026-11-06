using NeonBlocks.Config;
using NeonBlocks.Gameplay;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
namespace NeonBlocks.Audio
{
    /// <summary>Adapter Unity: source 2D, crossfade unscaled và voice pool hữu hạn.</summary>
    public sealed class AudioHost : MonoBehaviour
    {
        [SerializeField] private AudioSource menuSource, gameplaySource;
        [SerializeField] private AudioSource[] voices;
        [SerializeField] private AudioClip[] effects;
        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioMixerGroup musicGroup, sfxGroup;
        private AudioState state;
        private float blend, rotateTime = -1;
        private bool? currentTrack;
        private string sceneName;
        private int[] priorities;
        /// <summary>Gắn listener; tất cả source bắt đầu im lặng.</summary>
        private void OnEnable()
        {
            if (!Application.isPlaying) { return; }
            priorities = new int[voices.Length];
            menuSource.volume = gameplaySource.volume = 0;
            menuSource.outputAudioMixerGroup = musicGroup; gameplaySource.outputAudioMixerGroup = musicGroup;
            foreach (AudioSource source in voices) { source.outputAudioMixerGroup = sfxGroup; }
            ServiceLocator.Instance.Resolve<AudioStateSignal>().AddListener(Apply);
            ServiceLocator.Instance.Resolve<AudioCueSignal>().AddListener(Cue);
            ServiceLocator.Instance.Resolve<AudioStopSignal>().AddListener(Stop);
        }
        /// <summary>Nhận state; không gọi Play lại khi pause hoặc chỉnh volume.</summary>
        private void Apply(AudioState value)
        {
            state = value;
            if (currentTrack != value.Gameplay)
            {
                currentTrack = value.Gameplay;
                AudioSource target = value.Gameplay ? gameplaySource : menuSource;
                if (!target.isPlaying) { target.Play(); }
            }
            SetVolumes();
        }
        /// <summary>Đọc scene active và chạy fade không phụ thuộc thời gian gameplay.</summary>
        private void Update()
        {
            string active = SceneManager.GetActiveScene().name;
            if (active != sceneName)
            {
                sceneName = active;
                ServiceLocator.Instance.Resolve<AudioTrackSignal>().Dispatch(active == ServiceLocator.Instance.Resolve<ConfigManager>().Config.Scenes.Gameplay);
            }
            if (state == null) { return; }
            blend = Mathf.MoveTowards(blend, state.Gameplay ? 1 : 0, Time.unscaledDeltaTime / .4f);
            SetVolumes();
            if (blend == 1 && menuSource.isPlaying) { menuSource.Stop(); }
            if (blend == 0 && gameplaySource.isPlaying) { gameplaySource.Stop(); }
        }
        /// <summary>Áp dụng mute, mixer nếu đã gán; fallback source gain hoạt động ngay.</summary>
        private void SetVolumes()
        {
            if (state == null) { return; }
            float music = state.Settings.Muted ? 0 : state.Settings.Music;
            float sfx = state.Settings.Muted ? 0 : state.Settings.Sfx;
            bool musicMixed = mixer != null && mixer.SetFloat("MusicVolumeDb", GainToDb(music));
            bool sfxMixed = mixer != null && mixer.SetFloat("SfxVolumeDb", GainToDb(sfx));
            float gain = (musicMixed ? 1 : music) * state.Duck;
            menuSource.volume = (1 - blend) * gain; gameplaySource.volume = blend * gain;
            foreach (AudioSource source in voices) { source.volume = sfxMixed ? 1 : sfx; }
        }
        /// <summary>Chuyển linear gain sang dB, mute về -80.</summary>
        public static float GainToDb(float value) => value <= .0001f ? -80 : 20 * Mathf.Log10(Mathf.Clamp01(value));
        /// <summary>Ưu tiên Clear/GameOver, throttle rotate; không tạo source mới.</summary>
        private void Cue(FeedbackKind kind)
        {
            if (state == null || state.Settings.Muted || state.Settings.Sfx <= 0) { return; }
            if (kind == FeedbackKind.Rotate && Time.unscaledTime - rotateTime < .04f) { return; }
            if (kind == FeedbackKind.Rotate) { rotateTime = Time.unscaledTime; }
            int priority = kind == FeedbackKind.GameOver ? 3 : kind == FeedbackKind.Clear ? 2 : kind == FeedbackKind.Rotate ? 0 : 1;
            int index = -1;
            for (int i = 0; i < voices.Length; i++)
            {
                if (!voices[i].isPlaying) { index = i; break; }
                if (priorities[i] <= priority && (index < 0 || priorities[i] < priorities[index])) { index = i; }
            }
            if (index < 0 || (int)kind >= effects.Length) { return; }
            priorities[index] = priority; voices[index].clip = effects[(int)kind]; voices[index].Play();
        }
        /// <summary>Dừng nguồn trước khi instance bị release.</summary>
        private void Stop() { menuSource.Stop(); gameplaySource.Stop(); foreach (AudioSource source in voices) { source.Stop(); } }
        /// <summary>Gỡ listener đối xứng khi session kết thúc.</summary>
        private void OnDisable()
        {
            if (!Application.isPlaying) { return; }
            Stop();
            ServiceLocator.Instance.Resolve<AudioStateSignal>().RemoveListener(Apply);
            ServiceLocator.Instance.Resolve<AudioCueSignal>().RemoveListener(Cue);
            ServiceLocator.Instance.Resolve<AudioStopSignal>().RemoveListener(Stop);
        }
    }
}