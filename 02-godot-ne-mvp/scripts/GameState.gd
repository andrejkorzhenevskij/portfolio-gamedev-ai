extends Node

const PHASE_ORDER := ["F1", "F2", "F3"]
const FILM_METRIC_MAX := 9
const CONTROL_NEXT_MAX := 4
const CHARACTER_METRIC_MAX := 9
const SURGERY_ZONE_ORDER := ["scene", "victoria", "desmond"]
const PHASE_RELEVANT_CHARACTERS := {
	"F1": ["desmond"],
	"F2": ["desmond", "victoria"],
	"F3": ["desmond", "victoria", "leonard"],
}
const TRACE_FRAGMENT_IDS := [
	"DESMOND_FUNCTION",
	"VICTORIA_HUMAN_COST",
	"DESMOND_VICTORIA_PRICE",
	"VICTORIA_LEONARD_OLD_RESCUE",
	"MATERIAL_PRODUCTIVE_PARANOIA",
]
const TRACE_FRAGMENT_TIE_BREAK_ORDER := [
	"DESMOND_VICTORIA_PRICE",
	"VICTORIA_HUMAN_COST",
	"DESMOND_FUNCTION",
	"VICTORIA_LEONARD_OLD_RESCUE",
	"MATERIAL_PRODUCTIVE_PARANOIA",
]
const OUTCOMES := {
	"scene": {
		"ending_id": "12A",
		"badge_id": "escalation",
		"dossier_variant": "A",
		"resolution_label": "SCENE DOMINANT",
		"resolution_summary": "Scene carries the strongest accumulated surgery load and becomes the final consequence carrier.",
	},
	"victoria": {
		"ending_id": "12B",
		"badge_id": "mercy",
		"dossier_variant": "B",
		"resolution_label": "VICTORIA DOMINANT",
		"resolution_summary": "Victoria takes the strongest accumulated surgery load and preserves the run through passage.",
	},
	"desmond": {
		"ending_id": "12C",
		"badge_id": "precision_at_cost",
		"dossier_variant": "C",
		"resolution_label": "DESMOND DOMINANT",
		"resolution_summary": "Desmond carries the strongest accumulated surgery load and keeps the run narrow and controlled.",
	},
	"neutral": {
		"ending_id": "12D",
		"badge_id": "neutralized_split",
		"dossier_variant": "D",
		"resolution_label": "SMOOTHED LINE",
		"resolution_summary": "The freeze was deliberately neutralized instead of routed into a single carrier.",
	},
}

var current_phase := "F1"
var completed_phases: Array[String] = []
var dominant_zone: String = ""
var resolved_outcome: String = ""
var ending_id: String = ""
var current_outcome_id: String = ""
var badge_ids: Array[String] = []
var dossier_variant: String = ""
var resolution_label: String = ""
var resolution_summary: String = ""
var surgery_allocation := {
	"scene": 0,
	"victoria": 0,
	"desmond": 0,
}
var last_allocated_zone: String = ""
var film_depth := 0
var film_oblivion := 1
var film_pressure := 1
var control_next := 2
var desmond_integrity := 5
var desmond_trauma := 0
var victoria_integrity := 5
var victoria_trauma := 0
var leonard_integrity := 5
var leonard_trauma := 0
var gameplay_resume_phase := ""
var gameplay_resume_step_index := 0
var gameplay_resume_label := ""
var snapshot_source_phase := ""
var pending_snapshot_stage := ""
var snapshot_show_explanatory_overlay := false
var latest_outcome_focus := "neutral"
var trace_fragment_scores := {
	"DESMOND_FUNCTION": 0,
	"VICTORIA_HUMAN_COST": 0,
	"DESMOND_VICTORIA_PRICE": 0,
	"VICTORIA_LEONARD_OLD_RESCUE": 0,
	"MATERIAL_PRODUCTIVE_PARANOIA": 0,
}
var last_focus_by_phase := {}
var final_dossier_fragment_id := ""
var unlocked_dossier_fragment_ids: Array[String] = []
var seen_metrics_note := false


func reset_run() -> void:
	var preserved_unlocked_fragment_count := unlocked_dossier_fragment_ids.size()
	current_phase = PHASE_ORDER[0]
	completed_phases.clear()
	dominant_zone = ""
	resolved_outcome = ""
	ending_id = ""
	current_outcome_id = ""
	badge_ids.clear()
	dossier_variant = ""
	resolution_label = ""
	resolution_summary = ""
	surgery_allocation = {
		"scene": 0,
		"victoria": 0,
		"desmond": 0,
	}
	last_allocated_zone = ""
	film_depth = 0
	film_oblivion = 1
	film_pressure = 1
	control_next = 2
	desmond_integrity = 5
	desmond_trauma = 0
	victoria_integrity = 5
	victoria_trauma = 0
	leonard_integrity = 5
	leonard_trauma = 0
	clear_gameplay_resume()
	clear_snapshot_context()
	latest_outcome_focus = "neutral"
	trace_fragment_scores = _make_empty_trace_fragment_scores()
	last_focus_by_phase = {}
	final_dossier_fragment_id = ""
	print("[GameState] reset_run current_phase=", current_phase, " preserved_unlocked_dossier_entries=", preserved_unlocked_fragment_count)


func reset_all_state() -> void:
	reset_run()
	clear_unlocked_dossier_fragments()
	seen_metrics_note = false
	print("[GameState] reset_all_state cleared_unlocked_dossier_entries=true")


func unlock_final_dossier_fragment() -> void:
	var fragment_id := final_dossier_fragment_id.strip_edges().to_upper()
	if fragment_id.is_empty():
		return
	if not unlocked_dossier_fragment_ids.has(fragment_id):
		unlocked_dossier_fragment_ids.append(fragment_id)


func clear_unlocked_dossier_fragments() -> void:
	unlocked_dossier_fragment_ids.clear()


func apply_debug_preset_f2_desmond() -> void:
	reset_run()
	apply_surgery_result("F1", {
		"scene": 0,
		"victoria": 0,
		"desmond": 2,
	})
	clear_gameplay_resume()
	clear_snapshot_context()


func apply_debug_preset_f3_desmond() -> void:
	_apply_debug_preset_f3_base()
	dominant_zone = "desmond"
	current_outcome_id = "12C"
	clear_gameplay_resume()
	clear_snapshot_context()


func apply_debug_preset_f3_burn() -> void:
	_apply_debug_preset_f3_base()
	film_pressure = 3
	film_oblivion = 1
	clear_gameplay_resume()
	clear_snapshot_context()


func apply_debug_preset_f3_oblivion() -> void:
	_apply_debug_preset_f3_base()
	film_pressure = 1
	film_oblivion = 3
	clear_gameplay_resume()
	clear_snapshot_context()


func apply_debug_preset_f3_clean() -> void:
	_apply_debug_preset_f3_base()
	film_pressure = 2
	film_oblivion = 2
	clear_gameplay_resume()
	clear_snapshot_context()


func _apply_debug_preset_f3_base() -> void:
	reset_run()
	apply_surgery_result("F1", {
		"scene": 0,
		"victoria": 0,
		"desmond": 2,
	})
	apply_surgery_result("F2", {
		"scene": 0,
		"victoria": 1,
		"desmond": 1,
	})


func ensure_runtime_phase() -> void:
	if not PHASE_ORDER.has(current_phase):
		current_phase = PHASE_ORDER[0]


func get_phase_index(phase_id: String = current_phase) -> int:
	return PHASE_ORDER.find(phase_id)


func get_next_phase(phase_id: String = current_phase) -> String:
	var phase_index := get_phase_index(phase_id)
	if phase_index == -1:
		return PHASE_ORDER[0]
	if phase_index + 1 >= PHASE_ORDER.size():
		return ""
	return PHASE_ORDER[phase_index + 1]


func apply_surgery_result(phase_id: String, allocation: Dictionary) -> void:
	ensure_runtime_phase()
	if not PHASE_ORDER.has(phase_id):
		push_error("GameState.apply_surgery_result: unknown phase '%s'" % phase_id)
		return

	var previous_phase := current_phase

	var allocation_last_zone := str(allocation.get("_last_allocated_zone", "")).strip_edges().to_lower()
	if SURGERY_ZONE_ORDER.has(allocation_last_zone):
		last_allocated_zone = allocation_last_zone

	var pass_outcome := get_surgery_pass_outcome(allocation)
	current_outcome_id = _resolve_outcome_id_from_allocation(allocation)
	pending_snapshot_stage = phase_id
	latest_outcome_focus = _normalize_snapshot_focus(pass_outcome)
	_record_trace_fragment(phase_id, latest_outcome_focus)
	_accumulate_surgery_allocation(allocation)
	_apply_surgery_metric_deltas(phase_id, allocation, pass_outcome)

	if not completed_phases.has(phase_id):
		completed_phases.append(phase_id)

	var next_phase := get_next_phase(phase_id)
	if next_phase.is_empty():
		current_phase = ""
		resolve_run_from_allocation()
		print("[GameState] surgery_result phase=", phase_id, " previous_phase=", previous_phase, " next_phase=<final> outcome_id=", current_outcome_id, " pass_focus=", pass_outcome)
		return

	current_phase = next_phase
	print("[GameState] surgery_result phase=", phase_id, " previous_phase=", previous_phase, " next_phase=", current_phase, " outcome_id=", current_outcome_id, " pass_focus=", pass_outcome)


func is_run_complete() -> bool:
	return current_phase.is_empty() and not ending_id.is_empty()


func set_dominant_zone(zone_id: String) -> void:
	dominant_zone = zone_id


func set_surgery_allocation(allocation: Dictionary) -> void:
	surgery_allocation = {
		"scene": int(allocation.get("scene", 0)),
		"victoria": int(allocation.get("victoria", 0)),
		"desmond": int(allocation.get("desmond", 0)),
	}


func resolve_f2_outcome() -> void:
	var outcome_key := dominant_zone if OUTCOMES.has(dominant_zone) else _resolve_outcome_key_from_allocation(surgery_allocation)
	resolved_outcome = outcome_key
	_apply_outcome(outcome_key)


func resolve_surgery_allocation() -> void:
	resolve_run_from_allocation()


func resolve_run_from_allocation() -> void:
	var outcome_key := _resolve_outcome_key_from_allocation(surgery_allocation)
	dominant_zone = outcome_key
	resolved_outcome = outcome_key
	_apply_outcome(outcome_key)
	_resolve_final_dossier_fragment()


func apply_film_delta(depth_delta: int, oblivion_delta: int, pressure_delta: int) -> void:
	film_depth = clampi(film_depth + depth_delta, 0, FILM_METRIC_MAX)
	film_oblivion = clampi(film_oblivion + maxi(oblivion_delta, 0), 0, FILM_METRIC_MAX)
	film_pressure = clampi(film_pressure + pressure_delta, 0, FILM_METRIC_MAX)


func apply_control_next_delta(delta: int) -> void:
	control_next = clampi(control_next + delta, 0, CONTROL_NEXT_MAX)


func apply_character_delta(character_id: String, integrity_delta: int, trauma_delta: int) -> void:
	var safe_integrity_delta := mini(integrity_delta, 0)
	var safe_trauma_delta := maxi(trauma_delta, 0)
	match character_id:
		"desmond":
			desmond_integrity = clampi(desmond_integrity + safe_integrity_delta, 0, CHARACTER_METRIC_MAX)
			desmond_trauma = clampi(desmond_trauma + safe_trauma_delta, 0, CHARACTER_METRIC_MAX)
		"victoria":
			victoria_integrity = clampi(victoria_integrity + safe_integrity_delta, 0, CHARACTER_METRIC_MAX)
			victoria_trauma = clampi(victoria_trauma + safe_trauma_delta, 0, CHARACTER_METRIC_MAX)
		"leonard":
			leonard_integrity = clampi(leonard_integrity + safe_integrity_delta, 0, CHARACTER_METRIC_MAX)
			leonard_trauma = clampi(leonard_trauma + safe_trauma_delta, 0, CHARACTER_METRIC_MAX)
		_:
			push_error("GameState.apply_character_delta: unknown character '%s'" % character_id)


func get_surgery_pass_outcome(allocation: Dictionary) -> String:
	return _resolve_outcome_key_from_allocation(allocation)


func set_gameplay_resume(phase_id: String, step_index: int) -> void:
	gameplay_resume_phase = phase_id
	gameplay_resume_step_index = maxi(step_index, 0)
	gameplay_resume_label = ""


func set_gameplay_resume_branch(phase_id: String, label_name: String) -> void:
	gameplay_resume_phase = phase_id
	gameplay_resume_step_index = 0
	gameplay_resume_label = label_name.strip_edges()


func peek_gameplay_resume() -> Dictionary:
	return {
		"phase": gameplay_resume_phase,
		"step_index": gameplay_resume_step_index,
		"label": gameplay_resume_label,
	}


func consume_gameplay_resume() -> Dictionary:
	var resume_data := {
		"phase": gameplay_resume_phase,
		"step_index": gameplay_resume_step_index,
		"label": gameplay_resume_label,
	}
	clear_gameplay_resume()
	return resume_data


func clear_gameplay_resume() -> void:
	gameplay_resume_phase = ""
	gameplay_resume_step_index = 0
	gameplay_resume_label = ""


func set_snapshot_context(source_phase: String, show_explanatory_overlay: bool) -> void:
	snapshot_source_phase = source_phase.strip_edges()
	pending_snapshot_stage = snapshot_source_phase
	snapshot_show_explanatory_overlay = show_explanatory_overlay


func get_snapshot_context() -> Dictionary:
	return {
		"source_phase": snapshot_source_phase,
		"pending_snapshot_stage": pending_snapshot_stage,
		"show_explanatory_overlay": snapshot_show_explanatory_overlay,
		"latest_outcome_focus": latest_outcome_focus,
	}


func clear_snapshot_context() -> void:
	snapshot_source_phase = ""
	pending_snapshot_stage = ""
	snapshot_show_explanatory_overlay = false


func _accumulate_surgery_allocation(allocation: Dictionary) -> void:
	for zone_id: String in ["scene", "victoria", "desmond"]:
		surgery_allocation[zone_id] = int(surgery_allocation.get(zone_id, 0)) + int(allocation.get(zone_id, 0))


func _apply_surgery_metric_deltas(phase_id: String, allocation: Dictionary, pass_outcome: String) -> void:
	var relevant_characters := _get_relevant_characters_for_phase(phase_id)

	match pass_outcome:
		"scene":
			apply_film_delta(1, 0, 1)
			for character_id: String in relevant_characters:
				apply_character_delta(character_id, -1, 0)
		"victoria":
			apply_film_delta(1, 0, 1)
			_apply_character_focus_delta("victoria", int(allocation.get("victoria", 0)))
		"desmond":
			apply_film_delta(1, 0, 1)
			_apply_character_focus_delta("desmond", int(allocation.get("desmond", 0)))
		"neutral":
			apply_film_delta(-1, 1, -1)
			for character_id: String in relevant_characters:
				apply_character_delta(character_id, -1, 0)


func _apply_character_focus_delta(character_id: String, points_to_target: int) -> void:
	if points_to_target <= 0:
		return

	apply_character_delta(character_id, -1 if points_to_target >= 2 else 0, points_to_target)


func _get_relevant_characters_for_phase(phase_id: String) -> Array[String]:
	var characters: Array[String] = []
	var raw_characters: Variant = PHASE_RELEVANT_CHARACTERS.get(phase_id, [])
	if raw_characters is Array:
		for character_id: Variant in raw_characters:
			characters.append(str(character_id))
	return characters


func _resolve_outcome_key_from_allocation(allocation: Dictionary) -> String:
	if _is_explicit_neutral_allocation(allocation):
		return "neutral"

	var scene_points := int(allocation.get("scene", 0))
	var victoria_points := int(allocation.get("victoria", 0))
	var desmond_points := int(allocation.get("desmond", 0))
	var highest_points := maxi(scene_points, maxi(victoria_points, desmond_points))
	var highest_zones := PackedStringArray()

	for zone_id: String in SURGERY_ZONE_ORDER:
		if int(allocation.get(zone_id, 0)) == highest_points:
			highest_zones.append(zone_id)

	if highest_points <= 0:
		return "neutral"
	if highest_zones.size() == 1:
		return highest_zones[0]

	var tie_break_zone := _resolve_tie_break_zone(allocation, highest_zones)
	if not tie_break_zone.is_empty():
		return tie_break_zone
	return highest_zones[0]


func _resolve_outcome_id_from_allocation(allocation: Dictionary) -> String:
	var outcome_key := _resolve_outcome_key_from_allocation(allocation)
	var outcome_data: Dictionary = OUTCOMES.get(outcome_key, OUTCOMES["neutral"])
	return str(outcome_data.get("ending_id", "12D"))


func _apply_outcome(outcome_key: String) -> void:
	var outcome: Dictionary = OUTCOMES.get(outcome_key, {})
	if outcome.is_empty():
		ending_id = ""
		badge_ids.clear()
		dossier_variant = ""
		resolution_label = ""
		resolution_summary = ""
		push_error("GameState._apply_outcome: unknown outcome '%s'" % outcome_key)
		return

	ending_id = outcome["ending_id"]
	badge_ids = [outcome["badge_id"]]
	dossier_variant = outcome["dossier_variant"]
	resolution_label = outcome["resolution_label"]
	resolution_summary = outcome["resolution_summary"]


func _resolve_tie_break_zone(allocation: Dictionary, tied_zones: PackedStringArray) -> String:
	var allocation_last_zone := str(allocation.get("_last_allocated_zone", "")).strip_edges().to_lower()
	if tied_zones.has(allocation_last_zone):
		return allocation_last_zone
	if tied_zones.has(last_allocated_zone):
		return last_allocated_zone
	return ""


func _is_explicit_neutral_allocation(allocation: Dictionary) -> bool:
	return str(allocation.get("_intent", "")).strip_edges().to_lower() == "neutral"


func _normalize_snapshot_focus(focus_id: String) -> String:
	var normalized_focus := focus_id.strip_edges().to_lower()
	match normalized_focus:
		"scene", "victoria", "desmond", "neutral", "mixed":
			return normalized_focus
		_:
			return "neutral"


func _record_trace_fragment(phase_id: String, focus_id: String) -> void:
	var normalized_phase := phase_id.strip_edges().to_upper()
	var normalized_focus := _normalize_snapshot_focus(focus_id)
	last_focus_by_phase[normalized_phase] = normalized_focus

	match normalized_focus:
		"desmond":
			_add_trace_fragment_score("DESMOND_FUNCTION", 1 if normalized_phase != "F3" else 2)
			if normalized_phase in ["F2", "F3"] and _phase_has_focus("F1", "victoria"):
				_add_trace_fragment_score("DESMOND_VICTORIA_PRICE", 1)
			if normalized_phase == "F3" and (_phase_has_focus("F1", "victoria") or _phase_has_focus("F2", "victoria")):
				_add_trace_fragment_score("DESMOND_VICTORIA_PRICE", 2)
		"victoria":
			_add_trace_fragment_score("VICTORIA_HUMAN_COST", 1 if normalized_phase != "F3" else 2)
			if _has_any_focus(["F1", "F2"], "desmond"):
				_add_trace_fragment_score("DESMOND_VICTORIA_PRICE", 2 if normalized_phase == "F3" else 1)
			if normalized_phase == "F3":
				_add_trace_fragment_score("VICTORIA_LEONARD_OLD_RESCUE", 2)
				if _phase_has_focus("F2", "victoria"):
					_add_trace_fragment_score("VICTORIA_LEONARD_OLD_RESCUE", 1)
		"scene":
			_add_trace_fragment_score("MATERIAL_PRODUCTIVE_PARANOIA", 1 if normalized_phase != "F3" else 2)
		"neutral", "mixed":
			_add_trace_fragment_score("MATERIAL_PRODUCTIVE_PARANOIA", 1)

	if normalized_phase == "F3" and normalized_focus == "scene" and _phase_has_focus("F2", "scene"):
		_add_trace_fragment_score("MATERIAL_PRODUCTIVE_PARANOIA", 1)

	print("[GameState] trace phase=", normalized_phase, " focus=", normalized_focus, " scores=", trace_fragment_scores, " last_focus_by_phase=", last_focus_by_phase)


func _resolve_final_dossier_fragment() -> void:
	var highest_score := -1
	var resolved_fragment_id := ""

	for fragment_id: String in TRACE_FRAGMENT_TIE_BREAK_ORDER:
		var score := int(trace_fragment_scores.get(fragment_id, 0))
		if score > highest_score:
			highest_score = score
			resolved_fragment_id = fragment_id

	if highest_score <= 0 or resolved_fragment_id.is_empty():
		resolved_fragment_id = "MATERIAL_PRODUCTIVE_PARANOIA"

	final_dossier_fragment_id = resolved_fragment_id
	unlock_final_dossier_fragment()
	print("[GameState] final dossier fragment=", final_dossier_fragment_id, " scores=", trace_fragment_scores, " ending_id=", ending_id, " dominant_zone=", dominant_zone)


func _add_trace_fragment_score(fragment_id: String, delta: int) -> void:
	if delta <= 0:
		return
	trace_fragment_scores[fragment_id] = int(trace_fragment_scores.get(fragment_id, 0)) + delta


func _phase_has_focus(phase_id: String, focus_id: String) -> bool:
	return str(last_focus_by_phase.get(phase_id.strip_edges().to_upper(), "")) == _normalize_snapshot_focus(focus_id)


func _has_any_focus(phase_ids: Array[String], focus_id: String) -> bool:
	for phase_id: String in phase_ids:
		if _phase_has_focus(phase_id, focus_id):
			return true
	return false


func _make_empty_trace_fragment_scores() -> Dictionary:
	var scores := {}
	for fragment_id: String in TRACE_FRAGMENT_IDS:
		scores[fragment_id] = 0
	return scores
