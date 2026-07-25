extends CharacterBody2D

signal died(death_position: Vector2)

const SHAPE_SHARD_SCENE := preload("res://scenes/pickups/ShapeShard.tscn")

@export var hp: int = 2
@export var max_hp: int = 2

@onready var _body_polygon: Polygon2D = $Polygon2D

var _is_dead := false
var _state_version := 0


func _ready() -> void:
	add_to_group("enemies")
	add_to_group("damageable")


func take_stomp_damage() -> void:
	_take_damage()


func take_attack_damage() -> void:
	_take_damage()


func _take_damage() -> void:
	if _is_dead:
		return

	hp -= 1
	if hp <= 0:
		_die()
	else:
		_flash_damage()


func _die() -> void:
	_is_dead = true
	_state_version += 1
	velocity = Vector2.ZERO
	died.emit(global_position)
	_spawn_shape_shard()
	queue_free()


func _flash_damage() -> void:
	_body_polygon.color = Color(0.55, 0.18, 0.85, 1)


func _prepare_shape_shard() -> Node2D:
	var shard := SHAPE_SHARD_SCENE.instantiate() as Node2D
	shard.global_position = global_position
	return shard


func _spawn_shape_shard() -> void:
	var shard := _prepare_shape_shard()
	get_parent().add_child(shard)
