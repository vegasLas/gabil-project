using PoligonMaui.Helpers;
using PoligonMaui.Models;

namespace PoligonMaui.Controls;

public class AnimatedMarker : ContentView
{
    private readonly Ellipse _markerEllipse;
    private readonly Label _markerLabel;
    private Timer? _animationTimer;
    private bool _isAnimating = false;

    public static readonly BindableProperty TargetProperty = BindableProperty.Create(
        nameof(Target), typeof(Target), typeof(AnimatedMarker),
        propertyChanged: OnTargetChanged);

    public static readonly BindableProperty IsNearestProperty = BindableProperty.Create(
        nameof(IsNearest), typeof(bool), typeof(AnimatedMarker), false,
        propertyChanged: OnIsNearestChanged);

    public static readonly BindableProperty MarkerSizeProperty = BindableProperty.Create(
        nameof(MarkerSize), typeof(double), typeof(AnimatedMarker), 20.0);

    public Target? Target
    {
        get => (Target?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public bool IsNearest
    {
        get => (bool)GetValue(IsNearestProperty);
        set => SetValue(IsNearestProperty, value);
    }

    public double MarkerSize
    {
        get => (double)GetValue(MarkerSizeProperty);
        set => SetValue(MarkerSizeProperty, value);
    }

    public AnimatedMarker()
    {
        _markerEllipse = new Ellipse
        {
            WidthRequest = MarkerSize,
            HeightRequest = MarkerSize,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Stroke = Colors.Black,
            StrokeThickness = 2
        };

        _markerLabel = new Label
        {
            FontSize = 10,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };

        Content = new Grid
        {
            Children = { _markerEllipse, _markerLabel }
        };

        UpdateMarkerAppearance();
    }

    private void UpdateMarkerAppearance()
    {
        if (Target == null) return;

        try
        {
            // Update size
            _markerEllipse.WidthRequest = MarkerSize;
            _markerEllipse.HeightRequest = MarkerSize;

            // Update color based on target state
            var color = Target.IsReached ? Colors.Green :
                       IsNearest ? Colors.Yellow :
                       Color.FromArgb(Target.Color);

            _markerEllipse.Fill = color;

            // Update label
            _markerLabel.Text = !string.IsNullOrEmpty(Target.Name) ? 
                Target.Name.Length > 3 ? Target.Name.Substring(0, 3) : Target.Name :
                Target.Id.ToString();

            // Start/stop animation based on nearest status
            if (IsNearest && !Target.IsReached)
            {
                StartJumpAnimation();
            }
            else
            {
                StopJumpAnimation();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating marker appearance: {ex.Message}");
        }
    }

    private async void StartJumpAnimation()
    {
        if (_isAnimating) return;

        _isAnimating = true;

        try
        {
            _animationTimer = new Timer(async _ =>
            {
                if (_isAnimating && IsNearest && Target != null && !Target.IsReached)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await AnimationHelper.AnimateMarkerJump(this);
                    });
                }
            }, null, 0, 2000); // Jump every 2 seconds
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error starting jump animation: {ex.Message}");
        }
    }

    private void StopJumpAnimation()
    {
        _isAnimating = false;
        
        try
        {
            _animationTimer?.Dispose();
            _animationTimer = null;

            // Reset any transform that might be active
            AnimationHelper.StopAllAnimations(this);
            
            Scale = 1.0;
            TranslationY = 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping jump animation: {ex.Message}");
        }
    }

    private async void AnimateColorChange(Color newColor)
    {
        try
        {
            var currentColor = ((SolidColorBrush)_markerEllipse.Fill).Color;
            await AnimationHelper.AnimateColorChange(_markerEllipse, currentColor, newColor);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error animating color change: {ex.Message}");
        }
    }

    public async Task AnimateTargetReached()
    {
        try
        {
            StopJumpAnimation();
            
            // Animate color change to green
            await AnimationHelper.AnimateColorChange(_markerEllipse, Colors.Yellow, Colors.Green);
            
            // Scale animation to emphasize the reached state
            await this.ScaleTo(1.3, 250);
            await this.ScaleTo(1.0, 250);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error animating target reached: {ex.Message}");
        }
    }

    public void UpdateFromTarget(Target target)
    {
        Target = target;
        UpdateMarkerAppearance();
    }

    private static void OnTargetChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AnimatedMarker marker)
        {
            marker.UpdateMarkerAppearance();
        }
    }

    private static void OnIsNearestChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AnimatedMarker marker)
        {
            marker.UpdateMarkerAppearance();
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        UpdateMarkerAppearance();
    }

    ~AnimatedMarker()
    {
        StopJumpAnimation();
    }
}