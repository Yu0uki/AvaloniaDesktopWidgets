using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using WorldClockPlugin.ViewModels;
using System;

namespace WorldClockPlugin
{
    public partial class WorldClockView : UserControl
    {
        private WorldClockViewModel? _viewModel;
        private Line? _hourHand, _minuteHand, _secondHand;
        private bool _handsDrawn;

        public WorldClockView()
        {
            InitializeComponent();
            _viewModel = DataContext as WorldClockViewModel;
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName is nameof(WorldClockViewModel.HourAngle)
                        or nameof(WorldClockViewModel.MinuteAngle)
                        or nameof(WorldClockViewModel.SecondAngle))
                    {
                        // 定时器在后台线程，需要调度到 UI 线程绘制
                        Dispatcher.UIThread.Post(DrawClockHands, DispatcherPriority.Render);
                    }
                };
                // 首次绘制延迟到布局完成
                Dispatcher.UIThread.Post(DrawClockHands, DispatcherPriority.Loaded);
            }
        }

        private void DrawClockHands()
        {
            if (_viewModel == null || ClockCanvas == null) return;

            var cx = 120.0; var cy = 120.0;

            DrawHand(ref _hourHand, cx, cy, 55, _viewModel.HourAngle, Colors.White, 3);
            DrawHand(ref _minuteHand, cx, cy, 75, _viewModel.MinuteAngle, Color.FromRgb(220, 220, 220), 2);
            DrawHand(ref _secondHand, cx, cy, 80, _viewModel.SecondAngle, Color.FromRgb(255, 107, 53), 1);
        }

        private void DrawHand(ref Line? hand, double cx, double cy, double length, double angleDeg, Color color, double thickness)
        {
            var angleRad = (angleDeg - 90) * Math.PI / 180.0;
            var ex = cx + length * Math.Cos(angleRad);
            var ey = cy + length * Math.Sin(angleRad);

            if (hand == null)
            {
                hand = new Line
                {
                    Stroke = new SolidColorBrush(color),
                    StrokeThickness = thickness,
                    StrokeLineCap = PenLineCap.Round,
                    StartPoint = new Avalonia.Point(cx, cy),
                    ZIndex = thickness < 2 ? 3 : thickness < 3 ? 2 : 1
                };
                ClockCanvas.Children.Add(hand);
            }

            hand.EndPoint = new Avalonia.Point(ex, ey);
        }

        public void Dispose()
        {
            _viewModel?.Dispose();
        }
    }
}
