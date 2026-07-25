extends Control

@onready var casefile: PanelContainer = %CasefilePanel
@onready var toggle_button: Button = %DossierToggleButton

func _ready() -> void:
	_sync_dossier_state()

func _on_dossier_toggle_button_pressed() -> void:
	casefile.visible = not casefile.visible
	_sync_dossier_state()

func _on_casefile_dismiss_button_pressed() -> void:
	casefile.visible = false
	_sync_dossier_state()

func _sync_dossier_state() -> void:
	toggle_button.text = "CLOSE DOSSIER" if casefile.visible else "OPEN DOSSIER"
