using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.ViewModels.Events;

namespace SimpleSongsPlayer.Views.Controllers
{
    public abstract class LyricsPreviewControlBase : UserControl
    {
        public static readonly DependencyProperty LyricsProperty = DependencyProperty.Register(
            nameof(Lyrics), typeof(IList<LyricLine>), typeof(LyricsPreviewControlBase), new PropertyMetadata(null));

        public static readonly DependencyProperty CurrentLyricProperty = DependencyProperty.Register(
            nameof(CurrentLyric), typeof(LyricLine), typeof(LyricsPreviewControlBase), new PropertyMetadata(LyricLine.Empty));
        
        protected LyricLine BackLyric;
        protected int NextIndex;

        public IList<LyricLine> Lyrics
        {
            get => (IList<LyricLine>) GetValue(LyricsProperty);
            set => SetValue(LyricsProperty, value);
        }

        public LyricLine CurrentLyric
        {
            get => (LyricLine) GetValue(CurrentLyricProperty);
            set
            {
                BackLyric = GetValue(CurrentLyricProperty) as LyricLine;
                SetValue(CurrentLyricProperty, value); 
                Refreshed?.Invoke(this, new LyricsPreviewRefreshEventArgs(value));
            }
        }

        public event TypedEventHandler<LyricsPreviewControlBase, LyricsPreviewRefreshEventArgs> Refreshed;

        protected bool CanPreview => IsEnabled && Visibility == Visibility.Visible && Lyrics != null && Lyrics.Any();
        
        public void RefreshLyric(TimeSpan position)
        {
            if (!CanPreview)
                return;

            if (NextIndex >= Lyrics.Count)
                NextIndex = 0;

            long currentPositionTicks = position.Ticks;
            LyricLine nextLyric = Lyrics[NextIndex];
            long nextLyricTimeTicks = nextLyric.Time.Ticks;
            long nextLyricEndTimeTicks = nextLyricTimeTicks + 10000000;

            if (currentPositionTicks >= nextLyricTimeTicks && currentPositionTicks < nextLyricEndTimeTicks)
            {
                NextIndex++;
                CurrentLyric = nextLyric;
            }
        }

        public void Reposition(TimeSpan position)
        {
            if (!CanPreview)
                return;
            
            if (position.CompareTo(Lyrics.First().Time) <= 0)
            {
                NextIndex = 0;
                CurrentLyric = LyricLine.Empty;
                return;
            }

            if (position.CompareTo(Lyrics.Last().Time) >= 0)
            {
                NextIndex = 0;
                CurrentLyric = Lyrics.Last();
                return;
            }

            for (var i = 0; i < Lyrics.Count; i++)
            {
                if (position.CompareTo(Lyrics[i].Time) < 0)
                {
                    NextIndex = i;
                    CurrentLyric = Lyrics[i - 1];
                    break;
                }
            }
        }
    }
}