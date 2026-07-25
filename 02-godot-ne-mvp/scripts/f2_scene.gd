extends Control

const NEXT_SCENE_PATH := "res://scenes/gameplay/FinalScreen.tscn"

@onready var scene_image_label: Label = $Margin/MainRow/SceneFrame/FrameMargin/FrameCanvas/SceneImageArea/SceneImageLabel
@onready var scene_image_note: Label = $Margin/MainRow/SceneFrame/FrameMargin/FrameCanvas/SceneImageArea/SceneImageNote
@onready var overline: Label = $Margin/MainRow/ScriptColumn/ScriptMargin/ScriptStack/Overline
@onready var title_label: Label = $Margin/MainRow/ScriptColumn/ScriptMargin/ScriptStack/Title
@onready var beat_line: Label = $Margin/MainRow/ScriptColumn/ScriptMargin/ScriptStack/BeatLine
@onready var body_copy: RichTextLabel = $Margin/MainRow/ScriptColumn/ScriptMargin/ScriptStack/BodyCopy
@onready var cue_card_text: Label = $Margin/MainRow/ScriptColumn/ScriptMargin/ScriptStack/CueCard/CueCardMargin/CueCardText
@onready var scratch_notes: Label = %ScratchNotes
@onready var primary_action_button: Button = %PrimaryActionButton
@onready var secondary_action_button: Button = %SecondaryActionButton
@onready var tertiary_action_button: Button = %TertiaryActionButton


func _ready() -> void:
	scene_image_label.text = "F2 // OUTCOME SELECTION"
	scene_image_note.text = "Three concrete branches resolve the run. Pick one and lock the result into GameState."
	overline.text = "FIELD FLOW // F2"
	title_label.text = "Outcome Gate"
	beat_line.text = "INT. SURGERY THEATER - DECISION POINT"
	body_copy.text = "[i]This scene is intentionally literal.[/i]\n\nChoose one hard-coded branch. Each button writes the corresponding outcome into GameState and advances to the final scene."
	cue_card_text.text = "No generic branching system here. These three buttons are the flow."
	scratch_notes.text = "Branches: scene -> 12A, victoria -> 12B, desmond -> 12C."
	primary_action_button.text = "Resolve Scene"
	secondary_action_button.text = "Resolve Victoria"
	tertiary_action_button.text = "Resolve Desmond"
	primary_action_button.pressed.connect(_resolve_and_continue.bind("scene"))
	secondary_action_button.pressed.connect(_resolve_and_continue.bind("victoria"))
	tertiary_action_button.pressed.connect(_resolve_and_continue.bind("desmond"))
	primary_action_button.grab_focus()


func _resolve_and_continue(zone: String) -> void:
	GameState.set_dominant_zone(zone)
	GameState.resolve_f2_outcome()
	get_tree().change_scene_to_file(NEXT_SCENE_PATH)
