using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Composition.Interactions;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CompositionDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Blur : Page
    {
        private Compositor _compositor;
        private Visual _visual;
        private InteractionTracker _interactionTracker;
        private VisualInteractionSource _interactionSource;

        public Blur()
        {
            this.InitializeComponent();

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                if (Frame.CanGoBack)
                {
                    Frame.GoBack();
                }
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _compositor = ElementCompositionPreview.GetElementVisual(this)?.Compositor;
            _visual = ElementCompositionPreview.GetElementVisual(Image);

            Interaction();

            PointerPressed += Blur_PointerPressed;
        }

        private void Blur_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                _interactionSource.TryRedirectForManipulation(e.GetCurrentPoint(Window.Current.Content));
            }
        }

        private void Interaction()
        {
            _interactionTracker = InteractionTracker.Create(_compositor);

            _interactionTracker.MinScale = 0;
            _interactionTracker.MaxScale = 100f;

            _interactionTracker.MaxPosition = new Vector3((float)Window.Current.Bounds.Width * 0.5f, (float)Window.Current.Bounds.Height * 0.5f, 0f);
            _interactionTracker.MinPosition = new Vector3();

            _interactionSource = VisualInteractionSource.Create(ElementCompositionPreview.GetElementVisual(Window.Current.Content));
            _interactionSource.PositionXSourceMode = InteractionSourceMode.EnabledWithInertia;
            _interactionSource.ScaleSourceMode = InteractionSourceMode.EnabledWithInertia;

            _interactionTracker.InteractionSources.Add(_interactionSource);

            var blurEffect = new GaussianBlurEffect
            {
                BlurAmount = 0f,
                Optimization = EffectOptimization.Speed,
                Name = "blurEffect",
                Source = new CompositionEffectSourceParameter("image")
            };

            var effectFactory = _compositor.CreateEffectFactory(blurEffect, new[] { "blurEffect.BlurAmount" });

            var blurBrush = effectFactory.CreateBrush();
            blurBrush.SetSourceParameter("image", _compositor.CreateBackdropBrush());

            var sprite = _compositor.CreateSpriteVisual();
            sprite.Size = new Vector2((float)Window.Current.Bounds.Width, (float)Window.Current.Bounds.Height);
            sprite.Brush = blurBrush;

            var blurAnimation = _compositor.CreateExpressionAnimation("lerp(tracker.MinScale, tracker.MaxScale, clamp(tracker.Position.X / width, 0, 1))");
            blurAnimation.SetReferenceParameter("tracker", _interactionTracker);
            blurAnimation.SetScalarParameter("width", (float)Window.Current.Bounds.Width);

            sprite.Brush.Properties.StartAnimation("blurEffect.BlurAmount", blurAnimation);

            ElementCompositionPreview.SetElementChildVisual(Image, sprite);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
