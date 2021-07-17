extends MultiMeshInstance2D
class_name log_line

#onready var quad = preload("res://plane.obj");
# Declare member variables here. Examples:
# var a = 2
# var b = "text"
var id : int
var text_grams
var grams_words = PoolStringArray()
# Called when the node enters the scene tree for the first time.
#func _ready():
# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass

func set_text_shader(log_material):
	material = log_material
	
func set_text(index,grams,grams_types,grams_counts,max_count,line_grams,text_length):
	id = index
	text_grams = line_grams
	multimesh = MultiMesh.new()
	multimesh.mesh = QuadMesh.new()
	multimesh.mesh.size=Vector2(8,-12)
#	multimesh.mesh.size=Vector2(256,144)
	multimesh.color_format=MultiMesh.COLOR_8BIT
	multimesh.custom_data_format=MultiMesh.CUSTOM_DATA_FLOAT
	multimesh.instance_count=text_length + 16
#	print("set count to ",text_length)
	var instance = 0
	var u : float = 0
	var v : float = 0
	var char_pos = Vector2(4,6)
	var char_pos_increment_right = Vector2(8,0)
#	var char_pos_increment_down = Vector2(0,24)
	var line_number = (String(index+1)+" : ").to_ascii()
	line_number = "             ".to_ascii().subarray(0,15 - line_number.size()) + line_number
	for digit in line_number :
		multimesh.set_instance_color(instance,Color.white)
		multimesh.set_instance_transform_2d(instance,Transform2D(0,char_pos))
		digit -= 32
		u = float(digit % 16)/16.0
		v = float(digit / 16)/6.0
		multimesh.set_instance_custom_data(instance,Color(u,v,0,0))
		char_pos+=char_pos_increment_right
		instance+=1
		
	for gram_id in text_grams :
		grams_words.append(grams[gram_id])
		var hue = Color.darkgray
		if grams_types[gram_id] > 0 :
			"""
			var grams_to_right = 0
			var gram_count = grams_counts[gram_id].data_gram_count
			var check_node = grams_counts[0].right
			while check_node.data_gram_id != gram_id :
				if check_node.data_gram_count < gram_count :
					if check_node.right != null :
						check_node = check_node.right
					else :
						break
				elif check_node.data_gram_count > gram_count :
					grams_to_right+=1
					if check_node.right != null :
						grams_to_right += check_node.right.node_weight
					if check_node.left != null :
						check_node = check_node.left
					else :
						break
				elif check_node.data_gram_id < gram_id :
					if check_node.right != null :
						check_node = check_node.right
					else :
						break
				else :
					grams_to_right+=1
					if check_node.right != null :
						grams_to_right += check_node.right.node_weight
					if check_node.left != null :
						check_node = check_node.left
					else :
						break
			if check_node.right != null :
				grams_to_right += check_node.right.node_weight
			#print("evaluated gram brightness - ",gram_id," ",grams_to_right," ",grams_counts[0].node_weight)
			var gram_brightness = 0.5+0.5*float(grams_to_right)/grams_counts[0].node_weight
			hue = Color.from_hsv(float((gram_id*81)%512)/511,1.0,gram_brightness)
			"""
			var h : int = 5381
			for c in grams[gram_id].to_ascii():
				h += (h << 5) + c
			if(h < 0):
				#print_debug("trying to hash gram: '", grams[gram_id],"' got value: ",h)
				h *= -1
			var word_bright =  1.0 - float(grams_counts[gram_id])/float(max_count)
			word_bright *=  word_bright * 0.5
			word_bright += 0.5
			#print(word_bright)
			hue = Color.from_hsv(float(h%256)/255,word_bright*0.5,word_bright )

		for c in grams[gram_id].to_ascii():
			multimesh.set_instance_color(instance,hue)
			if c > 31 :
				c-=32
			else :
				if c == 9 :
					char_pos+=char_pos_increment_right*3
				c = 0
			u = float(c % 16)/16.0
			v = float(c / 16)/6.0
			multimesh.set_instance_custom_data(instance,Color(u,v,0,0))
			multimesh.set_instance_transform_2d(instance,Transform2D(0,char_pos))
			char_pos+=char_pos_increment_right
			instance+=1
	multimesh.visible_instance_count=instance

func highlight(instance):
	instance -= 16
	if instance < 0 :
		return ""
	var actual_characters = 0 
	for gram in grams_words:
		if gram.to_ascii()[0] == 9 :
			instance -= 3
		actual_characters+=gram.length()
		if actual_characters > instance :
			instance = actual_characters - gram.length()
			for i in range(instance,actual_characters):
				var data = multimesh.get_instance_custom_data(i)
				data.b = 0.4
				multimesh.set_instance_custom_data(i,data)
			return gram
			
func highlight_all():
	for instance in range(multimesh.visible_instance_count):
		var data = multimesh.get_instance_custom_data(instance)
		data.b = 0.4
		multimesh.set_instance_custom_data(instance,data)
