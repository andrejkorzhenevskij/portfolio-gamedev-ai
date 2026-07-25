extends CharacterBody2D

const SHAPE_SHARD_SCENE := preload("res://scenes/pickups/ShapeShard.tscn")

enum State {
	PATROL,
	CHASE,
	SEARCH,
}

@export var hp: int = 2
@export var patrol_left_x: float = 640.0
@export var patrol_right_x: float = 920.0
@export var patrol_speed: float = 80.0
@export var chase_speed: float = 130.0
@export var lose_player_distance: float = 260.0
@export var attack_cooldown: float = 0.7
@export var search_speed: float = 65.0
@export var search_wait_time: float = 1.0
@export var search_radius: float = 200.0

@onready var _body_polygon: Polygon2D = $Polygon2D
@onready var _alert_label: Label = $AlertLabel
@onready var _front_vision: Area2D = $FrontVision
@onready var _back_vision: Area2D = $BackVision
@onready var _front_damage_area: Area2D = $FrontDamageArea
@onready var _front_damage_visual: Polygon2D = $FrontDamageArea/Polygon2D

var _state: State = State.PATROL
var _patrol_direction: float = 1.0
var _is_dead := false
var _edge_message_visible := false
var _is_paused_at_edge := false
var _is_searching := false
var last_seen_player_position := Vector2.ZERO
var _search_left_x := 0.0
var _search_right_x := 0.0
var _search_center_x := 0.0
var _search_target_x := 0.0
var _search_direction: float = 1.0
var _search_phase := 0
var _search_waiting_to_start := false
var _attack_cooldown_remaining := 0.0
var _detected_player: CharacterBody2D = null
var _front_damage_bodies: Array[CharacterBody2D] = []


func _ready() -> void:
	_alert_label.hide()
	_update_vision_direction()


func _physics_process(delta: float) -> void:
	if _is_dead:
		return

	_update_attack_cooldown(delta)

	if _is_paused_at_edge:
		velocity = Vector2.ZERO
		move_and_slide()
		return

	match _state:
		State.PATROL:
			_patrol()
		State.CHASE:
			_chase()
		State.SEARCH:
			_search()

	move_and_slide()


func _patrol() -> void:
	if global_position.x >= patrol_right_x:
		_pause_at_edge_then_turn(-1.0)
	elif global_position.x <= patrol_left_x:
		_pause_at_edge_then_turn(1.0)

	velocity.x = _patrol_direction * patrol_speed


func _chase() -> void:
	if not is_instance_valid(_detected_player):
		_begin_search()
		return

	last_seen_player_position = _detected_player.global_position
	var distance_to_player: float = abs(_detected_player.global_position.x - global_position.x)
	if distance_to_player > lose_player_distance:
		_begin_search()
		return

	_set_direction(sign(_detected_player.global_position.x - global_position.x))
	velocity.x = _patrol_direction * chase_speed


func _search() -> void:
	if _search_waiting_to_start:
		velocity.x = 0.0
		return

	if abs(global_position.x - _search_target_x) <= 6.0:
		_advance_search_phase()
		return

	_search_direction = sign(_search_target_x - global_position.x)
	_set_direction(_search_direction)
	velocity.x = _search_direction * search_speed


func take_stomp_damage() -> void:
	if _is_dead:
		return

	_take_damage()


func take_attack_damage() -> void:
	if _is_dead:
		return

	_take_damage()


func _take_damage() -> void:
	hp -= 1
	if hp <= 0:
		_is_dead = true
		velocity = Vector2.ZERO
		_spawn_shape_shard()
		queue_free()
	else:
		_body_polygon.color = Color(0.55, 0.18, 0.85, 1)


func _on_stomp_area_body_entered(body: Node2D) -> void:
	if not body.has_method("bounce_from_enemy"):
		return

	var player := body as CharacterBody2D
	if player == null or player.velocity.y <= 0.0:
		return

	var bounce_direction: float = sign(player.velocity.x)
	player.bounce_from_enemy(bounce_direction)
	last_seen_player_position = player.global_position
	take_stomp_damage()
	if not _is_dead:
		_begin_search()


func _on_vision_body_entered(body: Node2D) -> void:
	var player := body as CharacterBody2D
	if player == null or not player.has_method("bounce_from_enemy"):
		return

	_detected_player = player
	last_seen_player_position = player.global_position
	_state = State.CHASE
	_is_searching = false
	_search_waiting_to_start = false
	_alert_label.text = "!!"
	_alert_label.show()


func _on_vision_body_exited(body: Node2D) -> void:
	if body == _detected_player:
		_begin_search()


func _return_to_patrol() -> void:
	_detected_player = null
	_is_searching = false
	_search_waiting_to_start = false
	_state = State.PATROL
	_alert_label.hide()


func _set_direction(direction: float) -> void:
	if is_zero_approx(direction):
		return

	var new_direction: float = sign(direction)
	if is_equal_approx(_patrol_direction, new_direction):
		return

	_patrol_direction = new_direction
	_update_vision_direction()


func _update_vision_direction() -> void:
	_front_vision.position.x = _patrol_direction * 120.0
	_back_vision.position.x = -_patrol_direction * 76.0
	_front_damage_area.position.x = _patrol_direction * 76.0


func _pause_at_edge_then_turn(next_direction: float) -> void:
	if _state != State.PATROL or _edge_message_visible:
		return

	_edge_message_visible = true
	_is_paused_at_edge = true
	velocity = Vector2.ZERO
	_alert_label.text = "о, край!"
	_alert_label.show()
	await get_tree().create_timer(1.0).timeout
	if _state == State.PATROL and _alert_label.text == "о, край!":
		_alert_label.hide()
		_set_direction(next_direction)
	_is_paused_at_edge = false
	_edge_message_visible = false


func _begin_search() -> void:
	if _is_searching or _is_dead:
		return

	_detected_player = null
	_is_searching = true
	_search_waiting_to_start = true
	_state = State.SEARCH
	velocity = Vector2.ZERO
	_start_search_after_delay()


func _start_search_after_delay() -> void:
	await get_tree().create_timer(search_wait_time).timeout
	if not _is_searching or _is_dead:
		return

	_alert_label.text = "?"
	_alert_label.show()
	_state = State.SEARCH
	_search_waiting_to_start = false
	_search_center_x = last_seen_player_position.x
	_search_left_x = _search_center_x - search_radius
	_search_right_x = _search_center_x + search_radius
	_search_phase = 0
	_search_target_x = _search_left_x if global_position.x >= _search_center_x else _search_right_x


func _advance_search_phase() -> void:
	match _search_phase:
		0:
			if is_equal_approx(_search_target_x, _search_left_x):
				_alert_label.text = "??"
				_search_target_x = _search_right_x
			else:
				_alert_label.text = "???"
				_search_target_x = _search_left_x
			_search_phase = 1
		1:
			if is_equal_approx(_search_target_x, _search_left_x):
				_alert_label.text = "??"
			else:
				_alert_label.text = "???"
			_search_target_x = _search_center_x
			_search_phase = 2
		_:
			_return_to_patrol()


func _on_front_damage_area_body_entered(body: Node2D) -> void:
	var player := body as CharacterBody2D
	if player == null or not player.has_method("take_damage"):
		return

	if not _front_damage_bodies.has(player):
		_front_damage_bodies.append(player)
	_try_damage_front_targets()


func _on_front_damage_area_body_exited(body: Node2D) -> void:
	var player := body as CharacterBody2D
	if player != null:
		_front_damage_bodies.erase(player)


func _update_attack_cooldown(delta: float) -> void:
	if _attack_cooldown_remaining > 0.0:
		_attack_cooldown_remaining -= delta
		return

	_try_damage_front_targets()


func _try_damage_front_targets() -> void:
	if _attack_cooldown_remaining > 0.0:
		return

	for player in _front_damage_bodies:
		if is_instance_valid(player) and player.has_method("take_damage"):
			player.take_damage(1, global_position)
			_attack_cooldown_remaining = attack_cooldown
			_play_attack_feedback()
			return


func _spawn_shape_shard() -> void:
	var shard := SHAPE_SHARD_SCENE.instantiate()
	get_parent().add_child(shard)
	shard.global_position = global_position


func _play_attack_feedback() -> void:
	_front_damage_visual.color = Color(1, 0, 0, 0.6)
	_body_polygon.scale = Vector2(1.12, 0.92)
	await get_tree().create_timer(0.1).timeout
	if not is_instance_valid(self):
		return

	_front_damage_visual.color = Color(1, 0, 0, 0.22)
	_body_polygon.scale = Vector2.ONE
