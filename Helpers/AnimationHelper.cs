using Microsoft.Maui.Animations;

namespace PoligonMaui.Helpers;

public static class AnimationHelper
{
    public static async Task AnimateMarkerJump(View marker, uint duration = 800)
    {
        try
        {
            var jumpAnimation = new Animation();
            
            // Scale animation
            jumpAnimation.Add(0, 0.3, new Animation(v => marker.Scale = v, 1, 1.3, Easing.CubicOut));
            jumpAnimation.Add(0.3, 0.7, new Animation(v => marker.Scale = v, 1.3, 1.1, Easing.CubicIn));
            jumpAnimation.Add(0.7, 1, new Animation(v => marker.Scale = v, 1.1, 1, Easing.CubicOut));
            
            // Translation Y animation (jump effect)
            jumpAnimation.Add(0, 0.5, new Animation(v => marker.TranslationY = v, 0, -20, Easing.CubicOut));
            jumpAnimation.Add(0.5, 1, new Animation(v => marker.TranslationY = v, -20, 0, Easing.BounceOut));

            jumpAnimation.Commit(marker, "MarkerJump", 16, duration);
            
            await Task.Delay((int)duration);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Animation error: {ex.Message}");
        }
    }

    public static async Task AnimateColorChange(View view, Color fromColor, Color toColor, uint duration = 500)
    {
        try
        {
            var colorAnimation = new Animation(callback =>
            {
                var r = fromColor.Red + (toColor.Red - fromColor.Red) * callback;
                var g = fromColor.Green + (toColor.Green - fromColor.Green) * callback;
                var b = fromColor.Blue + (toColor.Blue - fromColor.Blue) * callback;
                var a = fromColor.Alpha + (toColor.Alpha - fromColor.Alpha) * callback;
                
                view.BackgroundColor = new Color((float)r, (float)g, (float)b, (float)a);
            }, 0, 1, Easing.Linear);

            colorAnimation.Commit(view, "ColorChange", 16, duration);
            await Task.Delay((int)duration);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Color animation error: {ex.Message}");
        }
    }

    public static async Task AnimateRouteArrow(View arrow, double distance, uint duration = 2000)
    {
        try
        {
            var arrowAnimation = new Animation(callback =>
            {
                arrow.TranslationX = distance * callback;
            }, 0, 1, Easing.Linear);

            arrowAnimation.Commit(arrow, "RouteArrow", 16, duration, repeat: () => true);
            await Task.Delay((int)duration);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Route arrow animation error: {ex.Message}");
        }
    }

    public static async Task PulseAnimation(View view, uint duration = 1000)
    {
        try
        {
            var pulseAnimation = new Animation();
            
            // Opacity pulse
            pulseAnimation.Add(0, 0.5, new Animation(v => view.Opacity = v, 1, 0.3, Easing.CubicInOut));
            pulseAnimation.Add(0.5, 1, new Animation(v => view.Opacity = v, 0.3, 1, Easing.CubicInOut));

            pulseAnimation.Commit(view, "Pulse", 16, duration, repeat: () => true);
            await Task.Delay((int)duration);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Pulse animation error: {ex.Message}");
        }
    }

    public static async Task FadeIn(View view, uint duration = 300)
    {
        try
        {
            view.Opacity = 0;
            view.IsVisible = true;
            await view.FadeTo(1, duration);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fade in error: {ex.Message}");
        }
    }

    public static async Task FadeOut(View view, uint duration = 300)
    {
        try
        {
            await view.FadeTo(0, duration);
            view.IsVisible = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Fade out error: {ex.Message}");
        }
    }

    public static void StopAllAnimations(View view)
    {
        try
        {
            view.AbortAnimation("MarkerJump");
            view.AbortAnimation("ColorChange");
            view.AbortAnimation("RouteArrow");
            view.AbortAnimation("Pulse");
            view.AbortAnimation("FadeIn");
            view.AbortAnimation("FadeOut");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Stop animations error: {ex.Message}");
        }
    }
}