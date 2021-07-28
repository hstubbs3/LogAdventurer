extends TextureButton


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

onready var open_log_file = load("res://OpenLogFile-csharp-001.tscn")
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
func _pressed():
	print("pressed")
	var open_file_dialog = FileDialog.new()
	open_file_dialog.mode = FileDialog.MODE_OPEN_FILES
	open_file_dialog.access = FileDialog.ACCESS_FILESYSTEM
	open_file_dialog.connect("file_selected",self,"_open_file")
	open_file_dialog.connect("files_selected",self,"_open_files")
	add_child(open_file_dialog)
	open_file_dialog.popup(Rect2(0,0,500,500))

func _open_file(path):
	var new_open_log_file = open_log_file.instance()
	new_open_log_file.name = path
	$"../../TabContainer".add_child(new_open_log_file)
	$"../../TabContainer".set_tab_title($"../../TabContainer".get_child_count()-1,path)
	new_open_log_file.paste_output = 	$"../../TabContainer/Scratch"
	new_open_log_file.load_log(path)


func _open_files(paths):
	for path in paths:
		_open_file(path)
