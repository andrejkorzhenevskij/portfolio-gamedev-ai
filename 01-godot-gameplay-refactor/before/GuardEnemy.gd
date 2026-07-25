extends CharacterBody2D

const SHAPE_SHARD_SCENE := preload("res://scenes/pickups/ShapeShard.tscn")

@export var hp: int = 2
@export var patrol_half_width: float = 50.0
@export var patrol_speed: float = 35.0
@export var attack_cooldown: float = 0.9
@export var attack_knockback_velocity: Vector2 = Vector2(520, -340)

@onready var _body_polygon: Polygon2D = $Polygon2D
@onready var _front_damage_area: Area2D = $FrontDamageArea
@onready var _front_damage_visual: Polygon2D = $FrontDamageArea/Polygon2D

var _patrol_center_x := 0.0
var _patrol_direction := 1.0
var _is_dead := false
var _attack_cooldown_remaining := 0.0
var _front_damage_bodies: Array[CharacterBody2D] = []


func _ready() -> void:
	_patrol_center_x = position.x
	_update_front_direction()


func _physics_process(delta: float) -> void:
	if _is_dead:
		return

	_update_attack_cooldown(delta)
	_patrol()
	move_and_slide()


func take_attack_damage() -> void:
	if _is_dead:
		return

	_take_damage()


func take_stomp_damage() -> void:
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


func _patrol() -> void:
	var left_x := _patrol_center_x - patrol_half_width
	var right_x := _patrol_center_x + patrol_half_width

	if position.x >= right_x:
		_set_direction(-1.0)
	elif position.x <= left_x:
		_set_direction(1.0)

	velocity.x = _patrol_direction * patrol_speed


func _set_direction(direction: float) -> void:
	if is_zero_approx(direction):
		return

	var new_direction: float = sign(direction)
	if is_equal_approx(_patrol_direction, new_direction):
		return

	_patrol_direction = new_direction
	_update_front_direction()


func _update_front_direction() -> void:
	_front_damage_area.position.x = _patrol_direction * 32.0


func _on_stomp_area_body_entered(body: Node2D) -> void:
	if not body.has_method("bounce_from_enemy"):
		return

	var player := body as CharacterBody2D
	if player == null or player.velocity.y <= 0.0:
		return

	var bounce_direction: float = sign(player.velocity.x)
	player.bounce_from_enemy(bounce_direction)
	take_stomp_damage()


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
			player.take_damage(1, global_position, attack_knockback_velocity)
			_attack_cooldown_remaining = attack_cooldown
			_play_attack_feedback()
			return


func _play_attack_feedback() -> void:
	_front_damage_visual.color = Color(1, 0, 0, 0.65)
	_body_polygon.scale = Vector2(1.2, 0.92)
	await get_tree().create_timer(0.12).timeout
	if not is_instance_valid(self):
		return

	_front_damage_visual.color = Color(1, 0, 0, 0.24)
	_body_polygon.scale = Vector2.ONE


func _spawn_shape_shard() -> void:
	var shard := SHAPE_SHARD_SCENE.instantiate()
	var shard_parent := get_parent()
	if shard_parent == null:
		return
	if shard_parent.get_parent() != null:
		shard_parent = shard_parent.get_parent()

	shard_parent.add_child(shard)
	shard.global_position = global_position
