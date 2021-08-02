extends VBoxContainer

var old_x
var old_y
export var log_file_scene = "res://OpenLogFile.tscn"

func _ready():
		rect_size = OS.window_size
		get_viewport().size = rect_size
		old_x = rect_size.x
		old_y = rect_size.y
		$HBoxContainer/OpenFilesButton.open_log_file = load(log_file_scene)
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

"""
wait.. there's already stuff in Godot Engine so I don't have to filter the GUI inputs..	
func _input(event):
	if event is InputEventMouseButton:
		var mouse_at = event.position
		var stack = Array()
		stack.append(self)
		while stack.size() :
			var obj = stack.pop_back()
			var children = obj.get_children()
			if children.size():
				stack.append_array(children)
			else :
				var up_left = obj.rect_global_position
				if mouse_at.x < up_left.x or mouse_at.y < up_left.y :
					continue
				var down_right = up_left + obj.rect_size*obj.rect_scale
				if mouse_at.x > down_right.x or mouse_at.y > down_right.y :
					continue
				obj.call("_input")
"""
