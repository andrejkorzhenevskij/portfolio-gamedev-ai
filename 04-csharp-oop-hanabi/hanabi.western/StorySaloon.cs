using Godot;

public partial class StorySaloon : Control
{
    private const string MainScenePath = "res://Main.tscn";
    private const string FramePath = "res://assets/story_saloon_map.png";
    private const string ScarfShader = """
        shader_type canvas_item;

        float red_mask(vec3 color) {
            return smoothstep(0.025, 0.11, color.r - max(color.g, color.b));
        }

        void fragment() {
            vec4 base = texture(TEXTURE, UV);
            float upper = smoothstep(0.61, 0.67, UV.x)
                * (1.0 - smoothstep(0.80, 0.84, UV.x))
                * smoothstep(0.21, 0.24, UV.y)
                * (1.0 - smoothstep(0.34, 0.37, UV.y));
            float lower_center = UV.x - 0.22;
            float lower = smoothstep(0.56, 0.61, UV.x)
                * (1.0 - smoothstep(0.68, 0.71, UV.x))
                * (1.0 - smoothstep(0.022, 0.045, abs(UV.y - lower_center)));
            float loose_end = max(upper, lower * 0.72);
            vec2 offset = vec2(
                sin(TIME * 1.65) * 0.0115,
                sin(TIME * 2.15 + UV.x * 19.0) * 0.0065
            ) * loose_end;
            vec4 moved = texture(TEXTURE, UV - offset);
            float scarf = max(red_mask(base.rgb), red_mask(moved.rgb)) * loose_end;
            COLOR = mix(base, moved, scarf);
        }
        """;

    private StoryTextOverlay storyText = null!;

    public override void _Ready()
    {
        var material = new ShaderMaterial { Shader = new Shader { Code = ScarfShader } };
        AddChild(new TextureRect
        {
            LayoutMode = 1,
            AnchorsPreset = (int)LayoutPreset.FullRect,
            AnchorRight = 1,
            AnchorBottom = 1,
            GrowHorizontal = GrowDirection.Both,
            GrowVertical = GrowDirection.Both,
            Texture = GD.Load<Texture2D>(FramePath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            MouseFilter = MouseFilterEnum.Ignore,
            Material = material
        });

        storyText = new StoryTextOverlay
        {
            Segments = StoryTextOverlay.LoadSegments("res://texts/02-story-the-deal.md", "выход", "затем"),
            Finished = () => GetTree().ChangeSceneToFile(MainScenePath)
        };
        AddChild(storyText);
    }

    public override void _Process(double delta)
    {
    }

    public override void _Input(InputEvent @event)
    {
        if (storyText.Active)
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
        GetTree().ChangeSceneToFile(MainScenePath);
    }
}
