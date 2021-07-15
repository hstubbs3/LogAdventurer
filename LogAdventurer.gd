extends Control

var old_x
var old_y


func _ready():
		rect_size = OS.window_size
		get_viewport().size = rect_size
		old_x = rect_size.x
		old_y = rect_size.y
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	rect_size = OS.window_size
	get_viewport().size = rect_size

	if old_x != rect_size.x:
		rect_scale.x = float(rect_size.x) / old_x
		old_x = rect_size.x

	if old_y != rect_size.y:
		rect_scale.y = float(old_y) / rect_size.y
		old_y = rect_size.y
	
