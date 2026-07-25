extends TileMapLayer

@export var tile_count: int = 15
@export var tile_size: Vector2i = Vector2i(200, 40)
@export var dark_color: Color = Color(0.18, 0.19, 0.22, 1)
@export var light_color: Color = Color(0.28, 0.29, 0.33, 1)


func _ready() -> void:
	_build_ground_tiles()


func _build_ground_tiles() -> void:
	clear()

	var ground_tile_set := TileSet.new()
	ground_tile_set.tile_size = tile_size

	var atlas := TileSetAtlasSource.new()
	var image := Image.create_empty(tile_size.x * 2, tile_size.y, false, Image.FORMAT_RGBA8)
	image.fill_rect(Rect2i(Vector2i.ZERO, tile_size), dark_color)
	image.fill_rect(Rect2i(Vector2i(tile_size.x, 0), tile_size), light_color)

	atlas.set_texture(ImageTexture.create_from_image(image))
	atlas.set_texture_region_size(tile_size)
	atlas.create_tile(Vector2i(0, 0))
	atlas.create_tile(Vector2i(1, 0))
	ground_tile_set.add_source(atlas, 0)
	tile_set = ground_tile_set

	for cell_x in range(tile_count):
		set_cell(Vector2i(cell_x, 0), 0, Vector2i(cell_x % 2, 0))
