extends Control

const NEXT_SCENE_PATH := "res://scenes/gameplay/SurgeryLayer.tscn"

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
	GameState.reset_run()
	scene_image_label.text = "F1 // OPERATING FLOOR"
	scene_image_note.text = "The dossier is sealed. The theater is live. Freeze 1 hands directly into the Surgery Layer."
	overline.text = "FIELD FLOW // F1"
	title_label.text = "Field One"
	beat_line.text = "INT. PREP BAY - CONTINUOUS"
	body_copy.text = "[i]Octaviy steps through the first threshold.[/i]\n\nThis MVP flow skips Intake Desk entirely. F1 exists only to establish the first gameplay state and hand off to the Surgery Layer allocation scene."
	cue_card_text.text = "Continue into Freeze 1 pressure routing."
	scratch_notes.text = "Minimal path: TitleScreen -> F1 -> SurgeryLayer -> FinalScreen."
	primary_action_button.text = "Continue to Surgery Layer"
	primary_action_button.pressed.connect(_go_to_f2)
	secondary_action_button.hide()
	tertiary_action_button.hide()
	primary_action_button.grab_focus()


func _go_to_f2() -> void:
	get_tree().change_scene_to_file(NEXT_SCENE_PATH)
