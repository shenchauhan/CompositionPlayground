using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CompositionDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Gears : Page, INotifyPropertyChanged
    {
        private Compositor _compositor;
        private List<Visual> _gearVisuals;
        private ExpressionAnimation _rotationExpression;
        private ScalarKeyFrameAnimation _gearMotionScalarAnimation;
        private double _x = 87, _y = 0d, _width = 100, _height = 100;
        private double _gearDimension = 87;
        private int _count;

        public event PropertyChangedEventHandler PropertyChanged;

        public Gears()
        {
            InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            };
        }

        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                RaisePropertyChanged();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                App.Current.DebugSettings.EnableFrameRateCounter = true;
            }

            _compositor = ElementCompositionPreview.GetElementVisual(this)?.Compositor;
            Setup();
        }

        private void Setup()
        {
            var firstGearVisual = ElementCompositionPreview.GetElementVisual(FirstGear);
            firstGearVisual.Size = new Vector2((float)FirstGear.ActualWidth, (float)FirstGear.ActualHeight);
            firstGearVisual.AnchorPoint = new Vector2(0.5f, 0.5f);

            for (int i = Container.Children.Count - 1; i > 0; i--)
            {
                Container.Children.RemoveAt(i);
            }

            _x = 87;
            _y = 0d;
            _width = 100;
            _height = 100;
            _gearDimension = 87;

            Count = 1;
            _gearVisuals = new List<Visual>() { firstGearVisual };
        }

        private void AddGear_Click(object sender, RoutedEventArgs e)
        {
            // Create an image
            var bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/Gear.png"));
            var image = new Image
            {
                Source = bitmapImage,
                Width = _width,
                Height = _height,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            // Set the coordinates of where the image should be
            Canvas.SetLeft(image, _x);
            Canvas.SetTop(image, _y);

            PerformLayoutCalculation();

            // Add the gear to the container
            Container.Children.Add(image);

            // Add a gear visual to the screen
            var gearVisual = AddGear(image);

            ConfigureGearAnimation(_gearVisuals[_gearVisuals.Count - 1], _gearVisuals[_gearVisuals.Count - 2]);
        }

        private Visual AddGear(Image gear)
        {
            // Create a visual based on the XAML object
            var visual = ElementCompositionPreview.GetElementVisual(gear);
            visual.Size = new Vector2((float)gear.ActualWidth, (float)gear.ActualHeight);
            visual.AnchorPoint = new Vector2(0.5f, 0.5f);
            _gearVisuals.Add(visual);

            Count++;

            return visual;
        }

        private void ConfigureGearAnimation(Visual currentGear, Visual previousGear)
        {
            // If rotation expression is null then create an expression of a gear rotating the opposite direction
            _rotationExpression = _rotationExpression ?? _compositor.CreateExpressionAnimation("-previousGear.RotationAngleInDegrees");

            // put in placeholder parameters
            _rotationExpression.SetReferenceParameter("previousGear", previousGear);

            // Start the animation based on the Rotation Angle in Degrees.
            currentGear.StartAnimation("RotationAngleInDegrees", _rotationExpression);
        }

        private void StartGearMotor(double secondsPerRotation)
        {
            // Start the first gear (the red one)
            if (_gearMotionScalarAnimation == null)
            {
                _gearMotionScalarAnimation = _compositor.CreateScalarKeyFrameAnimation();
                var linear = _compositor.CreateLinearEasingFunction();

                _gearMotionScalarAnimation.InsertExpressionKeyFrame(0.0f, "this.StartingValue");
                _gearMotionScalarAnimation.InsertExpressionKeyFrame(1.0f, "this.StartingValue + 360", linear);

                _gearMotionScalarAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            }

            _gearMotionScalarAnimation.Duration = TimeSpan.FromSeconds(secondsPerRotation);
            _gearVisuals.First().StartAnimation("RotationAngleInDegrees", _gearMotionScalarAnimation);
        }

        private void AnimateFast_Click(object sender, RoutedEventArgs e)
        {
            // Setup and start the animation on the red gear.
            StartGearMotor(1);
        }

        private void AnimateSlow_Click(object sender, RoutedEventArgs e)
        {
            // Setup and start the animation on the red gear.
            StartGearMotor(5);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _gearVisuals.First().StopAnimation("RotationAngleInDegrees");
        }

        private void Reverse_Click(object sender, RoutedEventArgs e)
        {
            if (_gearMotionScalarAnimation.Direction == Windows.UI.Composition.AnimationDirection.Normal)
            {
                _gearMotionScalarAnimation.Direction = Windows.UI.Composition.AnimationDirection.Reverse;
            }
            else
            {
                _gearMotionScalarAnimation.Direction = Windows.UI.Composition.AnimationDirection.Normal;
            }

            _gearVisuals.First().StartAnimation("RotationAngleInDegrees", _gearMotionScalarAnimation);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void AddXGearsButton_Click(object sender, RoutedEventArgs e)
        {
            int gearsToAdd;

            if (int.TryParse(NumberOfGears.Text, out gearsToAdd))
            {
                int amount = gearsToAdd + _gearVisuals.Count - 1;
                Setup();

                var maxAreaPerTile = Math.Sqrt((Container.ActualWidth * Container.ActualHeight) / (amount + Container.Children.Count));

                if (maxAreaPerTile < _width)
                {
                    var wholeTilesHeight = Math.Floor(Container.ActualHeight / maxAreaPerTile);
                    var wholeTileWidth = Math.Floor(Container.ActualWidth / maxAreaPerTile);

                    FirstGear.Width = FirstGear.Height = maxAreaPerTile;
                    _width = _height = maxAreaPerTile;

                    _x = _gearDimension = _width * 0.87;
                }

                for (int i = 0; i < amount; i++)
                {
                    AddGear_Click(sender, e);
                }
            }
        }

        private void PerformLayoutCalculation()
        {
            if (
                ((_x + Container.Margin.Left + _width > Container.ActualWidth) && _gearDimension > 0) ||
                (_x < Container.Margin.Left && _gearDimension < 0))
            {
                if (_gearDimension < 0)
                {
                    _y -= _gearDimension;
                }
                else
                {
                    _y += _gearDimension;
                }
                _gearDimension = -_gearDimension;
            }
            else
            {
                _x += _gearDimension;
            }
        }

        private void RaisePropertyChanged([CallerMemberName]string property = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
