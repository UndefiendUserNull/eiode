extends Node3D

@onready var area_3d: Area3D = $Area3D

func _process(_delta: float) -> void:
	var vaggtor = Input.get_vector("right", "left", "up", "down")
	area_3d.position.x += vaggtor.x * 0.2
	area_3d.position.y += vaggtor.y * 0.2

func _on_area_3d_body_entered(body: Node3D) -> void:
	print(body.name)
