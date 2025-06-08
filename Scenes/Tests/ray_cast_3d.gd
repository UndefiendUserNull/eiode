extends RayCast3D

@onready var ray_cast_3d: RayCast3D = $"."
@onready var teleporting_object: Node3D = $"../teleporting_object"
var radius :float = 0.5
var height :float = 2.0

func _process(delta: float) -> void:
	if (ray_cast_3d.is_colliding()):
		var hitpoint = ray_cast_3d.get_collision_point()
		teleporting_object.position = Vector3(hitpoint.x-radius,hitpoint.y-height/2,hitpoint.z-radius)
		queue_free()
