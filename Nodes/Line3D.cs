using Godot;

namespace EIODE.Nodes;

// Most of the code are from the Godot raycast line debug drawing "RayCast3D::_update_debug_shape_vertices()"

[GlobalClass]
public partial class Line3D : MeshInstance3D
{
    [Export] public float Thickness { get; set; } = 0.1f;
    [Export] public int Segments { get; set; } = 12;

    public void DrawLine(Vector3 pointA, Vector3 pointB, Color color)
    {
        if (pointA.IsEqualApprox(pointB)) return;

        var immediateMesh = Mesh as ImmediateMesh;
        if (immediateMesh == null)
        {
            immediateMesh = new ImmediateMesh();
            Mesh = immediateMesh;
        }

        // Make sure we have a simple material
        if (MaterialOverride == null || !(MaterialOverride is StandardMaterial3D))
        {
            var mat = new StandardMaterial3D();
            mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded; // Optional: no lighting
            mat.Transparency = BaseMaterial3D.TransparencyEnum.Disabled;
            MaterialOverride = mat;
        }

        // Set the color
        ((StandardMaterial3D)MaterialOverride).AlbedoColor = color;

        immediateMesh.ClearSurfaces();

        immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);

        Vector3 dir = (pointB - pointA).Normalized();
        Vector3 up = Vector3.Up;

        if (Mathf.Abs(dir.Dot(up)) > 0.99f)
            up = Vector3.Right;

        Vector3 right = dir.Cross(up).Normalized();
        Vector3 forward = dir.Cross(right).Normalized();

        float radius = Thickness / 2f;

        Vector3[] circleA = new Vector3[Segments];
        Vector3[] circleB = new Vector3[Segments];

        for (int i = 0; i < Segments; i++)
        {
            float angle = Mathf.Tau * i / Segments;
            Vector3 offset = (right * Mathf.Cos(angle) + forward * Mathf.Sin(angle)) * radius;
            circleA[i] = pointA + offset;
            circleB[i] = pointB + offset;
        }

        for (int i = 0; i < Segments; i++)
        {
            int next = (i + 1) % Segments;

            // Correct winding
            immediateMesh.SurfaceAddVertex(circleA[i]);
            immediateMesh.SurfaceAddVertex(circleB[next]);
            immediateMesh.SurfaceAddVertex(circleB[i]);

            immediateMesh.SurfaceAddVertex(circleA[i]);
            immediateMesh.SurfaceAddVertex(circleA[next]);
            immediateMesh.SurfaceAddVertex(circleB[next]);
        }

        Vector3 capCenterA = pointA;
        for (int i = 0; i < Segments; i++)
        {
            int next = (i + 1) % Segments;
            immediateMesh.SurfaceAddVertex(capCenterA);
            immediateMesh.SurfaceAddVertex(circleA[i]);
            immediateMesh.SurfaceAddVertex(circleA[next]);
        }

        Vector3 capCenterB = pointB;
        for (int i = 0; i < Segments; i++)
        {
            int next = (i + 1) % Segments;
            immediateMesh.SurfaceAddVertex(capCenterB);
            immediateMesh.SurfaceAddVertex(circleB[next]);
            immediateMesh.SurfaceAddVertex(circleB[i]);
        }

        immediateMesh.SurfaceEnd();
    }

    public void Clear()
    {
        if (Mesh is ImmediateMesh immediateMesh)
        {
            immediateMesh.ClearSurfaces();
        }
    }


    public void DrawLineRelative(Vector3 pointA, Vector3 offset, Color color)
    {
        DrawLine(pointA, pointA + offset, color);
    }
}
