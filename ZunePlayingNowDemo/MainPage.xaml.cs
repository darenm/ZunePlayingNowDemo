using System;
using System.Numerics;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Effects;
using Robmikh.CompositionSurfaceFactory;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ZunePlayingNowDemo
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const float PointLightDistance = 300;

        #region Fields

        private readonly TimeSpan _animationDuration = TimeSpan.FromSeconds(10);
        private readonly DispatcherTimer _animationTimer = new DispatcherTimer();
        private readonly Random _positionRandom = new Random();
        private AmbientLight _ambientLight;
        private SpriteVisual _backgroundVisual;
        private ContainerVisual _containerVisual;
        private ImplicitAnimationCollection _implicitOffsetAnimation;
        private PointLight _pointLight1;
        private PointLight _pointLight2;
        private LoadedImageSurface _surface;
        private SurfaceFactory _surfaceFactory;
        private TextSurface _textSurface;
        private SpriteVisual _textVisual;

        #endregion

        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
            RootElement.SizeChanged += RootElementOnSizeChanged;

            //draw into the title bar
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            //remove the solid-colored backgrounds behind the caption controls and system back button
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            CreateOffsetAnimation();
        }

        private void AddLighting()
        {
            var compositor = Window.Current.Compositor;

            _ambientLight = compositor.CreateAmbientLight();
            _ambientLight.Intensity = 1.5f;
            _ambientLight.Color = Colors.Purple;
            _ambientLight.Targets.Add(_backgroundVisual);

            _pointLight1 = compositor.CreatePointLight();
            _pointLight1.Color = Colors.Yellow;
            _pointLight1.Intensity = 1f;
            _pointLight1.CoordinateSpace = _containerVisual;
            _pointLight1.Targets.Add(_backgroundVisual);
            _pointLight1.Offset = new Vector3((float) RootElement.ActualWidth, (float) RootElement.ActualHeight * 0.25f,
                PointLightDistance);

            _pointLight2 = compositor.CreatePointLight();
            _pointLight2.Color = Colors.Green;
            _pointLight2.Intensity = 2f;
            _pointLight2.CoordinateSpace = _containerVisual;
            _pointLight2.Targets.Add(_backgroundVisual);
            _pointLight2.Offset = new Vector3(0, (float) RootElement.ActualHeight * 0.75f, PointLightDistance);

            _pointLight1.ImplicitAnimations = _implicitOffsetAnimation;
            _pointLight2.ImplicitAnimations = _implicitOffsetAnimation;

            _pointLight1.Offset = new Vector3(0, (float) RootElement.ActualHeight * 0.25f, PointLightDistance);
            _pointLight2.Offset = new Vector3((float) RootElement.ActualWidth, (float) RootElement.ActualHeight * 0.75f,
                PointLightDistance);
        }

        private void CreateOffsetAnimation()
        {
            if (_implicitOffsetAnimation != null)
            {
                return;
            }

            var offsetAnimation = Window.Current.Compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = nameof(PointLight.Offset);
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = TimeSpan.FromMinutes(0.6);

            _implicitOffsetAnimation = Window.Current.Compositor.CreateImplicitAnimationCollection();
            _implicitOffsetAnimation[nameof(Visual.Offset)] = offsetAnimation;
        }

        private Vector3KeyFrameAnimation CreateTextOffsetAnimation(Vector3 centerVector3)
        {
            var offsetAnimation = Window.Current.Compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Duration = _animationDuration;
            offsetAnimation.InsertKeyFrame(0f, new Vector3(-2000, -2000, 0));
            //offsetAnimation.InsertKeyFrame(0.4f, centerVector3 - new Vector3(40, 40, 0));
            offsetAnimation.InsertKeyFrame(0.5f, centerVector3);
            //offsetAnimation.InsertKeyFrame(0.6f, centerVector3 + new Vector3(40, 40, 0));
            offsetAnimation.InsertKeyFrame(1f, new Vector3(2000, 2000, 0));
            offsetAnimation.Direction = AnimationDirection.Alternate;
            offsetAnimation.IterationBehavior = AnimationIterationBehavior.Forever;
            return offsetAnimation;
        }

        private Vector3 GenerateRandomPointInBounds(int width, int height)
        {
            const int margin = 48;
            return new Vector3(_positionRandom.Next(margin, width - margin),
                _positionRandom.Next(margin, width - margin), PointLightDistance);
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var compositor = Window.Current.Compositor;

            _surface = LoadedImageSurface.StartLoadFromUri(new Uri("ms-appx:///Assets/webster-pass.png"));

            var backgroundBrush = compositor.CreateSurfaceBrush(_surface);
            backgroundBrush.Stretch = CompositionStretch.UniformToFill;

            var saturationEffect = new SaturationEffect
            {
                Saturation = 0.0f,
                Source = new CompositionEffectSourceParameter("mySource")
            };

            var saturationEffectFactory = compositor.CreateEffectFactory(saturationEffect);

            var bwEffect = saturationEffectFactory.CreateBrush();
            bwEffect.SetSourceParameter("mySource", backgroundBrush);

            _backgroundVisual = compositor.CreateSpriteVisual();
            _backgroundVisual.Brush = bwEffect;
            _backgroundVisual.Size = RootElement.RenderSize.ToVector2();


            _containerVisual = compositor.CreateContainerVisual();
            _containerVisual.Children.InsertAtBottom(_backgroundVisual);
            ElementCompositionPreview.SetElementChildVisual(RootElement, _containerVisual);

            // Text
            _surfaceFactory = SurfaceFactory.GetSharedSurfaceFactoryForCompositor(compositor);

            _textSurface = _surfaceFactory.CreateTextSurface("Weston Pass");
            _textSurface.ForegroundColor = Color.FromArgb(50, 255, 255, 255);
            _textSurface.FontSize = 150;
            var textSurfaceBrush = compositor.CreateSurfaceBrush(_textSurface.Surface);


            _textVisual = compositor.CreateSpriteVisual();
            _textVisual.Size = _textSurface.Size.ToVector2();
            _textVisual.RotationAngleInDegrees = 45f;
            _textVisual.AnchorPoint = new Vector2(0.5f);

            _textVisual.Brush = textSurfaceBrush;
            _textVisual.StartAnimation(nameof(Visual.Offset), CreateTextOffsetAnimation(
                new Vector3((float) RootElement.ActualWidth / 2, (float) RootElement.ActualWidth / 2, 0)));

            _containerVisual.Children.InsertAtTop(_textVisual);

            AddLighting();

            StartLightingAnimationTimer();
        }

        private void MoveLights()
        {
            _pointLight1.Offset =
                GenerateRandomPointInBounds((int) RootElement.ActualWidth, (int) RootElement.ActualHeight);
            _pointLight2.Offset =
                GenerateRandomPointInBounds((int) RootElement.ActualWidth, (int) RootElement.ActualHeight);
        }

        private void RootElementOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (_backgroundVisual != null)
            {
                _backgroundVisual.Size = new Vector2((float) RootElement.ActualWidth, (float) RootElement.ActualHeight);
                _textVisual.StartAnimation(nameof(Visual.Offset), CreateTextOffsetAnimation(
                    new Vector3((float)RootElement.ActualWidth / 2, (float)RootElement.ActualWidth / 2, 0)));

                MoveLights();
            }
        }

        private void StartLightingAnimationTimer()
        {
            // Setup recuring timer to move the lights
            _animationTimer.Interval = _animationDuration;
            _animationTimer.Tick += (s, a) => MoveLights();
            _animationTimer.Start();
        }
    }
}