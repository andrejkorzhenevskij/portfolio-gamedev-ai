extends Control

var surgery_layer: Control


func setup(layer_owner: Control) -> void:
	surgery_layer = layer_owner


func _draw() -> void:
	if surgery_layer != null and surgery_layer.has_method("_draw_route_layer"):
		surgery_layer._draw_route_layer(self)
