using Godot;

namespace CoreUI
{
    public partial class SceneFlowManager : CanvasLayer
    {
        public static SceneFlowManager Instance { get; private set; }

        [Export] private float _fadeDuration = 0.3f;
        [Export] private Color _fadeColor = Colors.Black;

        private ColorRect _fadeOverlay;

        public override void _EnterTree()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;
            ProcessMode = ProcessModeEnum.Always; // Keep active while the tree is paused
        }

        public override void _Ready()
        {
            Layer = 100; // Draw above all gameplay and menus
            BuildOverlay();
        }

        private void BuildOverlay()
        {
            _fadeOverlay = new ColorRect
            {
                Color = _fadeColor,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            _fadeOverlay.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _fadeOverlay.Modulate = new Color(1, 1, 1, 0); // Fully transparent by default
            AddChild(_fadeOverlay);
        }

        public async void ChangeScene(string scenePath)
        {
            // Block mouse clicks during transitions
            _fadeOverlay.MouseFilter = Control.MouseFilterEnum.Stop;

            // Fade to black
            Tween fadeOut = CreateTween().SetPauseMode(Tween.TweenPauseMode.Process);
            fadeOut.TweenProperty(_fadeOverlay, "modulate:a", 1.0f, _fadeDuration);
            await ToSignal(fadeOut, Tween.SignalName.Finished);

            // Adhere to design rules: reset pause state and clear menus
            GetTree().Paused = false;
            MenuManager.Instance?.CloseAllMenus();

            // Load new scene
            Error err = GetTree().ChangeSceneToFile(scenePath);
            if (err != Error.Ok)
            {
                GD.PrintErr($"[SceneFlowManager] Failed to load scene at: {scenePath}");
            }

            // Wait a frame for scene initialization
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            // Fade back in
            Tween fadeIn = CreateTween().SetPauseMode(Tween.TweenPauseMode.Process);
            fadeIn.TweenProperty(_fadeOverlay, "modulate:a", 0.0f, _fadeDuration);
            await ToSignal(fadeIn, Tween.SignalName.Finished);

            _fadeOverlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        }

        public void RestartCurrentScene()
        {
            string currentScenePath = GetTree().CurrentScene?.SceneFilePath;
            if (!string.IsNullOrEmpty(currentScenePath))
            {
                ChangeScene(currentScenePath);
            }
        }

        public void QuitGame()
        {
            GetTree().Quit();
        }
    }
}