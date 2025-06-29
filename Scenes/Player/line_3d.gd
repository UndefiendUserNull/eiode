@tool
extends CSGPolygon3D

@export var line_radius : float = 0.1
@export var line_res  : float = 180

func _process(_delta: float) -> void:
    var circle = PackedVector2Array()
    for degree in line_res:
        var x = line_radius * sin(PI * 2.0 * (degree / line_res))
        var y = line_radius * cos(PI * 2.0 * (degree / line_res))
        var coords = Vector2(x, y)
        circle.append(coords)
    polygon = circle



