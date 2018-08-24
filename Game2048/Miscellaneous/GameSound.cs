using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Game2048
{
    class GameSoundPlayer
    {
        bool bgmOn = true;

        MediaPlayer sfx1Player = new MediaPlayer();
        MediaPlayer sfx2Player = new MediaPlayer();
        MediaPlayer bgmPlayer = new MediaPlayer();

        public GameSoundPlayer()
        {
            bgmPlayer.Open(new Uri("Breezala.mp3", UriKind.Relative));
            bgmPlayer.MediaEnded += (sender, e) => { ((MediaPlayer)sender).Position = new TimeSpan(0); };
            sfx1Player.Open(new Uri("socket.wav", UriKind.Relative));
            sfx1Player.MediaEnded += (sender, e) => { sfx1Player.Stop(); sfx1Player.Position = new TimeSpan(0); };
            sfx2Player.Open(new Uri("wall.wav", UriKind.Relative));
            sfx2Player.MediaEnded += (sender, e) => { sfx2Player.Stop(); sfx2Player.Position = new TimeSpan(0); };
        }

        public double BgmVolume
        {
            get => bgmPlayer.Volume;
            set => bgmPlayer.Volume = value;
        }
        public double SfxVolume
        {
            get => sfx1Player.Volume;
            set
            {
                sfx1Player.Volume = value;
                sfx2Player.Volume = value;
            }
        }

        public bool BgmOn
        {
            get => bgmOn;
            set
            {
                bgmOn = value;
                bgmPlayer.Stop();
                if(bgmOn)
                {
                    bgmPlayer.Play();
                }
            }
        }
        public bool SfxOn { get; set; } = true;

        public void PlayBgm()
        {
            bgmOn = true;
        }
        public void StopBgm()
        {
            bgmOn = false;
        }
        public void PlaySfx(int id)
        {
            if (SfxOn)
            {
                if (id == 2)
                {
                    sfx2Player.Play();
                }
                else
                {
                    sfx1Player.Play();
                }
            }
        }

        public void Config(SoundState state)
        {
            BgmOn = state.BgmOn;
            SfxOn = state.SfxOn;
            BgmVolume = state.BgmVolume;
            SfxVolume = state.SfxVolume;
        }
    }

    [Serializable]
    class SoundState
    {
        public SoundState() { }

        public SoundState(bool bgmOn, bool sfxOn)
        {
            BgmOn = bgmOn;
            SfxOn = sfxOn;
        }

        public SoundState(bool bgmOn, double bgmVolume, bool sfxOn, double sfxVolume)
        {
            BgmOn = bgmOn;
            BgmVolume = bgmVolume;
            SfxOn = sfxOn;
            SfxVolume = sfxVolume;
        }

        public bool BgmOn { get; set; } = true;
        public double BgmVolume { get; set; } = 0.5;
        public bool SfxOn { get; set; } = true;
        public double SfxVolume { get; set; } = 0.5;

        public static SoundState Default => new SoundState();
    }
}
