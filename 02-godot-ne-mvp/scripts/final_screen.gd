extends Control

const TITLE_SCENE_PATH := "res://scenes/start/TitleScreen.tscn"
const RESTART_SCENE_PATH := "res://scenes/gameplay/GameplayScreen.tscn"
const WEYR_BACKDROP_SCENE := preload("res://scenes/shared/WeyrBackdrop.tscn")
const DOMINANT_ZONE_ORDER := ["scene", "victoria", "desmond"]
const DEFAULT_FRAGMENT_ID := "MATERIAL_PRODUCTIVE_PARANOIA"
const STANDARD_OVERLAY_ACCENT := Color(0.63, 0.50, 0.34, 0.92)
const INTRUSION_OVERLAY_ACCENT := Color(0.63, 0.34, 0.86, 0.96)

var _text_repository := FinalTextRepository.new()
var _current_fragment: Dictionary = {}

@onready var background_shade: ColorRect = %BackgroundShade
@onready var shell_frame: PanelContainer = %ShellFrame
@onready var overline: Label = %Overline
@onready var title_label: Label = %Title
@onready var summary_panel: PanelContainer = %SummaryPanel
@onready var summary_heading: Label = %SummaryHeading
@onready var summary_body: Label = %SummaryBody
@onready var fragment_panel: PanelContainer = %FragmentPanel
@onready var fragment_heading: Label = %FragmentHeading
@onready var fragment_title: Label = %FragmentTitle
@onready var fragment_button: Button = %FragmentButton
@onready var stance_panel: PanelContainer = %StancePanel
@onready var stance_heading: Label = %StanceHeading
@onready var stance_title: Label = %StanceTitle
@onready var stance_body: Label = %StanceBody
@onready var markers_panel: PanelContainer = %MarkersPanel
@onready var markers_heading: Label = %MarkersHeading
@onready var marker_slot_a: PanelContainer = %MarkerSlotA
@onready var marker_slot_a_title: Label = %MarkerSlotATitle
@onready var marker_slot_a_body: Label = %MarkerSlotABody
@onready var marker_slot_b: PanelContainer = %MarkerSlotB
@onready var marker_slot_b_title: Label = %MarkerSlotBTitle
@onready var marker_slot_b_body: Label = %MarkerSlotBBody
@onready var marker_slot_c: PanelContainer = %MarkerSlotC
@onready var marker_slot_c_title: Label = %MarkerSlotCTitle
@onready var marker_slot_c_body: Label = %MarkerSlotCBody
@onready var footer_note: Label = %FooterNote
@onready var restart_button: Button = %RestartButton
@onready var return_button: Button = %ReturnButton
@onready var producer_note: PanelContainer = %ProducerNote
@onready var producer_note_header: Label = %ProducerNoteHeader
@onready var producer_note_body: Label = %ProducerNoteBody
@onready var fragment_overlay: Control = %FragmentOverlay
@onready var fragment_overlay_card: PanelContainer = %OverlayCard
@onready var fragment_overlay_accent: ColorRect = %OverlayAccent
@onready var fragment_overlay_section: Label = %OverlaySection
@onready var fragment_overlay_title: Label = %OverlayTitle
@onready var fragment_overlay_body: Label = %OverlayBody
@onready var fragment_overlay_close_button: Button = %OverlayCloseButton


func _ready() -> void:
	_ensure_weyr_backdrop()
	_apply_screen_styles()
	_bind_game_state()
	fragment_button.pressed.connect(_open_fragment_overlay)
	fragment_overlay_close_button.pressed.connect(_close_fragment_overlay)
	restart_button.pressed.connect(_restart_run)
	return_button.pressed.connect(_back_to_menu)
	restart_button.grab_focus()


func _bind_game_state() -> void:
	var dominant_zone := _resolve_summary_zone()
	var mode := _resolve_final_mode()
	var fragment_id := _resolve_fragment_id()
	var stance_id := _resolve_final_stance_id(dominant_zone, fragment_id)

	var summary := _text_repository.get_summary(mode, dominant_zone)
	var producer_note_copy := _text_repository.get_producer_note(stance_id, mode)
	var stance_copy := _text_repository.get_stance_text(stance_id)
	_current_fragment = _text_repository.get_dossier_fragment(fragment_id)
	var achievements := _resolve_achievements({
		"mode": mode,
		"dominant_zone": dominant_zone,
		"stance": stance_id,
		"pressure_high": _build_pressure_high(),
		"any_character_trace_high": _build_character_trace_high(),
	})

	overline.text = "WEYR RESOLUTION // TERMINAL RECORD"
	title_label.text = "Что вышло"

	summary_heading.text = "В ИТОГЕ"
	summary_body.text = str(summary.get("summary_ru", ""))

	fragment_heading.text = "ОТКРЫТЫЙ ФРАГМЕНТ"
	fragment_title.text = str(_current_fragment.get("title_ru", ""))
	fragment_button.text = str(_current_fragment.get("button_ru", "Открыть фрагмент"))

	stance_heading.text = "ВАШ ПОДХОД"
	stance_title.text = str(stance_copy.get("short_ru", ""))
	stance_body.text = str(stance_copy.get("full_ru", ""))

	markers_heading.text = "ОТМЕТКИ"
	_bind_marker_slots(achievements)

	producer_note_header.text = "ПОМЕТКА ПРОДЮСЕРА"
	producer_note_body.text = str(producer_note_copy.get("note_ru", ""))

	footer_note.text = "Новый прогон начнётся с первого фрагмента. Открытый фрагмент досье сохранится до полного сброса приложения."
	return_button.text = "Взглянуть на досье"

	fragment_overlay.visible = false


func _resolve_summary_zone() -> String:
	var raw_zone := _read_string("dominant_zone").strip_edges().to_upper()
	match raw_zone:
		"SCENE":
			return "SCENE"
		"VICTORIA":
			return "VICTORIA"
		"DESMOND":
			return "DESMOND"
		"NEUTRAL", "BALANCED":
			return "BALANCED"

	var allocation := _read_allocation()
	var scene_points := int(allocation.get("scene", 0))
	var victoria_points := int(allocation.get("victoria", 0))
	var desmond_points := int(allocation.get("desmond", 0))
	var highest_points := maxi(scene_points, maxi(victoria_points, desmond_points))
	if highest_points <= 0:
		return "BALANCED"

	var winners := PackedStringArray()
	for zone_id: String in DOMINANT_ZONE_ORDER:
		if int(allocation.get(zone_id, 0)) == highest_points:
			winners.append(zone_id)

	if winners.size() != 1:
		return "BALANCED"

	match winners[0]:
		"scene":
			return "SCENE"
		"victoria":
			return "VICTORIA"
		"desmond":
			return "DESMOND"
		_:
			return "BALANCED"


func _resolve_final_mode() -> String:
	var pressure := int(_read_game_state_value("film_pressure", 0))
	var oblivion := int(_read_game_state_value("film_oblivion", 0))
	if pressure > oblivion:
		return "BURN"
	if oblivion > pressure:
		return "OBLIVION"
	return "CLEAN"


func _resolve_fragment_id() -> String:
	var fragment_id := _read_string("final_dossier_fragment_id").strip_edges().to_upper()
	if fragment_id.is_empty():
		if has_node("/root/GameState"):
			GameState.final_dossier_fragment_id = DEFAULT_FRAGMENT_ID
			GameState.unlock_final_dossier_fragment()
		return DEFAULT_FRAGMENT_ID
	if has_node("/root/GameState"):
		GameState.unlock_final_dossier_fragment()
	return fragment_id


func _resolve_final_stance_id(dominant_zone: String, fragment_id: String) -> String:
	match dominant_zone:
		"VICTORIA":
			return "PSYCHOLOGIST"
		"DESMOND":
			return "DIRECTOR"
		"SCENE":
			return "DRAMATIST"

	match fragment_id:
		"VICTORIA_HUMAN_COST", "VICTORIA_LEONARD_OLD_RESCUE":
			return "PSYCHOLOGIST"
		"DESMOND_FUNCTION", "DESMOND_VICTORIA_PRICE":
			return "DIRECTOR"
		_:
			return "DRAMATIST"


func _resolve_achievements(context: Dictionary) -> Array[Dictionary]:
	var achievements: Array[Dictionary] = []
	for achievement: Dictionary in _text_repository.get_all_achievements():
		if achievements.size() >= 3:
			break
		if _matches_achievement(achievement, context):
			achievements.append(achievement)
	return achievements


func _matches_achievement(achievement: Dictionary, context: Dictionary) -> bool:
	var hint := str(achievement.get("trigger_hint", "")).strip_edges()
	if hint.is_empty():
		return false

	for raw_clause: String in hint.split(";", false):
		var clause := raw_clause.strip_edges()
		if clause.is_empty():
			continue

		var separator_index := clause.find("=")
		if separator_index == -1:
			continue

		var key := clause.substr(0, separator_index).strip_edges().to_lower()
		var value := clause.substr(separator_index + 1).strip_edges()
		if not _matches_achievement_clause(key, value, context):
			return false
	return true


func _matches_achievement_clause(key: String, value: String, context: Dictionary) -> bool:
	match key:
		"mode":
			return _matches_any_value(str(context.get("mode", "")), value)
		"dominant_zone":
			return _matches_any_value(str(context.get("dominant_zone", "")), value)
		"stance":
			return _matches_any_value(str(context.get("stance", "")), value)
		"pressure_high":
			return bool(context.get("pressure_high", false)) == (value.to_lower() == "true")
		"any_character_trace_high":
			return bool(context.get("any_character_trace_high", false)) == (value.to_lower() == "true")
		_:
			return false


func _matches_any_value(actual_value: String, expected_value: String) -> bool:
	var normalized_actual := actual_value.strip_edges().to_upper()
	for variant: String in expected_value.to_upper().split(" OR ", false):
		if normalized_actual == variant.strip_edges():
			return true
	return false


func _bind_marker_slots(achievements: Array[Dictionary]) -> void:
	var titles := [marker_slot_a_title, marker_slot_b_title, marker_slot_c_title]
	var bodies := [marker_slot_a_body, marker_slot_b_body, marker_slot_c_body]

	for index: int in range(3):
		if index < achievements.size():
			titles[index].text = str(achievements[index].get("title_ru", ""))
			bodies[index].text = str(achievements[index].get("description_ru", ""))
			titles[index].modulate = Color(1, 1, 1, 1)
			bodies[index].modulate = Color(1, 1, 1, 1)
		else:
			titles[index].text = ""
			bodies[index].text = ""
			titles[index].modulate = Color(1, 1, 1, 0.18)
			bodies[index].modulate = Color(1, 1, 1, 0.18)


func _open_fragment_overlay() -> void:
	fragment_overlay_section.text = str(_current_fragment.get("section_ru", ""))
	fragment_overlay_title.text = str(_current_fragment.get("title_ru", ""))
	fragment_overlay_body.text = str(_current_fragment.get("fragment_ru", ""))
	_apply_fragment_overlay_style(str(_current_fragment.get("style", "standard")))
	fragment_overlay.visible = true
	fragment_overlay_close_button.grab_focus()


func _close_fragment_overlay() -> void:
	fragment_overlay.visible = false
	fragment_button.grab_focus()


func _apply_fragment_overlay_style(style_id: String) -> void:
	var normalized_style := style_id.strip_edges().to_lower()
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.93, 0.89, 0.82, 0.985)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 2
	style.border_color = Color(0.58, 0.47, 0.34, 0.94)
	style.corner_radius_top_left = 18
	style.corner_radius_top_right = 12
	style.corner_radius_bottom_right = 18
	style.corner_radius_bottom_left = 12
	style.shadow_color = Color(0.0, 0.0, 0.0, 0.24)
	style.shadow_size = 20

	fragment_overlay_card.add_theme_stylebox_override("panel", style)
	fragment_overlay_accent.color = STANDARD_OVERLAY_ACCENT
	fragment_overlay_section.add_theme_color_override("font_color", Color(0.38, 0.28, 0.18, 0.95))
	fragment_overlay_title.add_theme_color_override("font_color", Color(0.15, 0.12, 0.09, 0.98))
	fragment_overlay_body.add_theme_color_override("font_color", Color(0.21, 0.17, 0.13, 0.96))

	if normalized_style == "weyr_intrusion":
		style.bg_color = Color(0.88, 0.84, 0.91, 0.985)
		style.border_color = Color(0.46, 0.28, 0.66, 0.96)
		fragment_overlay_card.add_theme_stylebox_override("panel", style)
		fragment_overlay_accent.color = INTRUSION_OVERLAY_ACCENT
		fragment_overlay_section.add_theme_color_override("font_color", Color(0.42, 0.22, 0.56, 0.98))
		fragment_overlay_title.add_theme_color_override("font_color", Color(0.19, 0.12, 0.25, 0.98))
		fragment_overlay_body.add_theme_color_override("font_color", Color(0.22, 0.14, 0.29, 0.96))


func _restart_run() -> void:
	if has_node("/root/GameState"):
		GameState.reset_run()
	get_tree().change_scene_to_file(RESTART_SCENE_PATH)


func _back_to_menu() -> void:
	get_tree().change_scene_to_file(TITLE_SCENE_PATH)


func _read_allocation() -> Dictionary:
	if has_node("/root/GameState"):
		return GameState.surgery_allocation
	return {"scene": 0, "victoria": 0, "desmond": 0}


func _read_string(field_name: String) -> String:
	if not has_node("/root/GameState"):
		return ""
	return str(GameState.get(field_name))


func _read_game_state_value(field_name: String, fallback: Variant) -> Variant:
	if not has_node("/root/GameState"):
		return fallback
	return GameState.get(field_name)


func _build_character_trace_high() -> bool:
	var trauma_values := [
		int(_read_game_state_value("desmond_trauma", 0)),
		int(_read_game_state_value("victoria_trauma", 0)),
		int(_read_game_state_value("leonard_trauma", 0)),
	]
	for trauma_value: int in trauma_values:
		if trauma_value >= 3:
			return true
	return false


func _build_pressure_high() -> bool:
	return int(_read_game_state_value("film_pressure", 0)) >= 3


func _ensure_weyr_backdrop() -> void:
	if has_node("WeyrBackdrop"):
		return
	var backdrop := WEYR_BACKDROP_SCENE.instantiate()
	backdrop.name = "WeyrBackdrop"
	add_child(backdrop)
	move_child(backdrop, 0)


func _apply_screen_styles() -> void:
	background_shade.color = Color(0.02, 0.015, 0.03, 0.18)
	shell_frame.add_theme_stylebox_override("panel", _make_shell_frame_style())

	for panel: PanelContainer in [
		summary_panel,
		fragment_panel,
		stance_panel,
		markers_panel,
	]:
		panel.add_theme_stylebox_override("panel", _make_panel_style())

	for slot: PanelContainer in [
		marker_slot_a,
		marker_slot_b,
		marker_slot_c,
	]:
		slot.add_theme_stylebox_override("panel", _make_marker_slot_style())

	producer_note.add_theme_stylebox_override("panel", _make_producer_note_style())

	_style_label(overline, 11, Color(0.72, 0.67, 0.84, 0.54))
	_style_label(title_label, 31, Color(0.95, 0.92, 0.98, 1.0))
	_style_label(summary_heading, 14, Color(0.88, 0.79, 0.65, 0.96))
	_style_label(summary_body, 16, Color(0.94, 0.94, 0.98, 0.98))
	_style_label(fragment_heading, 14, Color(0.88, 0.79, 0.65, 0.96))
	_style_label(fragment_title, 17, Color(0.95, 0.92, 0.98, 0.98))
	_style_label(stance_heading, 14, Color(0.88, 0.79, 0.65, 0.96))
	_style_label(stance_title, 16, Color(0.95, 0.92, 0.98, 0.98))
	_style_label(stance_body, 14, Color(0.86, 0.86, 0.92, 0.96))
	_style_label(markers_heading, 14, Color(0.88, 0.79, 0.65, 0.96))
	_style_label(footer_note, 12, Color(0.76, 0.76, 0.84, 0.88))
	_style_label(producer_note_header, 11, Color(0.29, 0.22, 0.15, 0.98))
	_style_label(producer_note_body, 13, Color(0.22, 0.17, 0.13, 0.96))
	_style_label(fragment_overlay_section, 13, Color(0.38, 0.28, 0.18, 0.95))
	_style_label(fragment_overlay_title, 22, Color(0.15, 0.12, 0.09, 0.98))
	_style_label(fragment_overlay_body, 16, Color(0.21, 0.17, 0.13, 0.96))

	for title: Label in [marker_slot_a_title, marker_slot_b_title, marker_slot_c_title]:
		_style_label(title, 14, Color(0.95, 0.92, 0.98, 0.98))
	for body: Label in [marker_slot_a_body, marker_slot_b_body, marker_slot_c_body]:
		_style_label(body, 12, Color(0.81, 0.81, 0.88, 0.94))

	_apply_button_style(fragment_button, true)
	_apply_button_style(restart_button, true)
	_apply_button_style(return_button, false)
	_apply_button_style(fragment_overlay_close_button, false)


func _style_label(label: Label, font_size: int, color: Color) -> void:
	if label == null:
		push_warning("FinalScreen: tried to style null label")
		return
	label.add_theme_font_size_override("font_size", font_size)
	label.add_theme_color_override("font_color", color)
	label.add_theme_color_override("font_outline_color", Color(0.01, 0.01, 0.02, 0.82))
	label.add_theme_constant_override("outline_size", 1)


func _apply_button_style(button: Button, primary: bool) -> void:
	var normal := StyleBoxFlat.new()
	normal.bg_color = Color(0.35, 0.18, 0.46, 0.98) if primary else Color(0.14, 0.12, 0.18, 0.95)
	normal.border_width_left = 2
	normal.border_width_top = 2
	normal.border_width_right = 2
	normal.border_width_bottom = 2
	normal.border_color = Color(0.9, 0.77, 1.0, 0.94) if primary else Color(0.56, 0.5, 0.66, 0.92)
	normal.corner_radius_top_left = 12
	normal.corner_radius_top_right = 12
	normal.corner_radius_bottom_right = 12
	normal.corner_radius_bottom_left = 12
	normal.shadow_color = Color(0, 0, 0, 0.34)
	normal.shadow_size = 10

	var hover := normal.duplicate() as StyleBoxFlat
	hover.bg_color = normal.bg_color.lightened(0.08)
	var pressed := normal.duplicate() as StyleBoxFlat
	pressed.bg_color = normal.bg_color.darkened(0.08)

	button.add_theme_stylebox_override("normal", normal)
	button.add_theme_stylebox_override("hover", hover)
	button.add_theme_stylebox_override("pressed", pressed)
	button.add_theme_font_size_override("font_size", 15)
	button.add_theme_color_override("font_color", Color(0.95, 0.95, 0.98, 1.0))
	button.add_theme_color_override("font_hover_color", Color(0.95, 0.95, 0.98, 1.0))
	button.add_theme_color_override("font_pressed_color", Color(0.95, 0.95, 0.98, 1.0))


func _make_panel_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.06, 0.055, 0.09, 0.62)
	style.border_width_left = 2
	style.border_width_top = 2
	style.border_width_right = 2
	style.border_width_bottom = 2
	style.border_color = Color(0.53, 0.37, 0.71, 0.82)
	style.corner_radius_top_left = 18
	style.corner_radius_top_right = 18
	style.corner_radius_bottom_right = 18
	style.corner_radius_bottom_left = 18
	style.shadow_color = Color(0.11, 0.04, 0.18, 0.18)
	style.shadow_size = 14
	return style


func _make_marker_slot_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.10, 0.09, 0.15, 0.56)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.46, 0.36, 0.62, 0.72)
	style.corner_radius_top_left = 12
	style.corner_radius_top_right = 12
	style.corner_radius_bottom_right = 12
	style.corner_radius_bottom_left = 12
	return style


func _make_shell_frame_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.02, 0.02, 0.03, 0.08)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.44, 0.31, 0.58, 0.22)
	style.corner_radius_top_left = 28
	style.corner_radius_top_right = 28
	style.corner_radius_bottom_right = 28
	style.corner_radius_bottom_left = 28
	style.shadow_color = Color(0.16, 0.06, 0.24, 0.08)
	style.shadow_size = 20
	return style


func _make_producer_note_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.92, 0.88, 0.8, 0.95)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 2
	style.border_color = Color(0.60, 0.50, 0.36, 0.94)
	style.corner_radius_top_left = 12
	style.corner_radius_top_right = 10
	style.corner_radius_bottom_right = 12
	style.corner_radius_bottom_left = 10
	style.shadow_color = Color(0, 0, 0, 0.20)
	style.shadow_size = 10
	return style
