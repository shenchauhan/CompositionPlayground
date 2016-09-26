using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Effects;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Composition.Effects;
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
    public sealed partial class Lighting : Page
    {
        private Compositor _compositor;
        private Visual _visual;
        private CompositionEffectFactory _effectFactory;
        private PointLight _pointLight;

        public Lighting()
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
            _pointLight = _compositor.CreatePointLight();
            _pointLight.Color = Colors.White;

            var graphicsEffect = new CompositeEffect
            {
                Mode = CanvasComposite.Add,
                Sources =
                            {
                                new CompositionEffectSourceParameter("ImageSource"),
                                new SceneLightingEffect()
                                {
                                    AmbientAmount = 0,
                                    DiffuseAmount = .75f,
                                    SpecularAmount = 0,
                                }
                            }
            };

            _effectFactory = _compositor.CreateEffectFactory(graphicsEffect);

            var lightRootVisual = ElementCompositionPreview.GetElementVisual(this);
            _pointLight.CoordinateSpace = lightRootVisual;
            _pointLight.Targets.Add(lightRootVisual);

            var brush = _effectFactory.CreateBrush();

            PointerMoved += Lighting_PointerMoved;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            PointerMoved -= Lighting_PointerMoved;
        }

        private void Lighting_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var offset = e.GetCurrentPoint(this).Position.ToVector2();
            _pointLight.Offset = new Vector3(offset.X, offset.Y, 75);
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
