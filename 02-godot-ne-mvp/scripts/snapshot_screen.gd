extends Control

const GAMEPLAY_SCENE_PATH := "res://scenes/gameplay/GameplayScreen.tscn"
const SNAPSHOT_NOTES_PATH := "res://narrative/snapshot_notes.tsv"
const DEFAULT_SNAPSHOT_NOTE := "Срез сохранён. Материал пока не решил, чем хочет быть, но уже оставил след."
const REVIEW_STATUS_COPY := "РАБОЧАЯ ПОМЕТКА"
const REVIEW_SUBNOTE_COPY := "Связка отмечена для финального разбора."
const EXPLANATION_COPY := "Служебная памятка: экран фиксирует, какой акцент материал вынес вперёд перед следующим фрагментом."
const METRICS_NOTE_TITLE_COPY := "КАК ЧИТАТЬ ПРОГОН"
const METRICS_NOTE_BODY_COPY := "Событие уже снято. Вы меняете не факт, а акцент:\nчто проявится, что сгладится и кто останется заметен.\n\nГлубина делает фильм сильнее.\nДавление может его пережечь.\nЗабвение может его стереть.\n\nПараметры героев — СБ (самобытность) и СЛ (след),\nвлияют на то, что именно о них станет известно\nи под каким углом это будет прочитано."
const CAST_CHARACTER_ORDER := ["desmond", "victoria", "leonard"]
const CAST_CHARACTER_NAMES := {
	"desmond": "Дезмонд",
	"victoria": "Виктория",
	"leonard": "Леонард",
}

@onready var overline: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/HeaderBlock/Overline
@onready var title_label: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/Title
@onready var subtitle_label: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/HeaderBlock/Subtitle
@onready var footer_note: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/FooterNote
@onready var resume_button: Button = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/SnapshotRow/LeftColumn/ContinueButtonRow/ResumeButton
@onready var explanation_bubble_tail: ColorRect = $ExplanationBubbleTail
@onready var explanation_bubble: PanelContainer = $ExplanationBubble
@onready var explanation_bubble_label: Label = $ExplanationBubble/BubbleMargin/ExplanationBubbleLabel

@onready var slot_a_headshot: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/SnapshotRow/LeftColumn/SnapshotStrip/StripMargin/StripColumn/PortraitRow/DossierSlotA/Margin/Stack/Headshot/HeadshotLabel
@onready var slot_a_title: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/SnapshotRow/LeftColumn/SnapshotStrip/StripMargin/StripColumn/PortraitRow/DossierSlotA/Margin/Stack/Title
@onready var slot_a_cue: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/SnapshotRow/LeftColumn/SnapshotStrip/StripMargin/StripColumn/PortraitRow/DossierSlotA/Margin/Stack/Cue
@onready var slot_b_headshot: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/SnapshotRow/LeftColumn/SnapshotStrip/StripMargin/StripColumn/PortraitRow/DossierSlotB/Margin/Stack/Headshot/HeadshotLabel
@onready var slot_b_title: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/SnapshotRow/LeftColumn/SnapshotStrip/StripMargin/StripColumn/PortraitRow/DossierSlotB/Margin/Stack/Title
@onready var slot_b_cue: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/SnapshotRow/LeftColumn/SnapshotStrip/StripMargin/StripColumn/PortraitRow/DossierSlotB/Margin/Stack/Cue
@onready var slot_c_headshot: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/SnapshotRow/LeftColumn/SnapshotStrip/StripMargin/StripColumn/PortraitRow/DossierSlotC/Margin/Stack/Headshot/HeadshotLabel
@onready var slot_c_title: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/SnapshotRow/LeftColumn/SnapshotStrip/StripMargin/StripColumn/PortraitRow/DossierSlotC/Margin/Stack/Title
@onready var slot_c_cue: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/SnapshotRow/LeftColumn/SnapshotStrip/StripMargin/StripColumn/PortraitRow/DossierSlotC/Margin/Stack/Cue
@onready var metrics_note_panel: PanelContainer = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/SnapshotRow/MetricsNotePanel
@onready var metrics_note_title: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/SnapshotRow/MetricsNotePanel/NoteMargin/NoteColumn/MetricsNoteTitle
@onready var metrics_note_body: Label = $Margin/CenterWrap/CenterFrame/FrameMargin/RootColumn/SnapshotRow/MetricsNotePanel/NoteMargin/NoteColumn/MetricsNoteBody

var snapshot_notes := {}


func _ready() -> void:
	snapshot_notes = _load_snapshot_notes()
	resume_button.pressed.connect(_resume_playback)
	resume_button.grab_focus()
	_style_metrics_readout()
	_apply_snapshot_copy()


func _apply_snapshot_copy() -> void:
	var snapshot_context := _get_snapshot_context()
	var stage := _resolve_snapshot_stage(snapshot_context)

	overline.text = "ПРОДЮСЕРСКИЙ ПРОСМОТР // %s" % stage
	subtitle_label.text = REVIEW_STATUS_COPY
	title_label.text = _resolve_snapshot_note(snapshot_context)
	footer_note.text = REVIEW_SUBNOTE_COPY
	_apply_explanation_bubble(bool(snapshot_context.get("show_explanatory_overlay", false)))
	_apply_metrics_note(stage)

	if not has_node("/root/GameState"):
		slot_a_headshot.text = "ФИЛЬМ"
		slot_a_title.text = "Глубина (неизвестно)"
		slot_a_cue.text = "Забвение (неизвестно)\nДавление (неизвестно)"
		slot_b_headshot.text = "АКЦЕНТ"
		slot_b_title.text = _format_snapshot_fragment_label(stage)
		slot_b_cue.text = "Сила влияния — (неизвестно)"
		slot_c_headshot.text = "ГЕРОИ"
		var fallback_cast_lines := _build_cast_metric_lines(false)
		slot_c_title.text = fallback_cast_lines[0]
		slot_c_cue.text = "\n".join(fallback_cast_lines.slice(1))
		return

	slot_a_headshot.text = "ФИЛЬМ"
	slot_a_title.text = "Глубина %s" % _display_metric(_game_state_value("film_depth"))
	slot_a_cue.text = "Забвение %s\nДавление %s" % [
		_display_metric_with_risk(_game_state_value("film_oblivion")),
		_display_metric_with_risk(_game_state_value("film_pressure")),
	]

	slot_b_headshot.text = "АКЦЕНТ"
	slot_b_title.text = _format_snapshot_fragment_label(stage)
	slot_b_cue.text = "Сила влияния — %s" % _display_metric(_game_state_value("control_next"))

	slot_c_headshot.text = "ГЕРОИ"
	var cast_lines := _build_cast_metric_lines(false)
	slot_c_title.text = cast_lines[0]
	slot_c_cue.text = "\n".join(cast_lines.slice(1))


func _apply_explanation_bubble(show_overlay: bool) -> void:
	explanation_bubble.visible = show_overlay
	explanation_bubble_tail.visible = show_overlay
	explanation_bubble_label.text = EXPLANATION_COPY


func _apply_metrics_note(stage: String) -> void:
	var show_note := _should_show_metrics_note(stage)
	metrics_note_panel.visible = show_note
	if not show_note:
		return
	metrics_note_title.text = METRICS_NOTE_TITLE_COPY
	metrics_note_body.text = METRICS_NOTE_BODY_COPY


func _should_show_metrics_note(stage: String) -> bool:
	if stage.strip_edges().to_upper() != "F1":
		return false
	if not has_node("/root/GameState"):
		return true
	return not bool(GameState.seen_metrics_note)


func _get_snapshot_context() -> Dictionary:
	if not has_node("/root/GameState"):
		return {
			"source_phase": "",
			"pending_snapshot_stage": "",
			"show_explanatory_overlay": false,
			"latest_outcome_focus": "neutral",
		}
	return GameState.get_snapshot_context()


func _style_metrics_readout() -> void:
	slot_b_title.add_theme_font_size_override("font_size", 17)
	slot_b_title.add_theme_color_override("font_color", Color(0.972, 0.925, 0.862, 0.98))
	slot_c_title.add_theme_font_size_override("font_size", 13)
	slot_c_title.add_theme_color_override("font_color", Color(0.804, 0.741, 0.64, 0.9))
	slot_c_cue.add_theme_font_size_override("font_size", 13)
	slot_c_cue.add_theme_color_override("font_color", Color(0.804, 0.741, 0.64, 0.9))


func _game_state_value(field_name: String) -> Variant:
	if not has_node("/root/GameState"):
		return null
	return GameState.get(field_name)


func _display_metric(value: Variant) -> String:
	if value == null:
		return "(неизвестно)"
	return str(value)


func _display_metric_with_risk(value: Variant) -> String:
	if value == null:
		return "(неизвестно)"
	var metric_value := int(value)
	return "%d [%s]" % [metric_value, _risk_tag(metric_value)]


func _risk_tag(value: int) -> String:
	if value <= 2:
		return "низкий"
	if value <= 5:
		return "средний"
	return "высокий"


func _build_completed_summary() -> String:
	if not has_node("/root/GameState") or GameState.completed_phases.is_empty():
		return "ничего"
	return ", ".join(GameState.completed_phases)


func _build_cast_metric_lines(use_short_labels: bool) -> PackedStringArray:
	var lines := PackedStringArray()
	for character_id: String in CAST_CHARACTER_ORDER:
		lines.append(_format_cast_metric_line(character_id, use_short_labels))
	if use_short_labels:
		lines.append("СБ — самобытность. СЛ — след.")
	return lines


func _format_cast_metric_line(character_id: String, use_short_labels: bool) -> String:
	var character_name := str(CAST_CHARACTER_NAMES.get(character_id, character_id.capitalize()))
	var integrity_value := _display_metric(_game_state_value("%s_integrity" % character_id))
	var trauma_value := _display_metric(_game_state_value("%s_trauma" % character_id))
	if use_short_labels:
		return "%s: СБ %s / СЛ %s" % [character_name, integrity_value, trauma_value]
	return "%s: Самобытность %s / След %s" % [character_name, integrity_value, trauma_value]


func _resolve_focus_label(focus_id: String) -> String:
	match _normalize_snapshot_focus(focus_id):
		"desmond":
			return "Дезмонд"
		"victoria":
			return "Виктория"
		"scene":
			return "Сцена"
		_:
			return "Оставлено как есть"


func _resolve_snapshot_stage(snapshot_context: Dictionary) -> String:
	var stage := str(snapshot_context.get("pending_snapshot_stage", snapshot_context.get("source_phase", ""))).strip_edges().to_upper()
	if stage.is_empty():
		return "(неизвестно)"
	return stage


func _format_snapshot_fragment_label(stage: String) -> String:
	match stage.strip_edges().to_upper():
		"F1":
			return "После фрагмента 1"
		"F2":
			return "После фрагмента 2"
		"F3":
			return "После фрагмента 3"
		_:
			return "После фрагмента"


func _load_snapshot_notes() -> Dictionary:
	var notes := {
		"DEFAULT": DEFAULT_SNAPSHOT_NOTE,
	}
	if not FileAccess.file_exists(SNAPSHOT_NOTES_PATH):
		push_warning("SnapshotScreen: snapshot notes TSV not found at %s" % SNAPSHOT_NOTES_PATH)
		return notes

	var file := FileAccess.open(SNAPSHOT_NOTES_PATH, FileAccess.READ)
	if file == null:
		push_warning("SnapshotScreen: unable to open snapshot notes TSV at %s" % SNAPSHOT_NOTES_PATH)
		return notes

	while not file.eof_reached():
		var raw_line := file.get_line()
		var line := raw_line.strip_edges()
		if line.is_empty():
			continue

		var columns := raw_line.split("\t", true, 3)
		if columns.size() < 4:
			continue

		var key := str(columns[0]).strip_edges()
		if key.is_empty() or key.to_lower() == "key":
			continue

		notes[key.to_upper()] = str(columns[3]).strip_edges()

	return notes


func _resolve_snapshot_note(snapshot_context: Dictionary) -> String:
	var stage := str(snapshot_context.get("pending_snapshot_stage", snapshot_context.get("source_phase", ""))).strip_edges().to_upper()
	var focus := _normalize_snapshot_focus(str(snapshot_context.get("latest_outcome_focus", "neutral")))

	if not stage.is_empty():
		var key := "%s_%s" % [stage, focus.to_upper()]
		if snapshot_notes.has(key):
			return str(snapshot_notes[key])

		if focus == "mixed":
			var neutral_key := "%s_NEUTRAL" % stage
			if snapshot_notes.has(neutral_key):
				return str(snapshot_notes[neutral_key])

	return str(snapshot_notes.get("DEFAULT", DEFAULT_SNAPSHOT_NOTE))


func _normalize_snapshot_focus(focus: String) -> String:
	var normalized_focus := focus.strip_edges().to_lower()
	match normalized_focus:
		"scene", "victoria", "desmond", "neutral", "mixed":
			return normalized_focus
		_:
			return "neutral"


func _resume_playback() -> void:
	if has_node("/root/GameState"):
		if metrics_note_panel.visible:
			GameState.seen_metrics_note = true
		var snapshot_context := GameState.get_snapshot_context()
		var source_phase := str(snapshot_context.get("source_phase", "")).strip_edges()
		if source_phase == "F1":
			var next_phase := str(GameState.current_phase).strip_edges()
			if next_phase.is_empty():
				next_phase = GameState.get_next_phase(source_phase)
			if not next_phase.is_empty():
				GameState.set_gameplay_resume(next_phase, 0)
		GameState.clear_snapshot_context()
	get_tree().change_scene_to_file(GAMEPLAY_SCENE_PATH)
