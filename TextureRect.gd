extends Sprite


# Declare member variables here. Examples:
# var a = 2
# var b = "text"
var HUES : Image


# Called when the node enters the scene tree for the first time.
func _ready():
	HUES = Image.new()
	HUES.create(512,512,false,Image.FORMAT_RGBA8)
	HUES.lock()
	for x in range(512):
		var hue = Color.from_hsv(float((x*81)%512)/511,1.0,0.9)
		for y in range(512): 
			HUES.set_pixel(x,y,hue)
	HUES.unlock()
	texture=ImageTexture.new()
	texture.create_from_image(HUES)



# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass
