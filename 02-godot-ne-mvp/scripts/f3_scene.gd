extends Control

const TITLE_SCENE_PATH := "res://scenes/start/TitleScreen.tscn"
const REPLAY_SCENE_PATH := "res://scenes/gameplay/F1.tscn"

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
	var badge_summary := ", ".join(GameState.badge_ids)
	if badge_summary.is_empty():
		badge_summary = "none"
	var allocation_summary := "Scene %d / Victoria %d / Desmond %d" % [
		int(GameState.surgery_allocation.get("scene", 0)),
		int(GameState.surgery_allocation.get("victoria", 0)),
		int(GameState.surgery_allocation.get("desmond", 0)),
	]

	scene_image_label.text = "F3 // FINAL OUTCOME"
	scene_image_note.text = "The run resolves here using the exact values stored in GameState."
	overline.text = "FIELD FLOW // F3"
	title_label.text = "Outcome Report"
	beat_line.text = "INT. ARCHIVE CHAMBER - POST-OP"
	body_copy.text = "[i]Run locked.[/i]\n\nAllocation: %s\nResolved outcome: %s\nEnding: %s\nDossier variant: %s\nBadges: %s" % [
		allocation_summary,
		_value_or_placeholder(GameState.resolved_outcome),
		_value_or_placeholder(GameState.ending_id),
		_value_or_placeholder(GameState.dossier_variant),
		badge_summary,
	]
	cue_card_text.text = "This is the concrete end of the MVP path."
	scratch_notes.text = "Restart returns to TitleScreen. Replay jumps back to F1."
	primary_action_button.text = "Return to Title"
	secondary_action_button.text = "Replay F1"
	tertiary_action_button.hide()
	primary_action_button.pressed.connect(_return_to_title)
	secondary_action_button.pressed.connect(_replay_flow)
	primary_action_button.grab_focus()


func _return_to_title() -> void:
	get_tree().change_scene_to_file(TITLE_SCENE_PATH)


func _replay_flow() -> void:
	get_tree().change_scene_to_file(REPLAY_SCENE_PATH)


func _value_or_placeholder(value: String) -> String:
	return value if not value.is_empty() else "unset"
