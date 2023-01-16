using Raylib_cs;
using System.Numerics;

Raylib.InitWindow(800, 480, "Hello World");
Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_MSAA_4X_HINT);
Raylib.SetExitKey(KeyboardKey.KEY_ESCAPE);
Rlgl.rlDisableBackfaceCulling();

Vector3[] vertices = new Vector3[]
{
new Vector3( -64, -112, -16),
new Vector3( -64, -112, 192),
new Vector3( -64, 64, -16),
new Vector3( -64, 64, 192),
new Vector3( 128, -112, -16),
new Vector3( 128, -112, 192),
new Vector3( 128, 64, -16),
new Vector3( 128, 64, 192)
};

var mapMesh = new Mesh();

unsafe
{
    mapMesh.vertices = (float*)Raylib.MemAlloc(sizeof(float) * vertices.Count() * 3);
    for (int i = 0; i < vertices.Count(); i += 3)
    {
        mapMesh.vertices[(i * 3)] = vertices[i].X;
        mapMesh.vertices[(i * 3) + 1] = vertices[i].Y;
        mapMesh.vertices[(i * 3) + 2] = vertices[i].Z;
    }

    mapMesh.normals = (float*)Raylib.MemAlloc(sizeof(float) * vertices.Count() * 3);
    for (int i = 0; i < vertices.Count(); i++)
    {
        mapMesh.normals[(i * 3)] = 0;
        mapMesh.normals[(i * 3) + 1] = 1;
        mapMesh.normals[(i * 3) + 2] = 0;
    }

    mapMesh.indices = (ushort*)Raylib.MemAlloc(sizeof(ushort) * vertices.Count() * 3);
    mapMesh.indices[0] = 0;
    mapMesh.indices[1] = 2;
    mapMesh.indices[2] = 1;
}
Raylib.UploadMesh(ref mapMesh, false);


// Camera
float cameraDistance = 100;
Camera3D camera = new Camera3D
{
    position = Vector3.One * cameraDistance,
    target = Vector3.Zero,
    up = Vector3.UnitY,
    fovy = 45,
    projection = CameraProjection.CAMERA_PERSPECTIVE
};

float modelRotationY = 0;
var defaultMaterial = Raylib.LoadMaterialDefault();
Matrix4x4 transform = Matrix4x4.Identity;

var meshCube = Raylib.GenMeshCube(2, 2, 2);
Raylib.UploadMesh(ref meshCube, false);

while (!Raylib.WindowShouldClose())
{
    ProcessInputs();
    transform = Matrix4x4.CreateRotationY(modelRotationY);

    Raylib.UpdateCamera(ref camera);

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.DARKBLUE);

    Raylib.BeginMode3D(camera);

    Raylib.DrawGrid(100, 10);
    Raylib.DrawMesh(meshCube, defaultMaterial, transform);


    Raylib.EndMode3D();

    Raylib.DrawText($"Camera Position: {camera.position}", 0, 0, 20, Color.WHITE);

    Raylib.EndDrawing();
}

Raylib.UnloadMesh(ref mapMesh);
Raylib.CloseWindow();

void ProcessInputs()
{
    if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
    {
        modelRotationY += MathF.PI / 16f;
    }
    if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
    {
        modelRotationY -= MathF.PI / 16f;
    }
    if (Raylib.IsKeyDown(KeyboardKey.KEY_UP))
    {
        cameraDistance += 1;
        camera.position = Vector3.One * cameraDistance;
    }
    if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
    {
        cameraDistance -= 1;
        camera.position = Vector3.One * cameraDistance;
    }
}