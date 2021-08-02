extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
export var file_name : String

onready var paste_output=$Label
onready var log_file = load("res://log_file.cs")

onready var log_child = log_file.new();
	
func load_log(file = file_name):
	#$LogPane.paste_output = paste_output
	file_name = file
	#$LogPane.loadLog(file_name)
	log_child._open_file(file_name)
	add_child(log_child)
	print("opened ",file_name)
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if $CloseButton.pressed :
		log_child.free_all()
		self.queue_free()
	$HScrollBar.rect_position.y = rect_size.y - 16
	$HScrollBar.rect_size.x = rect_size.x - 16
	$VScrollBar.rect_position.y = 16
	$VScrollBar.rect_position.x = rect_size.x - 16
	$VScrollBar.rect_size.y = rect_size.y - 32
	$CloseButton.rect_position.x = rect_size.x - 16
	$Label.text = "words: "+String(log_child.the_words_data.num_words)+" max < lines/16: "+String(log_child.the_words_data.max_line_count)+" lines: "+String(log_child.num_lines) + "\n"
	var i = log_child.num_lines# - 31;
	if i < 1 :
		i = 1
	while i <= log_child.num_lines:
		$Label.text += log_child.get_line(i)
		for word in log_child.get_line_words(i):
			$Label.text += word + "\n"
		i += 1
		$Label.text += "\n"
