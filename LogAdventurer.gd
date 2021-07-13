extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	rect_size = OS.window_size
	get_viewport().size = rect_size


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
