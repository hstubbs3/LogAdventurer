extends Control


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
var file_name : String
var gram_count_sort_tree = gram_count_sort_tree_node.new()
var max_count = 1
var grams_counts = Array()
var grams_type = PoolByteArray()
var grams = PoolStringArray()
var grams_index_dict = {}
var gram_lengths = PoolIntArray()
var counts = PoolIntArray()
var lines = Array()
var lines_grams = Array()
var log_material 
var text_font
var view_starts_line = 0
var index = 0
var read_lines_frame = 1
var log_file = null
var stash_delta = 0.0
var view_line_height = 12.0
var view_char_width = 8.0
var view_size = Vector2(0,0)
var dragging = false
var last_drag_position : Vector2
var selected_line = -1
var selected_char = -1
var paste_output

onready var v_scroll = $"../VScrollBar"
onready var h_scroll = $"../HScrollBar"

# Called when the node enters the scene tree for the first time.
func _ready():
	log_material = preload('res://textMaterial-Smooth.tres')
	text_font = preload('res://LogAdventurerMono-8x12.png')
	log_material.set_shader_param("bitmapFont",text_font)
#	var testPhrase = "This log is not a banana"
#	var line = log_line.new()
#	var line_mesh = line.set_text(0,testPhrase)
#	$LogView.add_child(line_mesh)
#	loadLog(file_name)
#	loadLog('res://lorem.txt')


func _input(event):
	if not $"..".visible :
		return
#	print(event.as_text())
	if event is InputEventMouseButton :
		if event.button_index == BUTTON_WHEEL_UP :
			if event.pressed :
				$LogView.rect_position.y += 48 
				view_starts_line -= 4
		elif event.button_index == BUTTON_WHEEL_DOWN :
			if event.pressed :
				$LogView.rect_position.y -= 48 
				view_starts_line += 4
		elif event.button_index == BUTTON_LEFT:
			if not dragging and event.pressed:
				last_drag_position = event.position
			dragging = event.pressed
		elif event.button_index == BUTTON_RIGHT:
			if event.pressed:
				var index_selected = int((event.position.y-92)/view_line_height +view_starts_line)
				if selected_line == index_selected :
					var char_selected = int(event.position.x/view_char_width+v_scroll.value)
					print(char_selected)
					if selected_char == char_selected :
						var string = '\n'
						for gram_id in lines_grams[index_selected]:
							string+=grams[gram_id]
						print(string)
						paste_output.text+=string
						for child in $LogView.get_children():
							if child.id == selected_line :
								child.highlight_all()
								break
					else :
						for child in $LogView.get_children():
							if child.id == selected_line :
								var gram = child.highlight(char_selected)
								if gram :
									paste_output.text+='\n' + gram
								break
						selected_char = char_selected
				else :
					selected_char = -1
					selected_line = index_selected
				
	if event is InputEventMouseMotion and dragging:
		var relative = event.position - last_drag_position
		h_scroll.value -= relative.x
		v_scroll.value -= relative.y
		last_drag_position = event.position
		
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	stash_delta += delta
	rect_size.x = $"..".rect_size.x - 16
	rect_size.y = $"..".rect_size.y - 16

	#name = file_name
	if log_file != null:
		if delta < 0.1 :
			read_lines_frame += 2
		else:
			read_lines_frame -= 1
		print_debug(delta," gonna try to read : ",read_lines_frame," starting at ",index, " current max_count: ",max_count)
		read_log_lines(read_lines_frame)
	if stash_delta < 0.1 : 
		return
	stash_delta -= 0.1
	
	if not $"..".visible :
		return 
		
	if Input.is_action_pressed("zoom_in"):
		rect_scale*=1.1
		view_line_height*=1.1
		view_char_width*=1.1
	elif Input.is_action_pressed("zoom_out"):
		rect_scale/=1.1
		view_line_height/=1.1
		view_char_width/=1.1
	elif Input.is_action_pressed("zoom_reset"):
		rect_scale.x=1
		rect_scale.y=1
		view_line_height=12.0
		view_char_width=8.0
		$LogView.rect_position.x=0
	
	view_starts_line = v_scroll.value
	$LogView.rect_position.x = -1.0 * h_scroll.value
	v_scroll.max_value = index
	$Label.rect_position.y = rect_size.y - 16
	$Label.rect_position.x = rect_size.x*0.95
	$Label.text = String(index)
	
	if Input.is_action_pressed("ui_home"):
		view_starts_line=0
	if Input.is_action_pressed("ui_end"):
		view_starts_line=index - int(float(rect_size.y)/view_line_height)
		
	if Input.is_action_pressed("ui_left"):
		h_scroll.value += 1
	if Input.is_action_pressed("ui_right"):
		h_scroll.value -= 1

	if Input.is_action_pressed("ui_up"):
		#if view_starts_line > 0 : 
			view_starts_line -= 1.0
	elif Input.is_action_pressed("ui_page_up") :
	#	if view_starts_line > 60 :
			view_starts_line -= int(float(rect_size.y)/view_line_height)
	elif Input.is_action_pressed("ui_down"):
	#	if view_starts_line + 60 < lines.size():
			view_starts_line += 1
	elif Input.is_action_pressed("ui_page_down"):
	#	if view_starts_line + 60 < lines.size():
			view_starts_line += int(float(rect_size.y)/view_line_height)
	$LogView.rect_position.y = -12.0*view_starts_line
#	print("view set to start at - ",view_starts_line)
	
	v_scroll.value = view_starts_line
	$LogView.rect_position.x= -1.0*view_char_width*h_scroll.value

	var lines_screen = int(float(rect_size.y)/view_line_height) 
	while $LogView.get_child_count() > 7 * lines_screen:
		$LogView.remove_child($LogView.get_child(0))
		#print("removing child")
	var lines_in_view = $LogView.get_children()
	for line_number in range(int(max(0,view_starts_line)),int(min(lines.size(),view_starts_line + lines_screen))):
		var need = true
		for line_in_view in lines_in_view :
			if line_in_view.id == line_number :
				need = false
				break
		if need :
			#print("adding child ",line_number)
			var line = log_line.new()
			line.set_text_shader(log_material)
			line.set_text(line_number,grams,grams_type,grams_counts,max_count,lines_grams[line_number],lines[line_number])
			line.translate(Vector2(0,line_number * 12))		
			$LogView.add_child(line)
#	$Label.text = String(OS.window_size)+" "+String(rect_size)+" "+String(rect_scale)
	
func gramify_string(text):
	var text_grams = PoolIntArray()
	var i = -1
	var last_end = -1
	var gram_ends = PoolIntArray()
	var is_alphanum = PoolByteArray()
	var text_bytes = text#.to_ascii()		 
	#print(text)
	for c in text_bytes:
		i+=1
		if c > 47 :
			if c < 58 : # numbers
				continue
			if c > 64 :
				if c < 91 : # capital letters
					continue
				if c > 96 :
					if c < 123 : # lowercase letters
						continue
		if last_end < i - 1 :
			gram_ends.append(i-1)
			is_alphanum.append(1)
		gram_ends.append(i)
		is_alphanum.append(0)
		last_end = i
	if last_end < i :
		gram_ends.append(i)	
		is_alphanum.append(1)
	var gram_start = 0 ;
	i = 0;
	for gram_end in gram_ends :
		var gram = text_bytes.subarray(gram_start,gram_end)
		var gram_text = gram.get_string_from_ascii()
		var gram_id =  grams_index_dict.get(gram_text)
		if gram_id == null :
			gram_id = grams.size()
			grams.append(gram_text)
			grams_type.append(is_alphanum[i])
			grams_index_dict[gram_text]=gram_id
			#var new_node = gram_count_sort_tree_node.new()
			#new_node.data_gram_id = gram_id
			#new_node.data_gram_count = 1
			#grams_counts.append(new_node)
			#print("starting insert for new gram: ",gram_id," ",gram_text)
			#gram_count_sort_tree.insert_gram_count_sort_tree_node(new_node)
			#print("finished insert for new gram: ",gram_id," ",gram_text)
			grams_counts.append(1)
		else :
			#grams_counts[gram_id].increment_self_by_one()
			if grams_counts[gram_id] < 100000:
				grams_counts[gram_id]+=1
				if grams_counts[gram_id] > max_count :
					max_count = grams_counts[gram_id]
		text_grams.append(gram_id)
#		print("adding gram - ",gram_id," : ",gram_text)
		gram_start = gram_end + 1
		i+=1
#	print("Line grammed as follows")
#	for gram_id in text_grams :
#		print(gram_id, " : ",grams[gram_id])
	return text_grams
	 
func loadLog(file = file_name):
	file_name = file
	print("loadLog: ",file_name)
	log_file = File.new()
	log_file.open(file, File.READ)
	index = 0
	read_log_lines(read_lines_frame)
	
func read_log_lines(max_lines):
	var end_read_at = index + max_lines
	while not log_file.eof_reached(): # iterate through all lines until the end of file is reached
		var text = PoolByteArray();
		var next_byte = 0
		while (text.size() < 128 or ( next_byte < 127 and next_byte !=32) ) and !log_file.eof_reached():
			next_byte = log_file.get_8();
			text.append(next_byte);
			if next_byte == 10:
				break
		var line_length = text.size()
		lines.append(line_length)
		if line_length > h_scroll.max_value : h_scroll.max_value = line_length
		var line_grams = gramify_string(text)
		lines_grams.append(line_grams)
		index+=1
		if index > end_read_at:
			break
	if index <= end_read_at :
		log_file.close()
		log_file = null
