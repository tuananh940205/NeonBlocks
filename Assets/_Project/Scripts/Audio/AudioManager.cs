using System;
using System.Threading;
using System.Threading.Tasks;
using NeonBlocks.Assets;
using NeonBlocks.Gameplay;
using NeonBlocks.Save;
using NeonBlocks.Popups;
using UnityEngine;
namespace NeonBlocks.Audio
{
    /// <summary>Điều phối âm thanh thuần C#, sở hữu bank theo session.</summary>
    public sealed class AudioManager : IDisposable
    {
        private readonly AddressableManager assets;
        private readonly AssetOwnerToken owner = new AssetOwnerToken("Session:Audio");
        private AudioSettings settings;
        private bool gameplay;
        private float duck = 1;
        private string feedbackSession;
        private long sequence;
        /// <summary>Đăng ký listener trước khi tạo host.</summary>
        public AudioManager(AddressableManager assets)
        {
            this.assets = assets;
            ReadSave();
            ServiceLocator.Instance.Resolve<AudioPreviewSignal>().AddListener(Preview);
            ServiceLocator.Instance.Resolve<AudioTrackSignal>().AddListener(Track);
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().AddListener(Snapshot);
            ServiceLocator.Instance.Resolve<GameplayFeedbackSignal>().AddListener(Feedback);
            ServiceLocator.Instance.Resolve<SaveChangedSignal>().AddListener(ReadSave);
        }
        /// <summary>Tạo bank dưới SessionManager; prefab giữ clip dependencies.</summary>
        public async Task InitializeAsync(string key, Transform parent, CancellationToken ct)
        {
            await assets.InstantiateAsync(key, parent, owner, ct);
            Publish();
        }
        /// <summary>Đọc giá trị đã lưu, bao gồm mute trước âm thanh đầu tiên.</summary>
        private void ReadSave()
        {
            SaveData data = ServiceLocator.Instance.Resolve<LocalSaveManager>().Data;
            settings = new AudioSettings(data.MusicVolume, data.SfxVolume, data.Muted);
            Publish();
        }
        /// <summary>Áp dụng preview mà không ghi save.</summary>
        private void Preview(AudioSettings value) { settings = value; Publish(); }
        /// <summary>Chỉ đổi track khi scene active thay đổi.</summary>
        private void Track(bool value) { gameplay = value; duck = 1; Publish(); }
        /// <summary>Duck nhạc khi pause/gameover, không restart loop.</summary>
        private void Snapshot(GameplaySnapshot value)
        {
            duck = value.State == GameSessionState.GameOver ? .15f : value.State == GameSessionState.Paused ? .35f : 1;
            Publish();
        }
        /// <summary>Chặn replay cùng sequence và gửi cue sau commit.</summary>
        private void Feedback(GameplayFeedback value)
        {
            if (feedbackSession == value.SessionId && value.Sequence <= sequence) { return; }
            feedbackSession = value.SessionId; sequence = value.Sequence;
            ServiceLocator.Instance.Resolve<AudioCueSignal>().Dispatch(value.Kind);
        }
        /// <summary>Phát trạng thái bất biến cho host.</summary>
        private void Publish() => ServiceLocator.Instance.Resolve<AudioStateSignal>().Dispatch(new AudioState(settings, gameplay, duck));
        /// <summary>Dừng trước release và gỡ toàn bộ listener đối xứng.</summary>
        public void Dispose()
        {
            ServiceLocator.Instance.Resolve<AudioStopSignal>().Dispatch();
            ServiceLocator.Instance.Resolve<AudioPreviewSignal>().RemoveListener(Preview);
            ServiceLocator.Instance.Resolve<AudioTrackSignal>().RemoveListener(Track);
            ServiceLocator.Instance.Resolve<GameplaySnapshotSignal>().RemoveListener(Snapshot);
            ServiceLocator.Instance.Resolve<GameplayFeedbackSignal>().RemoveListener(Feedback);
            ServiceLocator.Instance.Resolve<SaveChangedSignal>().RemoveListener(ReadSave);
            assets.ReleaseOwnerAsync(owner).GetAwaiter().GetResult();
        }
    }
}