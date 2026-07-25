extends Control

const GAMEPLAY_SCENE_PATH := "res://scenes/gameplay/GameplayScreen.tscn"
const FINAL_SCENE_PATH := "res://scenes/gameplay/FinalScreen.tscn"
const TOTAL_CONTROL := 2
const ZONE_IDS := ["scene", "victoria", "desmond"]
const ZONE_CONTENT := {
	"scene": {
		"title": "СЦЕНА",
		"subtitle": "Сделать красиво",
	},
	"victoria": {
		"title": "ВИКТОРИЯ",
		"subtitle": "Вопрос цены",
	},
	"desmond": {
		"title": "ДЕЗМОНД",
		"subtitle": "Вопрос решения",
	},
}

const COLOR_PANEL := Color(0.08, 0.09, 0.12, 0.92)
const COLOR_PANEL_ACTIVE := Color(0.16, 0.09, 0.22, 0.95)
const COLOR_STEEL := Color(0.34, 0.38, 0.44, 0.6)
const COLOR_VIOLET := Color(0.69, 0.39, 1.0, 1.0)
const COLOR_VIOLET_SOFT := Color(0.55, 0.28, 0.85, 0.35)
const COLOR_VIOLET_HARD := Color(0.88, 0.68, 1.0, 1.0)
const COLOR_CONTAMINATION := Color(0.75, 0.45, 1.0, 0.18)
const COLOR_TEXT := Color(0.92, 0.94, 0.98, 1.0)
const COLOR_TEXT_MUTED := Color(0.66, 0.69, 0.78, 0.92)
const COLOR_TEXT_DIM := Color(0.48, 0.5, 0.58, 0.86)

@onready var header_block: VBoxContainer = $Margin/RootColumn/HeaderWrap/HeaderBlock
@onready var overline_label: Label = $Margin/RootColumn/HeaderWrap/HeaderBlock/Overline
@onready var title_label: Label = $Margin/RootColumn/HeaderWrap/HeaderBlock/Title
@onready var subtitle_label: Label = $Margin/RootColumn/HeaderWrap/HeaderBlock/Subtitle
@onready var helper_label: Label = $Margin/RootColumn/HeaderWrap/HeaderBlock/Helper

@onready var route_layer: Control = $Margin/RootColumn/DiagramArea/RouteLayer
@onready var weyr_core: PanelContainer = $Margin/RootColumn/DiagramArea/WeyrCore
@onready var weyr_title: Label = $Margin/RootColumn/DiagramArea/WeyrCore/Margin/Column/WeyrTitle
@onready var weyr_subtitle: Label = $Margin/RootColumn/DiagramArea/WeyrCore/Margin/Column/WeyrSubtitle
@onready var diagram_area: Control = $Margin/RootColumn/DiagramArea
@onready var footer_block: VBoxContainer = $Margin/RootColumn/FooterBlock
@onready var button_wrap: MarginContainer = $Margin/RootColumn/FooterBlock/ButtonWrap
@onready var button_row: HBoxContainer = $Margin/RootColumn/FooterBlock/ButtonWrap/ButtonRow

@onready var scene_zone: PanelContainer = $Margin/RootColumn/DiagramArea/SceneZone
@onready var scene_zone_title: Label = $Margin/RootColumn/DiagramArea/SceneZone/Margin/Column/SceneZoneTitle
@onready var scene_zone_subtitle: Label = $Margin/RootColumn/DiagramArea/SceneZone/Margin/Column/SceneZoneSubtitle
@onready var scene_zone_points: Label = $Margin/RootColumn/DiagramArea/SceneZone/Margin/Column/SceneZonePoints

@onready var victoria_zone: PanelContainer = $Margin/RootColumn/DiagramArea/VictoriaZone
@onready var victoria_zone_title: Label = $Margin/RootColumn/DiagramArea/VictoriaZone/Margin/Column/VictoriaZoneTitle
@onready var victoria_zone_subtitle: Label = $Margin/RootColumn/DiagramArea/VictoriaZone/Margin/Column/VictoriaZoneSubtitle
@onready var victoria_zone_points: Label = $Margin/RootColumn/DiagramArea/VictoriaZone/Margin/Column/VictoriaZonePoints

@onready var desmond_zone: PanelContainer = $Margin/RootColumn/DiagramArea/DesmondZone
@onready var desmond_zone_title: Label = $Margin/RootColumn/DiagramArea/DesmondZone/Margin/Column/DesmondZoneTitle
@onready var desmond_zone_subtitle: Label = $Margin/RootColumn/DiagramArea/DesmondZone/Margin/Column/DesmondZoneSubtitle
@onready var desmond_zone_points: Label = $Margin/RootColumn/DiagramArea/DesmondZone/Margin/Column/DesmondZonePoints

@onready var remaining_label: Label = $Margin/RootColumn/FooterBlock/SummaryBlock/RemainingLabel
@onready var status_label: Label = $Margin/RootColumn/FooterBlock/SummaryBlock/StatusLabel
@onready var reset_button: Button = $Margin/RootColumn/FooterBlock/ButtonWrap/ButtonRow/ResetButton
@onready var confirm_button: Button = $Margin/RootColumn/FooterBlock/ButtonWrap/ButtonRow/ConfirmButton

var pulse_time := 0.0
var source_phase := "F1"
var smooth_button: Button
var zone_panels := {}
var zone_title_labels := {}
var zone_subtitle_labels := {}
var zone_point_labels := {}
var zone_allocation := {
	"scene": 0,
	"victoria": 0,
	"desmond": 0,
}
var last_allocated_zone := ""


func _ready() -> void:
	_resolve_source_phase()
	_configure_copy()
	_configure_buttons()
	_configure_zones()
	_update_header_width()
	_apply_compact_layout()
	if route_layer.has_method("setup"):
		route_layer.setup(self)
	_refresh_allocation_state()
	set_process(true)
	route_layer.queue_redraw()


func _notification(what: int) -> void:
	if what == NOTIFICATION_RESIZED:
		_update_header_width()
		_apply_compact_layout()


func _process(delta: float) -> void:
	pulse_time += delta
	route_layer.queue_redraw()


func _draw_route_layer(layer: Control) -> void:
	if not is_instance_valid(weyr_core):
		return

	var core_center := _to_layer_center(weyr_core, layer)
	var phase := 0.5 + 0.5 * sin(pulse_time * 1.7)
	var outer_radius := 96.0 + phase * 9.0
	var middle_radius := 66.0 + phase * 13.0
	var inner_radius := 28.0 + phase * 5.0

	for zone_id: String in zone_panels.keys():
		var zone_center := _to_layer_center(zone_panels[zone_id], layer)
		var zone_rect := _to_layer_rect(zone_panels[zone_id], layer)
		var allocation := int(zone_allocation.get(zone_id, 0))
		var route_strength := float(allocation) / float(TOTAL_CONTROL)
		var route_direction: Vector2 = (zone_center - core_center).normalized()
		var route_endpoint: Vector2 = zone_center - route_direction * min(zone_rect.size.x, zone_rect.size.y) * 0.46
		var baseline_color := Color(0.46, 0.5, 0.6, 0.62)
		var baseline_glow := Color(COLOR_VIOLET_SOFT.r, COLOR_VIOLET_SOFT.g, COLOR_VIOLET_SOFT.b, 0.22)
		var route_color := Color(
			lerp(COLOR_VIOLET_SOFT.r, COLOR_VIOLET_HARD.r, route_strength),
			lerp(COLOR_VIOLET_SOFT.g, COLOR_VIOLET_HARD.g, route_strength),
			lerp(COLOR_VIOLET_SOFT.b, COLOR_VIOLET_HARD.b, route_strength),
			0.26 + route_strength * 0.74
		)

		layer.draw_line(core_center, route_endpoint, baseline_glow, 11.0, true)
		layer.draw_line(core_center, route_endpoint, baseline_color, 5.0, true)
		layer.draw_circle(route_endpoint, 7.0, baseline_color)

		if allocation > 0:
			var route_width := 8.0 + float(allocation) * 5.0
			layer.draw_line(core_center, route_endpoint, Color(route_color.r, route_color.g, route_color.b, 0.28 + route_strength * 0.28), route_width + 7.0, true)
			layer.draw_line(core_center, route_endpoint, route_color, route_width, true)
			layer.draw_circle(route_endpoint, 9.0 + float(allocation) * 3.0, route_color)

			for seam_index: int in range(allocation):
				var seam_ratio := 0.34 + float(seam_index) * 0.21 + phase * 0.02
				var midpoint := core_center.lerp(route_endpoint, seam_ratio)
				var drift := Vector2(
					cos(pulse_time * (2.1 + float(seam_index))),
					sin(pulse_time * (1.7 + float(seam_index)))
				) * (6.0 + 2.0 * seam_index)
				layer.draw_circle(midpoint + drift, 7.0 + phase * 3.0 + float(seam_index) * 2.0, Color(route_color.r, route_color.g, route_color.b, 0.45 + 0.16 * seam_index))

			var haze_rect := Rect2(zone_rect.position - Vector2.ONE * 6.0, zone_rect.size + Vector2.ONE * 12.0)
			layer.draw_rect(haze_rect, Color(COLOR_CONTAMINATION.r, COLOR_CONTAMINATION.g, COLOR_CONTAMINATION.b, 0.08 + route_strength * 0.12), false, 2.0)
			var seam_color := Color(COLOR_VIOLET_HARD.r, COLOR_VIOLET_HARD.g, COLOR_VIOLET_HARD.b, 0.22 + route_strength * 0.2)
			var seam_shift := 10.0 + phase * 8.0
			layer.draw_line(zone_rect.position + Vector2(18.0, 16.0), zone_rect.position + Vector2(zone_rect.size.x - 26.0, zone_rect.size.y * 0.45 + seam_shift * 0.15), seam_color, 2.0, true)
			layer.draw_line(zone_rect.position + Vector2(zone_rect.size.x * 0.22, zone_rect.size.y - 20.0), zone_rect.position + Vector2(zone_rect.size.x - 22.0, 22.0 + seam_shift * 0.1), seam_color, 1.0 + float(allocation), true)

	layer.draw_circle(core_center, outer_radius, Color(0.17, 0.05, 0.22, 0.42))
	layer.draw_circle(core_center + Vector2(sin(pulse_time * 1.2), cos(pulse_time * 1.6)) * 10.0, middle_radius, Color(0.45, 0.12, 0.65, 0.3))
	layer.draw_circle(core_center + Vector2(cos(pulse_time * 2.0), sin(pulse_time * 1.4)) * 7.0, inner_radius * 1.9, Color(0.72, 0.28, 1.0, 0.22))
	layer.draw_circle(core_center, inner_radius, Color(0.96, 0.83, 1.0, 0.96))

	var wound_points := PackedVector2Array()
	for index: int in range(12):
		var angle := (TAU / 12.0) * float(index) + pulse_time * 0.35
		var radius := 42.0 + sin(pulse_time * 2.4 + float(index) * 1.3) * 10.0
		wound_points.append(core_center + Vector2.from_angle(angle) * radius)
	layer.draw_colored_polygon(wound_points, Color(0.39, 0.11, 0.52, 0.22))


func _configure_copy() -> void:
	overline_label.text = "ХИРУРГИЧЕСКОЕ ВМЕШАТЕЛЬСТВО // РАССТАНОВКА АКЦЕНТОВ"
	title_label.text = "КУДА ПОЙДЁТ НАПРЯЖЕНИЕ?"
	subtitle_label.text = "Всё уже было снято — событие останется тем же.\nНо как именно его запомнят?"
	helper_label.text = ""
	helper_label.visible = false
	weyr_title.text = "ВЕЙР"
	weyr_subtitle.text = "Жизнь, которая ищет выход"
	remaining_label.text = ""
	status_label.text = ""

	_style_header_label(overline_label, 14, COLOR_TEXT_MUTED)
	_style_header_label(title_label, 34, COLOR_TEXT)
	_style_header_label(subtitle_label, 18, COLOR_TEXT_MUTED)
	_style_header_label(helper_label, 15, COLOR_TEXT_MUTED)
	subtitle_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	helper_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	_style_header_label(weyr_title, 24, COLOR_TEXT)
	_style_header_label(weyr_subtitle, 13, COLOR_TEXT_MUTED)
	_style_header_label(remaining_label, 16, COLOR_VIOLET_HARD)
	_style_header_label(status_label, 15, COLOR_TEXT_MUTED)


func _update_header_width() -> void:
	if not is_instance_valid(header_block):
		return

	var available_width := get_viewport_rect().size.x - 72.0
	header_block.custom_minimum_size.x = clampf(available_width, 420.0, 680.0)


func _configure_buttons() -> void:
	reset_button.text = "СБРОСИТЬ"
	confirm_button.text = "ПРИНЯТЬ АКЦЕНТ"
	reset_button.pressed.connect(_reset_allocation)
	confirm_button.pressed.connect(_confirm_allocation)
	_ensure_smooth_button()
	confirm_button.disabled = true
	_apply_button_style(reset_button, false)
	_apply_button_style(smooth_button, false)
	_apply_button_style(confirm_button, true)


func _configure_zones() -> void:
	zone_panels = {
		"scene": scene_zone,
		"victoria": victoria_zone,
		"desmond": desmond_zone,
	}
	zone_title_labels = {
		"scene": scene_zone_title,
		"victoria": victoria_zone_title,
		"desmond": desmond_zone_title,
	}
	zone_subtitle_labels = {
		"scene": scene_zone_subtitle,
		"victoria": victoria_zone_subtitle,
		"desmond": desmond_zone_subtitle,
	}
	zone_point_labels = {
		"scene": scene_zone_points,
		"victoria": victoria_zone_points,
		"desmond": desmond_zone_points,
	}

	for zone_id: String in zone_panels.keys():
		var panel: PanelContainer = zone_panels[zone_id] as PanelContainer
		var title: Label = zone_title_labels[zone_id] as Label
		var subtitle: Label = zone_subtitle_labels[zone_id] as Label
		var points: Label = zone_point_labels[zone_id] as Label
		var zone_copy: Dictionary = ZONE_CONTENT[zone_id]

		panel.mouse_filter = Control.MOUSE_FILTER_STOP
		panel.mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND
		panel.gui_input.connect(_on_zone_gui_input.bind(zone_id))

		title.text = zone_copy["title"]
		subtitle.text = zone_copy["subtitle"]
		points.text = ""
		_style_header_label(title, 24, COLOR_TEXT)
		_style_header_label(subtitle, 14, COLOR_TEXT_MUTED)
		_style_header_label(points, 13, COLOR_VIOLET_HARD)
		title.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		subtitle.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
		points.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER

	weyr_core.add_theme_stylebox_override("panel", _make_core_style())


func _on_zone_gui_input(event: InputEvent, zone_id: String) -> void:
	if event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT and event.pressed:
		if _get_total_allocated() >= TOTAL_CONTROL:
			return

		zone_allocation[zone_id] = int(zone_allocation.get(zone_id, 0)) + 1
		last_allocated_zone = zone_id
		_refresh_allocation_state()


func _confirm_allocation() -> void:
	if _get_total_allocated() != TOTAL_CONTROL:
		return

	if has_node("/root/GameState"):
		var outcome_id := _resolve_ending_id()
		print("[SurgeryLayer] confirm outcome_id=", outcome_id)
		GameState.current_outcome_id = outcome_id
		GameState.set_gameplay_resume_branch(source_phase, "OUTCOME_%s" % outcome_id)
		var allocation_payload := zone_allocation.duplicate(true)
		allocation_payload["_last_allocated_zone"] = last_allocated_zone
		GameState.apply_surgery_result(source_phase, allocation_payload)
		print("[SurgeryLayer] phase_transition current_phase=", source_phase, " next_phase=", GameState.current_phase, " surgery_result=", outcome_id)
		if source_phase == "F3":
			# F3 still has authored outcome playback after surgery, so keep the phase live
			# and let GameplayScreen resume into the F3 outcome branch.
			GameState.current_phase = source_phase
		if GameState.is_run_complete():
			get_tree().change_scene_to_file(FINAL_SCENE_PATH)
			return
		GameState.set_snapshot_context(source_phase, source_phase == "F1")

	get_tree().change_scene_to_file(GAMEPLAY_SCENE_PATH)


func _confirm_smooth() -> void:
	if not has_node("/root/GameState"):
		get_tree().change_scene_to_file(GAMEPLAY_SCENE_PATH)
		return

	GameState.current_outcome_id = "12D"
	GameState.set_gameplay_resume_branch(source_phase, "OUTCOME_12D")
	GameState.apply_surgery_result(source_phase, {
		"scene": 0,
		"victoria": 0,
		"desmond": 0,
		"_intent": "neutral",
	})
	print("[SurgeryLayer] phase_transition current_phase=", source_phase, " next_phase=", GameState.current_phase, " surgery_result=12D")
	if source_phase == "F3":
		GameState.current_phase = source_phase
	if GameState.is_run_complete():
		get_tree().change_scene_to_file(FINAL_SCENE_PATH)
		return
	GameState.set_snapshot_context(source_phase, source_phase == "F1")
	get_tree().change_scene_to_file(GAMEPLAY_SCENE_PATH)


func _reset_allocation() -> void:
	zone_allocation = {
		"scene": 0,
		"victoria": 0,
		"desmond": 0,
	}
	last_allocated_zone = ""
	_refresh_allocation_state()


func _refresh_allocation_state() -> void:
	_update_zone_visuals()
	route_layer.queue_redraw()


func _update_zone_visuals() -> void:
	for zone_id: String in zone_panels.keys():
		var allocation := int(zone_allocation.get(zone_id, 0))
		var route_strength := float(allocation) / float(TOTAL_CONTROL)
		var has_pressure := allocation > 0
		zone_panels[zone_id].add_theme_stylebox_override("panel", _make_zone_style(allocation))
		zone_title_labels[zone_id].modulate = Color(0.95, 0.86, 1.0, 1.0) if allocation == TOTAL_CONTROL else (COLOR_VIOLET_HARD if has_pressure else COLOR_TEXT)
		zone_subtitle_labels[zone_id].modulate = Color(0.82, 0.74, 0.93, 0.96) if has_pressure else COLOR_TEXT_MUTED
		zone_point_labels[zone_id].text = _format_zone_points(zone_id)
		zone_point_labels[zone_id].modulate = Color(COLOR_VIOLET_HARD.r, COLOR_VIOLET_HARD.g, COLOR_VIOLET_HARD.b, 0.62 + route_strength * 0.38) if has_pressure else COLOR_TEXT_DIM

	remaining_label.text = "Осталось: %d / %d" % [_get_remaining_control(), TOTAL_CONTROL]
	status_label.text = _build_status_line()
	reset_button.disabled = _get_total_allocated() == 0
	confirm_button.disabled = _get_total_allocated() != TOTAL_CONTROL
	if is_instance_valid(smooth_button):
		smooth_button.disabled = false
	_apply_button_style(reset_button, false)
	_apply_button_style(smooth_button, false)
	_apply_button_style(confirm_button, true)


func _get_total_allocated() -> int:
	return int(zone_allocation.get("scene", 0)) + int(zone_allocation.get("victoria", 0)) + int(zone_allocation.get("desmond", 0))


func _get_remaining_control() -> int:
	return TOTAL_CONTROL - _get_total_allocated()


func _format_zone_points(zone_id: String) -> String:
	var allocation := int(zone_allocation.get(zone_id, 0))
	return "%d / %d" % [allocation, TOTAL_CONTROL]


func _build_status_line() -> String:
	var parts := PackedStringArray()
	for zone_id: String in ZONE_IDS:
		parts.append("%s %d" % [ZONE_CONTENT[zone_id]["title"], int(zone_allocation.get(zone_id, 0))])
	return "Акцент %s: %s." % [source_phase, ", ".join(parts)]


func _resolve_ending_id() -> String:
	var outcome_key := "neutral"
	if has_node("/root/GameState"):
		var allocation_payload := zone_allocation.duplicate(true)
		allocation_payload["_last_allocated_zone"] = last_allocated_zone
		outcome_key = GameState.get_surgery_pass_outcome(allocation_payload)
	else:
		if int(zone_allocation.get("scene", 0)) == TOTAL_CONTROL:
			outcome_key = "scene"
		elif int(zone_allocation.get("victoria", 0)) == TOTAL_CONTROL:
			outcome_key = "victoria"
		elif int(zone_allocation.get("desmond", 0)) == TOTAL_CONTROL:
			outcome_key = "desmond"

	match outcome_key:
		"scene":
			return "12A"
		"victoria":
			return "12B"
		"desmond":
			return "12C"
		_:
			return "12D"


func _ensure_smooth_button() -> void:
	if is_instance_valid(smooth_button):
		return

	smooth_button = Button.new()
	smooth_button.name = "SmoothButton"
	smooth_button.text = "ОСТАВИТЬ КАК ЕСТЬ"
	smooth_button.focus_mode = Control.FOCUS_ALL
	smooth_button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	button_row.add_child(smooth_button)
	button_row.move_child(smooth_button, 1)
	smooth_button.pressed.connect(_confirm_smooth)


func _resolve_source_phase() -> void:
	if has_node("/root/GameState"):
		GameState.ensure_runtime_phase()
		source_phase = GameState.current_phase
	if source_phase not in ["F1", "F2", "F3"]:
		source_phase = "F1"


func _apply_compact_layout() -> void:
	if not is_instance_valid(diagram_area):
		return

	var compact_mode := get_viewport_rect().size.y <= 650.0
	diagram_area.custom_minimum_size.y = 270.0 if compact_mode else 320.0
	footer_block.add_theme_constant_override("separation", 6 if compact_mode else 10)
	button_wrap.add_theme_constant_override("margin_top", 2 if compact_mode else 6)
	button_wrap.add_theme_constant_override("margin_bottom", 0 if compact_mode else 10)
	button_row.add_theme_constant_override("separation", 10 if compact_mode else 12)

	var button_height := 48 if compact_mode else 54
	for button: Button in [reset_button, smooth_button, confirm_button]:
		if is_instance_valid(button):
			button.custom_minimum_size = Vector2(0, button_height)


func _to_layer_center(control: Control, layer: Control) -> Vector2:
	return control.get_global_rect().get_center() - layer.global_position


func _to_layer_rect(control: Control, layer: Control) -> Rect2:
	var global_rect := control.get_global_rect()
	return Rect2(global_rect.position - layer.global_position, global_rect.size)


func _style_header_label(label: Label, font_size: int, color: Color) -> void:
	label.add_theme_font_size_override("font_size", font_size)
	label.add_theme_color_override("font_color", color)
	label.add_theme_color_override("font_outline_color", Color(0.01, 0.01, 0.02, 0.9))
	label.add_theme_constant_override("outline_size", 1)


func _apply_button_style(button: Button, is_confirm: bool) -> void:
	var enabled := not button.disabled
	var font_color := COLOR_TEXT if enabled or not is_confirm else COLOR_TEXT_DIM
	var normal := StyleBoxFlat.new()
	normal.bg_color = Color(0.11, 0.12, 0.15, 0.94) if not is_confirm else (Color(0.19, 0.11, 0.26, 0.98) if enabled else Color(0.08, 0.085, 0.11, 0.92))
	normal.border_width_left = 2
	normal.border_width_top = 2
	normal.border_width_right = 2
	normal.border_width_bottom = 2
	normal.border_color = COLOR_STEEL if not is_confirm else (COLOR_VIOLET_HARD if enabled else COLOR_TEXT_DIM)
	normal.corner_radius_top_left = 12
	normal.corner_radius_top_right = 12
	normal.corner_radius_bottom_right = 12
	normal.corner_radius_bottom_left = 12
	normal.shadow_color = Color(0, 0, 0, 0.4)
	normal.shadow_size = 10

	var hover := normal.duplicate() as StyleBoxFlat
	hover.bg_color = normal.bg_color.lightened(0.08)

	var pressed := normal.duplicate() as StyleBoxFlat
	pressed.bg_color = normal.bg_color.darkened(0.08)

	button.add_theme_stylebox_override("normal", normal)
	button.add_theme_stylebox_override("hover", hover)
	button.add_theme_stylebox_override("pressed", pressed)
	button.add_theme_font_size_override("font_size", 18)
	button.add_theme_color_override("font_color", font_color)
	button.add_theme_color_override("font_hover_color", font_color)
	button.add_theme_color_override("font_pressed_color", font_color)
	button.add_theme_color_override("font_disabled_color", COLOR_TEXT_DIM)


func _make_zone_style(allocation: int) -> StyleBoxFlat:
	var route_strength := float(allocation) / float(TOTAL_CONTROL)
	var style := StyleBoxFlat.new()
	style.bg_color = COLOR_PANEL.lerp(COLOR_PANEL_ACTIVE, 0.18 + route_strength * 0.72) if allocation > 0 else COLOR_PANEL
	style.border_width_left = 2
	style.border_width_top = 2
	style.border_width_right = 2
	style.border_width_bottom = 2
	style.border_color = COLOR_STEEL.lerp(COLOR_VIOLET_HARD, route_strength)
	style.corner_radius_top_left = 20
	style.corner_radius_top_right = 20
	style.corner_radius_bottom_right = 20
	style.corner_radius_bottom_left = 20
	style.shadow_color = Color(COLOR_VIOLET.r, COLOR_VIOLET.g, COLOR_VIOLET.b, 0.12 + route_strength * 0.2)
	style.shadow_size = 10 + allocation * 5
	style.expand_margin_left = 2.0
	style.expand_margin_top = 2.0
	style.expand_margin_right = 2.0
	style.expand_margin_bottom = 2.0
	return style


func _make_core_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.07, 0.035, 0.1, 0.88)
	style.border_width_left = 2
	style.border_width_top = 2
	style.border_width_right = 2
	style.border_width_bottom = 2
	style.border_color = Color(0.71, 0.45, 1.0, 0.92)
	style.corner_radius_top_left = 90
	style.corner_radius_top_right = 90
	style.corner_radius_bottom_right = 90
	style.corner_radius_bottom_left = 90
	style.shadow_color = Color(0.35, 0.08, 0.5, 0.45)
	style.shadow_size = 28
	return style
