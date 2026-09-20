using System;

public static class UIEventBus
{
    // Menu Navigation Events
    public static event Action<string> OnMenuOpenRequested;
    public static event Action OnMenuBackRequested;
    public static event Action OnCloseAllMenusRequested;

    // Audio Hooks
    public enum UISound { Hover, Click, Cancel, Error, Open, Close }
    public static event Action<UISound> OnPlayUISound;

    // Helper methods to raise events safely
    public static void RequestOpenMenu(string menuId) => OnMenuOpenRequested?.Invoke(menuId);
    public static void RequestBack() => OnMenuBackRequested?.Invoke();
    public static void RequestCloseAll() => OnCloseAllMenusRequested?.Invoke();
    public static void PlaySound(UISound sound) => OnPlayUISound?.Invoke(sound);
}
