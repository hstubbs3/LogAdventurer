extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
export var file_name : String

# Called when the node enters the scene tree for the first time.
func _ready():
	pass

func load_log(file = file_name):
	file_name = file
	$LogPane.loadLog(file_name)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if $CloseButton.pressed :
		self.queue_free()
	$HScrollBar.rect_position.y = rect_size.y - 16
	$HScrollBar.rect_size.x = rect_size.x - 16
	$VScrollBar.rect_position.y = 16
	$VScrollBar.rect_position.x = rect_size.x - 16
	$VScrollBar.rect_size.y = rect_size.y - 32
	$CloseButton.rect_position.x = rect_size.x - 16
