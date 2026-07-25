extends RefCounted
class_name FinalTextRepository

const FINAL_TEXTS_DIR := "res://narrative/final_texts/"
const SUMMARY_PATH := FINAL_TEXTS_DIR + "final_summary_variants.tsv"
const PRODUCER_NOTES_PATH := FINAL_TEXTS_DIR + "final_producer_notes.tsv"
const STANCE_TEXTS_PATH := FINAL_TEXTS_DIR + "final_stance_texts.tsv"
const ACHIEVEMENTS_PATH := FINAL_TEXTS_DIR + "final_achievements.tsv"
const DOSSIER_FRAGMENTS_PATH := FINAL_TEXTS_DIR + "final_dossier_fragments.tsv"

const FALLBACK_SUMMARY := {
	"id": "fallback_summary",
	"mode": "CLEAN",
	"dominant_zone": "BALANCED",
	"title_ru": "Результаты просмотра",
	"summary_ru": "Итоговый вариант не найден. Материал дошёл до финала, но его текстовая расшифровка пока не подключена.",
}
const FALLBACK_PRODUCER_NOTE := {
	"id": "fallback_producer_note",
	"stance": "PSYCHOLOGIST",
	"mode": "CLEAN",
	"note_ru": "Материал дошёл до финального просмотра, но продюсерская пометка для этого сочетания пока не найдена.",
}
const FALLBACK_STANCE := {
	"stance_id": "PSYCHOLOGIST",
	"label_ru": "Подход не определён",
	"short_ru": "Ваш подход не удалось распознать.",
	"full_ru": "Ваш подход не удалось распознать, поэтому экран использует безопасную текстовую заглушку.",
}
const FALLBACK_ACHIEVEMENT := {
	"achievement_id": "UNRECORDED",
	"title_ru": "Отметка не найдена",
	"description_ru": "Для этой отметки не удалось найти текст в таблице.",
	"trigger_hint": "",
}
const FALLBACK_DOSSIER_FRAGMENT := {
	"fragment_id": "MATERIAL_PRODUCTIVE_PARANOIA",
	"character_id": "octaviy",
	"title_ru": "Материал // продуктивная паранойя",
	"section_ru": "НЕСАНКЦИОНИРОВАННАЯ ПОМЕТКА",
	"button_ru": "Открыть постороннюю пометку",
	"fragment_ru": "В записи проступает не факт, а подозрение: материал ведёт себя так, будто хочет быть прочитанным определённым образом. Эту версию нельзя канонизировать, но её уже нельзя полностью игнорировать.",
	"style": "weyr_intrusion",
}

var _summary_by_key := {}
var _producer_note_by_key := {}
var _stance_by_id := {}
var _achievement_by_id := {}
var _achievement_rows: Array[Dictionary] = []
var _dossier_fragment_by_id := {}


func _init() -> void:
	_load_all()


func get_summary(mode: String, dominant_zone: String) -> Dictionary:
	var key := _summary_key(mode, dominant_zone)
	if _summary_by_key.has(key):
		return _summary_by_key[key]
	push_warning("FinalTextRepository: summary not found for key %s" % key)
	return FALLBACK_SUMMARY.duplicate(true)


func get_producer_note(stance: String, mode: String) -> Dictionary:
	var key := _producer_note_key(stance, mode)
	if _producer_note_by_key.has(key):
		return _producer_note_by_key[key]
	push_warning("FinalTextRepository: producer note not found for key %s" % key)
	return FALLBACK_PRODUCER_NOTE.duplicate(true)


func get_stance_text(stance_id: String) -> Dictionary:
	var key := stance_id.strip_edges().to_upper()
	if _stance_by_id.has(key):
		return _stance_by_id[key]
	push_warning("FinalTextRepository: stance text not found for %s" % key)
	return FALLBACK_STANCE.duplicate(true)


func get_achievement(achievement_id: String) -> Dictionary:
	var key := achievement_id.strip_edges()
	if _achievement_by_id.has(key):
		return _achievement_by_id[key]
	push_warning("FinalTextRepository: achievement not found for %s" % key)
	return FALLBACK_ACHIEVEMENT.duplicate(true)


func get_all_achievements() -> Array[Dictionary]:
	return _achievement_rows.duplicate(true)


func get_dossier_fragment(fragment_id: String) -> Dictionary:
	var key := fragment_id.strip_edges().to_upper()
	if _dossier_fragment_by_id.has(key):
		return _dossier_fragment_by_id[key]
	push_warning("FinalTextRepository: dossier fragment not found for %s" % key)
	return FALLBACK_DOSSIER_FRAGMENT.duplicate(true)


func _load_all() -> void:
	_load_summaries()
	_load_producer_notes()
	_load_stance_texts()
	_load_achievements()
	_load_dossier_fragments()


func _load_summaries() -> void:
	for row: Dictionary in _read_tsv_rows(SUMMARY_PATH):
		var key := _summary_key(str(row.get("mode", "")), str(row.get("dominant_zone", "")))
		if key == "::":
			push_warning("FinalTextRepository: invalid summary row in %s" % SUMMARY_PATH)
			continue
		_summary_by_key[key] = row


func _load_producer_notes() -> void:
	for row: Dictionary in _read_tsv_rows(PRODUCER_NOTES_PATH):
		var key := _producer_note_key(str(row.get("stance", "")), str(row.get("mode", "")))
		if key == "::":
			push_warning("FinalTextRepository: invalid producer note row in %s" % PRODUCER_NOTES_PATH)
			continue
		_producer_note_by_key[key] = row


func _load_stance_texts() -> void:
	for row: Dictionary in _read_tsv_rows(STANCE_TEXTS_PATH):
		var stance_id := str(row.get("stance_id", "")).strip_edges().to_upper()
		if stance_id.is_empty():
			push_warning("FinalTextRepository: invalid stance row in %s" % STANCE_TEXTS_PATH)
			continue
		_stance_by_id[stance_id] = row


func _load_achievements() -> void:
	for row: Dictionary in _read_tsv_rows(ACHIEVEMENTS_PATH):
		var achievement_id := str(row.get("achievement_id", "")).strip_edges()
		if achievement_id.is_empty():
			push_warning("FinalTextRepository: invalid achievement row in %s" % ACHIEVEMENTS_PATH)
			continue
		_achievement_by_id[achievement_id] = row
		_achievement_rows.append(row)


func _load_dossier_fragments() -> void:
	var loaded_count := 0
	for row: Dictionary in _read_tsv_rows(DOSSIER_FRAGMENTS_PATH):
		var fragment_id := str(row.get("fragment_id", "")).strip_edges().to_upper()
		if fragment_id.is_empty():
			push_warning("FinalTextRepository: invalid dossier fragment row in %s" % DOSSIER_FRAGMENTS_PATH)
			continue
		_dossier_fragment_by_id[fragment_id] = row
		loaded_count += 1
	print("[FinalTextRepository] dossier_data_source_path=", DOSSIER_FRAGMENTS_PATH, " loaded_entry_count=", loaded_count)


func _read_tsv_rows(path: String) -> Array[Dictionary]:
	var rows: Array[Dictionary] = []
	if not FileAccess.file_exists(path):
		push_warning("FinalTextRepository: TSV file not found at %s" % path)
		return rows

	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_warning("FinalTextRepository: unable to open TSV file at %s" % path)
		return rows

	if file.eof_reached():
		push_warning("FinalTextRepository: TSV file is empty at %s" % path)
		return rows

	var header_line := file.get_line().strip_edges()
	if header_line.is_empty():
		push_warning("FinalTextRepository: TSV header row is empty at %s" % path)
		return rows

	var headers := header_line.split("\t", false)
	while not file.eof_reached():
		var raw_line := file.get_line()
		if raw_line.strip_edges().is_empty():
			continue

		var columns := raw_line.split("\t", false)
		var row := {}
		for index: int in range(headers.size()):
			var header := str(headers[index]).strip_edges()
			var value := ""
			if index < columns.size():
				value = str(columns[index]).strip_edges()
			row[header] = value
		rows.append(row)

	return rows


func _summary_key(mode: String, dominant_zone: String) -> String:
	return "%s::%s" % [mode.strip_edges().to_upper(), dominant_zone.strip_edges().to_upper()]


func _producer_note_key(stance: String, mode: String) -> String:
	return "%s::%s" % [stance.strip_edges().to_upper(), mode.strip_edges().to_upper()]
