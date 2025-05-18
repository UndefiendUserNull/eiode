extends Control

@export var radius : float = 500

func _ready() -> void:
	circular_positions(get_children() as Array[Node])

func circular_positions(nodes : Array[Node]):
	var positions = []
	var n = nodes.size()

	if n == 0:
		return

	for i in range(0, n):
		var angle = i * (2.0 * PI / n)
		var x = radius * cos(angle)
		var y = radius * sin(angle)	
		positions.append(Vector2(x, y))
		nodes[i].position = positions[i]
