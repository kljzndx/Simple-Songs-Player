using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SimpleSongsPlayer.Models.DTO.Lyric;

namespace SimpleSongsPlayer.Views.Controllers.Abstracts
{
    public abstract class LyricsPreviewControlBase : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(IList<LyricLine>), typeof(LyricsPreviewControlBase), new PropertyMetadata(null));

        public static readonly DependencyProperty CurrentLyricProperty = DependencyProperty.Register(
            nameof(CurrentLyric), typeof(LyricLine), typeof(LyricsPreviewControlBase),
            new PropertyMetadata(LyricLine.Empty));

        protected LyricLine BackLyric;
        protected int NextIndex;

        public IList<LyricLine> Source
        {
            get => (IList<LyricLine>) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public LyricLine CurrentLyric
        {
            get => (LyricLine) GetValue(CurrentLyricProperty);
            set
            {
                BackLyric = GetValue(CurrentLyricProperty) as LyricLine;
                SetValue(CurrentLyricProperty, value);
                Refreshed?.Invoke(this, value);
            }
        }

        public event TypedEventHandler<LyricsPreviewControlBase, LyricLine> Refreshed;

        protected bool CanPreview => IsEnabled && Visibility == Visibility.Visible && Source != null && Source.Any();

        public void RefreshLyric(TimeSpan position)
        {
            if (!CanPreview)
                return;

            if (NextIndex >= Source.Count)
                NextIndex = 0;

            long currentPositionTicks = position.Ticks;
            LyricLine nextLyric = Source[NextIndex];
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

            if (position.CompareTo(Source.First().Time) <= 0)
            {
                NextIndex = 0;
                CurrentLyric = LyricLine.Empty;
                return;
            }

            if (position.CompareTo(Source.Last().Time) >= 0)
            {
                NextIndex = 0;
                CurrentLyric = Source.Last();
                return;
            }

            for (var i = 0; i < Source.Count; i++)
            {
                if (position.CompareTo(Source[i].Time) < 0)
                {
                    NextIndex = i;
                    CurrentLyric = Source[i - 1];
                    break;
                }
            }
        }
    }
}