extends Node

class_name gram_count_sort_tree_node

var data_gram_id : int = -1
var data_gram_count : int = 0
var node_weight : int = 1
var node_count : int = 0
var left  = null
var right = null
var parent = null

func insert_gram_count_sort_tree_node(orphan):
	#print("inserting gram into count sort tree - ",orphan.data_gram_id, " ", orphan.data_gram_count)
	var check_node = self
	#var depth = -1
	while true :
		#depth+=1
		#print(depth," check_node is: ",check_node.data_gram_id," counts: ",check_node.data_gram_count," / ",orphan.data_gram_count," - ",orphan.data_gram_count < check_node.data_gram_count)
		check_node.node_count+=orphan.node_count
		check_node.node_weight+=orphan.node_weight
		if orphan.data_gram_count < check_node.data_gram_count:
			if check_node.left == null :
				check_node.left = orphan
				orphan.parent = check_node
				break
			else :
				#print("going left A")
				check_node = check_node.left	
		elif orphan.data_gram_count > check_node.data_gram_count:
			if check_node.right == null :
				check_node.right = orphan
				orphan.parent = check_node
				break
			else :
				#print("going right B")
				check_node = check_node.right
		elif orphan.data_gram_id < check_node.data_gram_id:
			if check_node.left == null :
				check_node.left = orphan
				orphan.parent = check_node
				break
			else :
				#print("going left C")
				check_node = check_node.left
		else :
			if check_node.right == null :
				check_node.right = orphan
				orphan.parent = check_node
				break
			else :
				#print("going right D")
				check_node = check_node.right
#	var dead = 0
#	print('going to die now, thanks')
#	var live = 1/dead
	
func increment_self_by_one():
	data_gram_count += 1
	var check_parent = self
	while check_parent != null :
		check_parent.node_count += 1
		check_parent = check_parent.parent
	if right != null and data_gram_count > right.data_gram_count :
		check_parent = self
		while check_parent != null :
			check_parent.node_count -= right.node_count
			check_parent.node_weight -= right.node_weight
			check_parent = check_parent.parent
		var root = self
		while root.parent != null : 
			root = root.parent
		root.insert_gram_count_sort_tree_node(right)
		right = null
