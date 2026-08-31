using Godot;

public partial class IntroSaloon : Control
{
    private const string NextScenePath = "res://StorySaloon.tscn";
    private const string EnterFramePath = "res://assets/intro_saloon_enter.png";
    private const string CleanFramePath = "res://assets/intro_saloon_clean.png";
    private const string DoorsPath = "res://assets/intro_saloon_doors_closed.png";
    private const double EntranceDuration = 2.0;
    private const string HalfTextureShader = """
        shader_type canvas_item;
        uniform float keep_right = 0.0;

        float edge(float value, float limit, float direction) {
            return smoothstep(-0.002, 0.002, (value - limit) * direction);
        }

        void fragment() {
            vec4 pixel = texture(TEXTURE, UV);
            float outer = keep_right < 0.5 ? 0.342 : 0.660;
            float inner = keep_right < 0.5 ? 0.500 : 0.500;
            float across = clamp((UV.x - outer) / (inner - outer), 0.0, 1.0);
            float top = mix(0.319, 0.337, across);
            float bottom = 0.807;
            float horizontal = keep_right < 0.5
                ? edge(UV.x, outer, 1.0) * edge(UV.x, inner, -1.0)
                : edge(UV.x, inner, 1.0) * edge(UV.x, outer, -1.0);
            float mask = horizontal * edge(UV.y, top, 1.0) * edge(UV.y, bottom, -1.0);
            COLOR = vec4(pixel.rgb, pixel.a * mask);
        }
        """;

    private TextureRect entrance = null!;
    private TextureRect leftDoor = null!;
    private TextureRect rightDoor = null!;
    private StoryTextOverlay storyText = null!;
    private double elapsed;
    private bool doorsActive;

    public override void _Ready()
    {
        AddChild(FullRectTexture(CleanFramePath));
        leftDoor = CreateDoor(false, new Vector2(395, 330));
        rightDoor = CreateDoor(true, new Vector2(760, 330));
        AddChild(leftDoor);
        AddChild(rightDoor);

        entrance = FullRectTexture(EnterFramePath);
        AddChild(entrance);

        storyText = new StoryTextOverlay
        {
            Segments = StoryTextOverlay.LoadSegments("res://texts/01-story-enter-saloon.md", "картина", "затем"),
            Finished = () => GetTree().ChangeSceneToFile(NextScenePath)
        };
        AddChild(storyText);
    }

    public override void _Process(double delta)
    {
        if (storyText.ShowingBlack)
        {
            return;
        }

        elapsed += delta;
        if (!doorsActive && elapsed >= EntranceDuration)
        {
            doorsActive = true;
            leftDoor.Visible = true;
            rightDoor.Visible = true;
            var fade = CreateTween();
            fade.TweenProperty(entrance, "modulate:a", 0f, 0.3);
            fade.TweenCallback(Callable.From(entrance.QueueFree));
        }

        if (!doorsActive)
        {
            return;
        }

        var time = (float)(elapsed - EntranceDuration);
        var initialAngle = Mathf.DegToRad(87.5f);
        var damping = Mathf.Exp(-time * 0.38f);
        var leftAngle = initialAngle * damping * Mathf.Cos(time * 3.10f);
        var rightAngle = initialAngle * damping * Mathf.Cos(time * 3.18f);
        leftDoor.Scale = new Vector2(Mathf.Cos(leftAngle), 1f);
        rightDoor.Scale = new Vector2(Mathf.Cos(rightAngle), 1f);

        var leftLight = 0.92f - 0.08f * Mathf.Sin(leftAngle);
        var rightLight = 0.92f - 0.08f * Mathf.Sin(rightAngle);
        leftDoor.Modulate = new Color(leftLight, leftLight, leftLight);
        rightDoor.Modulate = new Color(rightLight, rightLight, rightLight);
    }

    public override void _Input(InputEvent @event)
    {
        if (storyText.Active)
        {
            return;
        }

        if (!doorsActive)
        {
            return;
        }

        var activated =
            @event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } ||
            @event is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Enter or Key.KpEnter or Key.Space };

        if (!activated)
        {
            return;
        }

        GetViewport().SetInputAsHandled();
        GetTree().ChangeSceneToFile(NextScenePath);
    }

    private static TextureRect CreateDoor(bool right, Vector2 pivot)
    {
        var material = new ShaderMaterial { Shader = new Shader { Code = HalfTextureShader } };
        material.SetShaderParameter("keep_right", right ? 1f : 0f);
        var door = FullRectTexture(DoorsPath);
        door.Material = material;
        door.PivotOffset = pivot;
        door.Visible = false;
        return door;
    }

    private static TextureRect FullRectTexture(string texturePath)
    {
        return new TextureRect
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            Texture = GD.Load<Texture2D>(texturePath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            MouseFilter = MouseFilterEnum.Ignore
        };
    }
}
