extends Control

const TITLE_SCENE_PATH := "res://scenes/start/TitleScreen.tscn"
const FINAL_TEXT_REPOSITORY_SCRIPT := preload("res://scripts/final_text_repository.gd")
static var pending_initial_character_id := ""
static var pending_return_scene_path := ""
const DOSSIER_FILES := {
	"victoria": {
		"label": "ВИКТОРИЯ",
		"path": "res://narrative/dossiers/Виктория.md",
		"portrait": "res://art/ChatGPT Image 13 мар. 2026 г., 02_20_06.png",
	},
	"desmond": {
		"label": "ДЕЗМОНД",
		"path": "res://narrative/dossiers/Дезмонд.md",
		"portrait": "res://art/ChatGPT Image 13 мар. 2026 г., 02_26_30.png",
	},
	"leonard": {
		"label": "ЛЕОНАРД",
		"path": "res://narrative/dossiers/Леонард.md",
		"portrait": "res://art/ChatGPT Image 13 мар. 2026 г., 01_57_23.png",
	},
	"oktaviy": {
		"label": "ОКТАВИЙ",
		"path": "res://narrative/dossiers/Октавий.md",
		"portrait": "res://art/ChatGPT Image 13 мар. 2026 г., 03_02_40.png",
	},
}
const CHARACTER_ORDER := ["victoria", "desmond", "leonard", "oktaviy"]
const SECTION_NAME_MAP := {
	"АНКЕТА": "АНКЕТА",
	"ИЗВЕСТНОЕ ПРОШЛОЕ": "ИЗВЕСТНОЕ ПРОШЛОЕ",
	"СВЯЗИ": "СВЯЗИ",
	"РОЛЬ В СЦЕНАРИИ": "РОЛЬ В СЦЕНАРИИ",
	"РОЛЬ В СЮЖЕТЕ": "РОЛЬ В СЦЕНАРИИ",
	"АНОМАЛИЯ": "НЕСООТВЕТСТВИЕ",
	"НЕСООТВЕТСТВИЕ": "НЕСООТВЕТСТВИЕ",
	"ТЕКУЩЕЕ ПРОЧТЕНИЕ": "ТЕКУЩЕЕ ПРОЧТЕНИЕ",
	"МАЛОИЗВЕСТНЫЕ ФАКТЫ (?)": "НЕПОДТВЕРЖДЁННЫЕ СТРОКИ",
	"НЕПОДТВЕРЖДЁННЫЕ СТРОКИ": "НЕПОДТВЕРЖДЁННЫЕ СТРОКИ",
}
const OVERLINE_COPY := "ДОСЬЕ // ПРОЯВЛЕНИЕ ЗАПИСИ"
const SUBTITLE_COPY := "Часть карточки уже читается. Остальное ещё держится в промежуточной форме."
const NAV_HINT_COPY := "Непроявленные фразы не скрыты. Они ещё не приняли окончательную форму."
const INLINE_NOTE_COPY := "Часть фрагментов существует как промежуточная запись и ещё не закрепилась в окончательной формулировке."
const FALLBACK_META_COPY := "Исходное прочтение персонажа"
const INLINE_UNRESOLVED_MARK := "────"
const UNRESOLVED_PARTICLES_ENABLED := true
const UNRESOLVED_PARTICLE_COUNT_MIN := 6
const UNRESOLVED_PARTICLE_COUNT_MAX := 14
const UNRESOLVED_ANIMATION_INTENSITY := 1.0
const UNRESOLVED_DEBUG_ANIMATION := false
const POST_RUN_SECTION_TITLE := "ПРОЯВИЛОСЬ ПОСЛЕ ПРОГОНА"
const POST_RUN_STANDARD_STYLE := "post_run_standard"
const POST_RUN_INTRUSION_STYLE := "post_run_intrusion"

@onready var overline_label: Label = $Margin/RootStack/HeaderBlock/Overline
@onready var subtitle_label: Label = $Margin/RootStack/HeaderBlock/Subtitle
@onready var nav_hint_label: Label = $Margin/RootStack/MainRow/NavPanel/NavMargin/NavStack/NavHint
@onready var tabs_container: VBoxContainer = $Margin/RootStack/MainRow/NavPanel/NavMargin/NavStack/TabsContainer
@onready var portrait_texture: TextureRect = $Margin/RootStack/MainRow/CardPaper/CardMargin/CardRoot/IdentityRow/PortraitPanel/PortraitMargin/PortraitTexture
@onready var character_name_label: Label = $Margin/RootStack/MainRow/CardPaper/CardMargin/CardRoot/IdentityRow/IdentityStack/CharacterNameLabel
@onready var character_meta_label: Label = $Margin/RootStack/MainRow/CardPaper/CardMargin/CardRoot/IdentityRow/IdentityStack/CharacterMetaLabel
@onready var working_note_label: Label = $Margin/RootStack/MainRow/CardPaper/CardMargin/CardRoot/IdentityRow/IdentityStack/WorkingNotePanel/WorkingNoteMargin/WorkingNoteLabel
@onready var sections_column: VBoxContainer = $Margin/RootStack/MainRow/CardPaper/CardMargin/CardRoot/SectionScroll/SectionsColumn
@onready var section_scroll: ScrollContainer = $Margin/RootStack/MainRow/CardPaper/CardMargin/CardRoot/SectionScroll
@onready var back_button: Button = $Margin/RootStack/MainRow/CardPaper/CardMargin/CardRoot/FooterRow/BackButton

var dossier_cache := {}
var tab_buttons := {}
var active_character_id := ""
var unresolved_animated_blocks: Array = []
var unresolved_animation_time := 0.0
var _text_repository = FINAL_TEXT_REPOSITORY_SCRIPT.new()
var dossier_source_paths := {}
var dossier_loaded_entry_counts := {}
var dossier_rendered_entry_counts := {}


static func prepare_open(character_id: String, return_scene_path: String = TITLE_SCENE_PATH) -> void:
	pending_initial_character_id = character_id.strip_edges().to_lower()
	pending_return_scene_path = return_scene_path.strip_edges()


func _ready() -> void:
	_apply_static_copy()
	_load_dossiers()
	_build_tabs()
	back_button.pressed.connect(_return_to_previous_scene)
	var initial_character_id := pending_initial_character_id if dossier_cache.has(pending_initial_character_id) else CHARACTER_ORDER[0]
	if CHARACTER_ORDER.size() > 0:
		_select_character(initial_character_id)
	back_button.grab_focus()
	set_process(true)


func _load_dossiers() -> void:
	dossier_cache.clear()
	dossier_source_paths.clear()
	dossier_loaded_entry_counts.clear()
	for character_id: String in CHARACTER_ORDER:
		var file_data: Dictionary = DOSSIER_FILES.get(character_id, {})
		var path := str(file_data.get("path", ""))
		var dossier := _parse_dossier_markdown(_read_text_file(path))
		var hydrated_dossier := _inject_unlocked_fragments(dossier, character_id)
		var loaded_entry_count := _count_dossier_entries(hydrated_dossier)
		dossier_cache[character_id] = hydrated_dossier
		dossier_source_paths[character_id] = path
		dossier_loaded_entry_counts[character_id] = loaded_entry_count
		print(
			"[DossierReviewScreen] dossier_load character_id=", character_id,
			" dossier_data_source_path=", path if not path.is_empty() else "<none>",
			" loaded_entry_count=", loaded_entry_count
		)
		if loaded_entry_count <= 0:
			push_warning("DossierReviewScreen: dossier for '%s' loaded zero entries from %s" % [character_id, path])


func _build_tabs() -> void:
	for child in tabs_container.get_children():
		child.queue_free()
	tab_buttons.clear()

	for character_id: String in CHARACTER_ORDER:
		var file_data: Dictionary = DOSSIER_FILES.get(character_id, {})
		var button := Button.new()
		button.text = str(file_data.get("label", character_id.to_upper()))
		button.focus_mode = Control.FOCUS_ALL
		button.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		button.custom_minimum_size = Vector2(0, 44)
		button.pressed.connect(_select_character.bind(character_id))
		_style_tab_button(button, false)
		tabs_container.add_child(button)
		tab_buttons[character_id] = button


func _select_character(character_id: String) -> void:
	if not dossier_cache.has(character_id):
		push_warning("DossierReviewScreen: requested character dossier '%s' is not loaded" % character_id)
		return

	active_character_id = character_id
	var file_data: Dictionary = DOSSIER_FILES.get(character_id, {})
	var dossier: Dictionary = dossier_cache.get(character_id, {})
	var source_path := str(dossier_source_paths.get(character_id, ""))
	var loaded_entry_count := int(dossier_loaded_entry_counts.get(character_id, 0))
	print(
		"[DossierReviewScreen] dossier_select character_id=", character_id,
		" dossier_data_source_path=", source_path if not source_path.is_empty() else "<none>",
		" loaded_entry_count=", loaded_entry_count
	)

	character_name_label.text = str(file_data.get("label", character_id.to_upper()))
	character_meta_label.text = _build_meta_line(dossier)
	working_note_label.text = INLINE_NOTE_COPY
	portrait_texture.texture = load(str(file_data.get("portrait", "")))
	_render_sections(dossier)
	_refresh_tab_button_states()
	section_scroll.scroll_vertical = 0


func _refresh_tab_button_states() -> void:
	for character_id: String in tab_buttons.keys():
		var button: Button = tab_buttons[character_id] as Button
		var is_active := character_id == active_character_id
		button.disabled = false
		_style_tab_button(button, is_active)


func _render_sections(dossier: Dictionary) -> void:
	unresolved_animated_blocks.clear()
	for child in sections_column.get_children():
		child.queue_free()

	var sections: Array = dossier.get("sections", [])
	var rendered_entry_count := 0
	for section_variant in sections:
		if section_variant is Dictionary:
			var section_data: Dictionary = section_variant
			var items: Variant = section_data.get("items", [])
			if items is Array:
				rendered_entry_count += items.size()
			sections_column.add_child(_build_section(section_data))
	dossier_rendered_entry_counts[active_character_id] = rendered_entry_count
	print(
		"[DossierReviewScreen] dossier_render character_id=", active_character_id,
		" rendered_entry_count=", rendered_entry_count
	)
	if rendered_entry_count <= 0:
		push_warning("DossierReviewScreen: dossier rendered zero entries for '%s'" % active_character_id)


func _build_section(section_data: Dictionary) -> Control:
	var wrapper := VBoxContainer.new()
	wrapper.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	wrapper.add_theme_constant_override("separation", 8)
	var section_style := str(section_data.get("section_style", "default"))

	var header := Label.new()
	header.text = str(section_data.get("title", ""))
	header.add_theme_font_size_override("font_size", 13)
	header.add_theme_color_override("font_color", _section_header_color(section_style))
	wrapper.add_child(header)

	var panel := PanelContainer.new()
	panel.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	panel.add_theme_stylebox_override("panel", _make_section_panel_style(section_style))
	wrapper.add_child(panel)

	var margin := MarginContainer.new()
	margin.add_theme_constant_override("margin_left", 14)
	margin.add_theme_constant_override("margin_top", 12)
	margin.add_theme_constant_override("margin_right", 14)
	margin.add_theme_constant_override("margin_bottom", 12)
	panel.add_child(margin)

	var body := VBoxContainer.new()
	body.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	body.add_theme_constant_override("separation", 8)
	margin.add_child(body)

	if section_style != "default":
		var section_label := Label.new()
		section_label.text = str(section_data.get("fragment_section", ""))
		section_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		section_label.add_theme_font_size_override("font_size", 12)
		section_label.add_theme_color_override("font_color", _section_accent_color(section_style))
		body.add_child(section_label)

		var fragment_title := Label.new()
		fragment_title.text = str(section_data.get("fragment_title", ""))
		fragment_title.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		fragment_title.add_theme_font_size_override("font_size", 16)
		fragment_title.add_theme_color_override("font_color", _section_title_color(section_style))
		body.add_child(fragment_title)

	var items: Array = section_data.get("items", [])
	for item_variant in items:
		if not (item_variant is Dictionary):
			continue
		var item: Dictionary = item_variant
		var item_type := str(item.get("type", "text"))
		match item_type:
			"text":
				body.add_child(_make_text_line(str(item.get("text", ""))))
			"unresolved_line":
				body.add_child(_make_unresolved_line())
			"unresolved_block":
				body.add_child(_make_unresolved_block())
			"unresolved_focus":
				body.add_child(_make_unresolved_focus(str(item.get("text", "")).strip_edges().to_lower()))

	return wrapper


func _make_text_line(text: String) -> Control:
	if _has_inline_unresolved(text):
		return _make_text_line_with_unresolved(text)

	var label := Label.new()
	label.text = text
	label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	label.add_theme_font_size_override("font_size", 15)
	label.add_theme_color_override("font_color", Color(0.16, 0.11, 0.08, 0.96))
	label.add_theme_constant_override("line_spacing", 3)
	return label


func _make_unresolved_line() -> Control:
	return _make_unresolved_fragment([0.82, 0.64], "", 52, false)


func _make_unresolved_block() -> Control:
	return _make_unresolved_fragment([0.88, 0.74, 0.81], "", 98, false)


func _make_unresolved_focus(text: String) -> Control:
	return _make_unresolved_fragment([0.9, 0.58, 0.76], text, 92, true)


func _make_unresolved_fragment(line_widths: Array, tag_text: String, min_height: int, show_tag: bool) -> Control:
	var panel := PanelContainer.new()
	panel.custom_minimum_size = Vector2(0, min_height)
	panel.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	panel.clip_contents = true
	panel.add_theme_stylebox_override("panel", _make_unresolved_style(0.96))

	var margin := MarginContainer.new()
	margin.add_theme_constant_override("margin_left", 14)
	margin.add_theme_constant_override("margin_top", 12)
	margin.add_theme_constant_override("margin_right", 14)
	margin.add_theme_constant_override("margin_bottom", 12)
	panel.add_child(margin)

	var stack := VBoxContainer.new()
	stack.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	stack.add_theme_constant_override("separation", 8)
	margin.add_child(stack)

	var animated_bars: Array[ColorRect] = []
	var top_count := maxi(1, line_widths.size() / 2)
	for index: int in range(top_count):
		var top_bar := _make_blurred_bar(float(line_widths[index]), 12, 0.68 - float(index) * 0.08)
		stack.add_child(top_bar["wrap"])
		animated_bars.append(top_bar["overlay"])

	if show_tag and not tag_text.is_empty():
		stack.add_child(_make_unresolved_tag(tag_text))

	for index: int in range(top_count, line_widths.size()):
		var bottom_bar := _make_blurred_bar(float(line_widths[index]), 10, 0.6 - float(index - top_count) * 0.08)
		stack.add_child(bottom_bar["wrap"])
		animated_bars.append(bottom_bar["overlay"])

	var shimmer_glow := ColorRect.new()
	shimmer_glow.color = Color(0.86, 0.81, 0.9, 0.12 * _unresolved_intensity_scale())
	shimmer_glow.mouse_filter = Control.MOUSE_FILTER_IGNORE
	shimmer_glow.position = Vector2(-112, -4)
	shimmer_glow.size = Vector2(132, float(min_height + 8))
	panel.add_child(shimmer_glow)

	var shimmer := ColorRect.new()
	var shimmer_alpha := 0.28 * _unresolved_intensity_scale()
	shimmer.color = Color(0.92, 0.88, 0.8, shimmer_alpha)
	shimmer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	shimmer.position = Vector2(-96, 0)
	shimmer.size = Vector2(maxf(96.0, float(min_height) * 0.88), float(min_height))
	panel.add_child(shimmer)

	var specks: Array = []
	if UNRESOLVED_PARTICLES_ENABLED:
		var speck_count := _resolve_speck_count(line_widths.size(), show_tag)
		var speck_palette := [
			Color(0.94, 0.9, 0.98, 0.0),
			Color(0.9, 0.85, 0.76, 0.0),
			Color(0.73, 0.66, 0.87, 0.0),
		]
		var vertical_range: int = maxi(28, int(min_height) - 20)
		for speck_index: int in range(speck_count):
			var speck := Panel.new()
			speck.mouse_filter = Control.MOUSE_FILTER_IGNORE
			var speck_size := 1 + ((speck_index + line_widths.size()) % 3)
			speck.custom_minimum_size = Vector2(speck_size, speck_size)
			speck.size = Vector2(speck_size, speck_size)
			speck.position = Vector2(
				18.0 + float((speck_index * 29 + min_height * 3) % 168),
				10.0 + float((speck_index * 17 + line_widths.size() * 11) % vertical_range)
			)
			speck.add_theme_stylebox_override("panel", _make_particle_style(speck_palette[speck_index % speck_palette.size()]))
			panel.add_child(speck)
			specks.append({
				"node": speck,
				"base_position": speck.position,
				"base_scale": 0.82 + 0.08 * float((speck_index + 1) % 4),
				"drift": Vector2(
					2.0 + float((speck_index * 3) % 5),
					-2.0 - float((speck_index * 5) % 5)
				),
				"cycle": 2.8 + float((speck_index * 7) % 17) * 0.18,
				"phase": float(speck_index) * 0.73 + float(min_height) * 0.01,
				"peak_alpha": 0.34 + 0.03 * float(speck_index % 5),
				"color": speck_palette[speck_index % speck_palette.size()],
			})

	_register_unresolved_block(panel, shimmer, shimmer_glow, animated_bars, specks)
	return panel


func _make_blurred_bar(width_ratio: float, height: int, alpha: float) -> Dictionary:
	var wrap := HBoxContainer.new()
	wrap.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	wrap.add_theme_constant_override("separation", 0)

	var bar_shadow := ColorRect.new()
	bar_shadow.custom_minimum_size = Vector2(0, height + 2)
	bar_shadow.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	bar_shadow.size_flags_stretch_ratio = width_ratio
	bar_shadow.color = Color(0.98, 0.97, 1.0, alpha * 0.16)
	wrap.add_child(bar_shadow)

	var gap := Control.new()
	gap.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	gap.size_flags_stretch_ratio = maxf(0.08, 1.0 - width_ratio)
	wrap.add_child(gap)

	var overlay := ColorRect.new()
	overlay.mouse_filter = Control.MOUSE_FILTER_IGNORE
	overlay.color = Color(0.66, 0.61, 0.78, alpha)
	overlay.position = Vector2(0, 1)
	overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	overlay.offset_bottom = -1
	bar_shadow.add_child(overlay)

	return {
		"wrap": wrap,
		"overlay": overlay,
		"base_alpha": alpha,
	}


func _make_unresolved_tag(tag_text: String) -> Control:
	var center_row := HBoxContainer.new()
	center_row.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	center_row.alignment = BoxContainer.ALIGNMENT_CENTER

	var chip := PanelContainer.new()
	chip.add_theme_stylebox_override("panel", _make_focus_chip_style())
	center_row.add_child(chip)

	var chip_margin := MarginContainer.new()
	chip_margin.add_theme_constant_override("margin_left", 12)
	chip_margin.add_theme_constant_override("margin_top", 6)
	chip_margin.add_theme_constant_override("margin_right", 12)
	chip_margin.add_theme_constant_override("margin_bottom", 6)
	chip.add_child(chip_margin)

	var chip_label := Label.new()
	chip_label.text = tag_text
	chip_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	chip_label.add_theme_font_size_override("font_size", 13)
	chip_label.add_theme_color_override("font_color", Color(0.31, 0.23, 0.42, 0.96))
	chip_margin.add_child(chip_label)
	return center_row


func _build_meta_line(dossier: Dictionary) -> String:
	var anketa_lines := _collect_section_texts(dossier, "АНКЕТА")
	for line: String in anketa_lines:
		if line.begins_with("Статус:"):
			return _sanitize_inline_placeholders_for_plain_text(line)
	if anketa_lines.size() > 0:
		return _sanitize_inline_placeholders_for_plain_text(anketa_lines[0])
	return FALLBACK_META_COPY


func _collect_section_texts(dossier: Dictionary, section_title: String) -> Array[String]:
	var results: Array[String] = []
	var sections: Array = dossier.get("sections", [])
	for section_variant in sections:
		if not (section_variant is Dictionary):
			continue
		var section: Dictionary = section_variant
		if str(section.get("title", "")) != section_title:
			continue
		var items: Array = section.get("items", [])
		for item_variant in items:
			if item_variant is Dictionary and str(item_variant.get("type", "")) == "text":
				results.append(str(item_variant.get("text", "")))
	return results


func _count_dossier_entries(dossier: Dictionary) -> int:
	var entry_count := 0
	var sections: Array = dossier.get("sections", [])
	for section_variant in sections:
		if not (section_variant is Dictionary):
			continue
		var section: Dictionary = section_variant
		var items: Variant = section.get("items", [])
		if items is Array:
			entry_count += items.size()
	return entry_count


func _parse_dossier_markdown(raw_text: String) -> Dictionary:
	var sections: Array = []
	var current_section := {}

	for raw_line: String in raw_text.split("\n"):
		var line := raw_line.strip_edges()
		if line.is_empty():
			continue
		if line.begins_with("# "):
			continue
		if line.begins_with("## "):
			current_section = {
				"title": _map_section_name(line.substr(3).strip_edges()),
				"items": [],
			}
			sections.append(current_section)
			continue
		if current_section.is_empty():
			current_section = {
				"title": "АНКЕТА",
				"items": [],
			}
			sections.append(current_section)

		var item := _parse_content_line(line)
		var items: Array = current_section.get("items", [])
		items.append(item)
		current_section["items"] = items
		sections[sections.size() - 1] = current_section

	return {
		"sections": sections,
	}


func _inject_unlocked_fragments(dossier: Dictionary, character_id: String) -> Dictionary:
	var result := dossier.duplicate(true)
	var sections: Array = result.get("sections", []).duplicate(true)
	var unlocked_ids: Array = _get_unlocked_fragment_ids()
	var injected_sections: Array = []

	for fragment_id: String in unlocked_ids:
		var fragment := _text_repository.get_dossier_fragment(fragment_id)
		for target_character_id: String in _resolve_fragment_character_targets(fragment):
			if target_character_id != character_id:
				continue
			injected_sections.append(_make_unlocked_fragment_section(fragment))
			break

	if not injected_sections.is_empty():
		sections = injected_sections + sections
	result["sections"] = sections
	return result


func _make_unlocked_fragment_section(fragment: Dictionary) -> Dictionary:
	var style_id := str(fragment.get("style", "standard")).strip_edges().to_lower()
	var section_style := POST_RUN_STANDARD_STYLE
	if style_id == "weyr_intrusion":
		section_style = POST_RUN_INTRUSION_STYLE
	return {
		"title": POST_RUN_SECTION_TITLE,
		"section_style": section_style,
		"fragment_section": str(fragment.get("section_ru", "")),
		"fragment_title": str(fragment.get("title_ru", "")),
		"items": [{
			"type": "text",
			"text": str(fragment.get("fragment_ru", "")),
		}],
	}


func _resolve_fragment_character_targets(fragment: Dictionary) -> Array:
	var targets: Array = []
	var fragment_character_id := str(fragment.get("character_id", "")).strip_edges().to_lower()
	match fragment_character_id:
		"desmond":
			targets.append("desmond")
		"victoria":
			targets.append("victoria")
		"leonard":
			targets.append("leonard")
		"oktaviy":
			targets.append("oktaviy")
		"octaviy":
			targets.append("oktaviy")
		"desmond_victoria":
			targets.append_array(["desmond", "victoria"])
		"victoria_leonard":
			targets.append_array(["victoria", "leonard"])
	return targets


func _get_unlocked_fragment_ids() -> Array:
	var fragment_ids: Array = []
	if not has_node("/root/GameState"):
		return fragment_ids
	var raw_ids: Variant = GameState.unlocked_dossier_fragment_ids
	if raw_ids is Array:
		for raw_id: Variant in raw_ids:
			var fragment_id := str(raw_id).strip_edges().to_upper()
			if fragment_id.is_empty() or fragment_ids.has(fragment_id):
				continue
			fragment_ids.append(fragment_id)
	return fragment_ids


func _parse_content_line(line: String) -> Dictionary:
	var normalized := line.strip_edges()
	if normalized.begins_with("- "):
		normalized = "— %s" % normalized.substr(2).strip_edges()

	if _is_unresolved_line_token(normalized):
		return {"type": "unresolved_line"}

	var payload: Variant = _extract_unresolved_block_payload(normalized)
	if payload != null:
		var payload_text: String = str(payload)
		if payload_text.is_empty():
			return {"type": "unresolved_block"}
		return {
			"type": "unresolved_focus",
			"text": payload_text,
		}

	return {
		"type": "text",
		"text": normalized,
	}


func _map_section_name(section_name: String) -> String:
	var normalized := section_name.strip_edges().to_upper()
	return str(SECTION_NAME_MAP.get(normalized, normalized))


func _read_text_file(path: String) -> String:
	if path.is_empty():
		push_warning("DossierReviewScreen: dossier data source path is empty")
		return ""
	if not FileAccess.file_exists(path):
		push_warning("DossierReviewScreen: dossier file not found at %s" % path)
		return ""
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_warning("DossierReviewScreen: unable to open dossier file at %s" % path)
		return ""
	var text := file.get_as_text()
	if text.strip_edges().is_empty():
		push_warning("DossierReviewScreen: dossier file is empty at %s" % path)
	return text


func _return_to_previous_scene() -> void:
	var target_scene_path := pending_return_scene_path if not pending_return_scene_path.is_empty() else TITLE_SCENE_PATH
	pending_initial_character_id = ""
	pending_return_scene_path = ""
	get_tree().change_scene_to_file(target_scene_path)


func _has_inline_unresolved(text: String) -> bool:
	return _find_next_unresolved_token(text, 0).get("found", false)


func _make_text_line_with_unresolved(text: String) -> Control:
	var label := RichTextLabel.new()
	label.bbcode_enabled = true
	label.scroll_active = false
	label.fit_content = true
	label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	label.add_theme_font_size_override("normal_font_size", 15)
	label.add_theme_color_override("default_color", Color(0.16, 0.11, 0.08, 0.96))
	label.text = _format_inline_unresolved_bbcode(text)
	return label


func _format_inline_unresolved_bbcode(text: String) -> String:
	var formatted := text
	formatted = _replace_all_unresolved_tokens(formatted, _make_inline_unresolved_markup())
	return formatted


func _sanitize_inline_placeholders_for_plain_text(text: String) -> String:
	var sanitized := _replace_all_unresolved_tokens(text, "")
	return sanitized.strip_edges().replace("  ", " ").replace(" ,", ",").replace(" :", ":")


func _make_inline_unresolved_markup() -> String:
	return "[color=#8675bdcc][i]%s[/i][/color]" % INLINE_UNRESOLVED_MARK


func _is_unresolved_line_token(text: String) -> bool:
	var normalized := _normalize_unresolved_token_text(text)
	return normalized in ["{blured line}", "{blurred line}"]


func _extract_unresolved_block_payload(text: String) -> Variant:
	var normalized := _normalize_unresolved_token_text(text)
	for prefix: String in ["{blured block", "{blurred block"]:
		if not normalized.begins_with(prefix) or not normalized.ends_with("}"):
			continue
		return normalized.trim_prefix(prefix).trim_suffix("}").strip_edges()
	return null


func _normalize_unresolved_token_text(text: String) -> String:
	var normalized := text.strip_edges()
	if normalized.begins_with("\\{"):
		normalized = normalized.substr(1)
	return normalized


func _replace_all_unresolved_tokens(text: String, replacement: String) -> String:
	var formatted := text
	var search_from := 0
	while true:
		var token_data := _find_next_unresolved_token(formatted, search_from)
		if not bool(token_data.get("found", false)):
			break
		var start := int(token_data.get("start", -1))
		var end := int(token_data.get("end", -1))
		if start == -1 or end < start:
			break
		formatted = "%s%s%s" % [
			formatted.substr(0, start),
			replacement,
			formatted.substr(end + 1),
		]
		search_from = start + replacement.length()
	return formatted


func _find_next_unresolved_token(text: String, from_index: int) -> Dictionary:
	var best_start := -1
	var best_prefix := ""
	for prefix: String in ["\\{blurred line", "\\{blured line", "{blurred line", "{blured line", "\\{blurred block", "\\{blured block", "{blurred block", "{blured block"]:
		var start := text.find(prefix, from_index)
		if start == -1:
			continue
		if best_start == -1 or start < best_start:
			best_start = start
			best_prefix = prefix
	if best_start == -1:
		return {"found": false}

	var end := text.find("}", best_start)
	if end == -1:
		return {"found": false}
	if best_prefix.contains("line"):
		var token_text := text.substr(best_start, end - best_start + 1)
		if not _is_unresolved_line_token(token_text):
			return {"found": false}
	return {
		"found": true,
		"start": best_start,
		"end": end,
	}


func _style_tab_button(button: Button, is_active: bool) -> void:
	var normal := StyleBoxFlat.new()
	normal.bg_color = Color(0.28, 0.19, 0.12, 0.98) if is_active else Color(0.145, 0.118, 0.1, 0.9)
	normal.border_width_left = 2
	normal.border_width_top = 2
	normal.border_width_right = 2
	normal.border_width_bottom = 3
	normal.border_color = Color(0.79, 0.6, 0.34, 0.96) if is_active else Color(0.52, 0.39, 0.24, 0.78)
	normal.corner_radius_top_left = 10
	normal.corner_radius_top_right = 10
	normal.corner_radius_bottom_right = 10
	normal.corner_radius_bottom_left = 10
	normal.shadow_color = Color(0.05, 0.01, 0.08, 0.2)
	normal.shadow_size = 7

	var hover := normal.duplicate() as StyleBoxFlat
	hover.bg_color = normal.bg_color.lightened(0.05)

	var pressed := normal.duplicate() as StyleBoxFlat
	pressed.bg_color = normal.bg_color.darkened(0.06)

	button.add_theme_stylebox_override("normal", normal)
	button.add_theme_stylebox_override("hover", hover)
	button.add_theme_stylebox_override("pressed", pressed)
	button.add_theme_stylebox_override("focus", hover)
	button.add_theme_font_size_override("font_size", 15)
	var font_color := Color(0.95, 0.9, 0.82, 1.0) if is_active else Color(0.86, 0.81, 0.74, 0.96)
	button.add_theme_color_override("font_color", font_color)
	button.add_theme_color_override("font_hover_color", font_color)
	button.add_theme_color_override("font_pressed_color", font_color)


func _make_section_panel_style(section_style: String = "default") -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.958, 0.934, 0.878, 0.97)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.67, 0.53, 0.33, 0.72)
	style.corner_radius_top_left = 10
	style.corner_radius_top_right = 10
	style.corner_radius_bottom_right = 10
	style.corner_radius_bottom_left = 10
	style.shadow_color = Color(0.3, 0.16, 0.42, 0.05)
	style.shadow_size = 6
	match section_style:
		POST_RUN_STANDARD_STYLE:
			style.bg_color = Color(0.95, 0.92, 0.98, 0.985)
			style.border_width_left = 2
			style.border_color = Color(0.66, 0.57, 0.82, 0.9)
			style.shadow_color = Color(0.44, 0.3, 0.56, 0.08)
		POST_RUN_INTRUSION_STYLE:
			style.bg_color = Color(0.9, 0.86, 0.96, 0.985)
			style.border_width_left = 3
			style.border_color = Color(0.56, 0.34, 0.76, 0.95)
			style.shadow_color = Color(0.38, 0.18, 0.56, 0.12)
	return style


func _section_header_color(section_style: String) -> Color:
	match section_style:
		POST_RUN_STANDARD_STYLE:
			return Color(0.46, 0.34, 0.58, 0.98)
		POST_RUN_INTRUSION_STYLE:
			return Color(0.48, 0.24, 0.66, 0.98)
		_:
			return Color(0.39, 0.29, 0.18, 0.96)


func _section_accent_color(section_style: String) -> Color:
	match section_style:
		POST_RUN_STANDARD_STYLE:
			return Color(0.46, 0.34, 0.58, 0.92)
		POST_RUN_INTRUSION_STYLE:
			return Color(0.5, 0.24, 0.7, 0.96)
		_:
			return Color(0.39, 0.29, 0.18, 0.96)


func _section_title_color(section_style: String) -> Color:
	match section_style:
		POST_RUN_STANDARD_STYLE:
			return Color(0.24, 0.18, 0.31, 0.98)
		POST_RUN_INTRUSION_STYLE:
			return Color(0.25, 0.12, 0.36, 0.98)
		_:
			return Color(0.16, 0.11, 0.08, 0.96)


func _make_unresolved_style(opacity: float) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.84, 0.81, 0.9, opacity * 0.94)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.7, 0.65, 0.82, 0.74)
	style.corner_radius_top_left = 9
	style.corner_radius_top_right = 9
	style.corner_radius_bottom_right = 9
	style.corner_radius_bottom_left = 9
	style.shadow_color = Color(0.44, 0.3, 0.56, 0.09)
	style.shadow_size = 6
	return style


func _make_focus_chip_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.93, 0.91, 0.97, 0.9)
	style.border_width_left = 1
	style.border_width_top = 1
	style.border_width_right = 1
	style.border_width_bottom = 1
	style.border_color = Color(0.69, 0.61, 0.82, 0.84)
	style.corner_radius_top_left = 7
	style.corner_radius_top_right = 7
	style.corner_radius_bottom_right = 7
	style.corner_radius_bottom_left = 7
	return style


func _make_particle_style(color: Color) -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = color
	style.corner_radius_top_left = 6
	style.corner_radius_top_right = 6
	style.corner_radius_bottom_right = 6
	style.corner_radius_bottom_left = 6
	style.shadow_color = Color(color.r, color.g, color.b, 0.18)
	style.shadow_size = 2
	return style


func _apply_static_copy() -> void:
	overline_label.text = OVERLINE_COPY
	subtitle_label.text = SUBTITLE_COPY
	nav_hint_label.text = NAV_HINT_COPY
	nav_hint_label.visible = not NAV_HINT_COPY.is_empty()


func _register_unresolved_block(panel: Control, shimmer: ColorRect, shimmer_glow: ColorRect, bars: Array[ColorRect], specks: Array) -> void:
	unresolved_animated_blocks.append({
		"panel": panel,
		"shimmer": shimmer,
		"shimmer_glow": shimmer_glow,
		"bars": bars,
		"specks": specks,
		"seed": float(unresolved_animated_blocks.size()) * 0.91 + 0.37,
	})


func _resolve_speck_count(line_count: int, show_tag: bool) -> int:
	var baseline := UNRESOLVED_PARTICLE_COUNT_MIN + line_count + (1 if show_tag else 0)
	return mini(UNRESOLVED_PARTICLE_COUNT_MAX, maxi(UNRESOLVED_PARTICLE_COUNT_MIN, baseline))


func _unresolved_intensity_scale() -> float:
	return UNRESOLVED_ANIMATION_INTENSITY * (1.85 if UNRESOLVED_DEBUG_ANIMATION else 1.0)


func _process(delta: float) -> void:
	unresolved_animation_time += delta
	for entry_variant in unresolved_animated_blocks:
		if not (entry_variant is Dictionary):
			continue
		var entry: Dictionary = entry_variant
		var panel := entry.get("panel") as Control
		var shimmer := entry.get("shimmer") as ColorRect
		var shimmer_glow := entry.get("shimmer_glow") as ColorRect
		var bars: Array = entry.get("bars", [])
		var specks: Array = entry.get("specks", [])
		var seed := float(entry.get("seed", 0.0))
		if not is_instance_valid(panel) or not is_instance_valid(shimmer) or not is_instance_valid(shimmer_glow):
			continue

		var intensity := _unresolved_intensity_scale()
		var breath_cycle := 4.3 if not UNRESOLVED_DEBUG_ANIMATION else 3.0
		var pulse_mix := 0.5 + 0.5 * sin(TAU * ((unresolved_animation_time / breath_cycle) + seed * 0.19))
		var pulse := lerpf(0.7, 1.0, pulse_mix)
		if pulse_mix < 0.36:
			pulse = lerpf(pulse, 0.78, (0.36 - pulse_mix) / 0.36)
		panel.self_modulate = Color(1.0, 1.0, 1.0, pulse)

		var shimmer_cycle := 4.6 if not UNRESOLVED_DEBUG_ANIMATION else 3.2
		var shimmer_move_duration := 3.35 if not UNRESOLVED_DEBUG_ANIMATION else 2.35
		var cycle_time := fposmod(unresolved_animation_time + seed * 0.61, shimmer_cycle)
		var shimmer_visible := cycle_time < shimmer_move_duration
		var shimmer_progress := clampf(cycle_time / shimmer_move_duration, 0.0, 1.0)
		var shimmer_width := clampf(panel.size.x * (0.31 if not UNRESOLVED_DEBUG_ANIMATION else 0.4), 96.0, 220.0)
		shimmer.size.x = shimmer_width
		shimmer_glow.size.x = shimmer_width * 1.72
		shimmer_glow.size.y = panel.size.y + 10.0
		shimmer.size.y = panel.size.y + 2.0
		shimmer.position.x = lerpf(-shimmer.size.x, panel.size.x + shimmer.size.x, shimmer_progress)
		shimmer_glow.position.x = shimmer.position.x - shimmer_glow.size.x * 0.24
		shimmer.position.y = -1.0
		shimmer_glow.position.y = -5.0
		shimmer.rotation = -0.18
		shimmer_glow.rotation = -0.18
		var shimmer_peak := 0.31 * intensity
		var shimmer_mix := sin(shimmer_progress * PI) if shimmer_visible else 0.0
		shimmer.color.a = shimmer_mix * shimmer_peak
		shimmer_glow.color.a = shimmer_mix * (0.14 * intensity)

		var bar_index := 0
		for bar_variant in bars:
			var bar := bar_variant as ColorRect
			if not is_instance_valid(bar):
				bar_index += 1
				continue
			var base_alpha := 0.48 + 0.06 * float(bar_index % 3)
			var bar_cycle := 3.9 + float(bar_index) * 0.42
			var bar_mix := 0.5 + 0.5 * sin(TAU * ((unresolved_animation_time / bar_cycle) + seed * 0.33 + float(bar_index) * 0.11))
			bar.color.a = base_alpha + bar_mix * (0.2 * intensity)
			var brightness := 0.64 + bar_mix * (0.1 * intensity)
			bar.color.r = brightness
			bar.color.g = brightness - 0.05
			bar.color.b = brightness + 0.08
			bar_index += 1

		var speck_index := 0
		for speck_variant in specks:
			if not (speck_variant is Dictionary):
				speck_index += 1
				continue
			var speck_entry: Dictionary = speck_variant
			var speck := speck_entry.get("node") as Control
			if not is_instance_valid(speck):
				speck_index += 1
				continue
			var base_position := speck_entry.get("base_position", Vector2.ZERO) as Vector2
			var drift := speck_entry.get("drift", Vector2(3.0, -3.0)) as Vector2
			var speck_cycle := float(speck_entry.get("cycle", 3.6))
			var speck_phase := float(speck_entry.get("phase", 0.0))
			var peak_alpha := float(speck_entry.get("peak_alpha", 0.42)) * intensity
			var base_color := speck_entry.get("color", Color(0.94, 0.9, 0.98, 0.0)) as Color
			var base_scale := float(speck_entry.get("base_scale", 0.9))
			var speck_mix := 0.5 + 0.5 * sin(TAU * ((unresolved_animation_time / speck_cycle) + speck_phase))
			var drift_mix := 0.5 + 0.5 * sin(TAU * ((unresolved_animation_time / (speck_cycle + 1.2)) + speck_phase * 0.7))
			var cross_mix := 0.5 + 0.5 * cos(TAU * ((unresolved_animation_time / (speck_cycle + 0.6)) + speck_phase * 0.4))
			speck.position = base_position + Vector2(
				lerpf(-drift.x, drift.x, drift_mix),
				lerpf(-drift.y, drift.y, cross_mix) * 0.58
			)
			var color := base_color
			color.a = lerpf(0.02, peak_alpha, speck_mix)
			var style := speck.get_theme_stylebox("panel") as StyleBoxFlat
			if style != null:
				style.bg_color = color
				style.shadow_color = Color(color.r, color.g, color.b, color.a * 0.38)
			speck.scale = Vector2.ONE * (base_scale + (speck_mix - 0.5) * 0.32)
			speck_index += 1
