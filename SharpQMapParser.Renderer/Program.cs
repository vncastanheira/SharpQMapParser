using Raylib_cs;
using SharpQMapParser;
using SharpQMapParser.Renderer;
using System.Numerics;

// Loads map file
string filePath = args[0];
var map = new Map();
using (StreamReader streamReader = File.OpenText(filePath))
{
    map.Parse(streamReader);
}

Raylib.InitWindow(800, 480, "Hello World");
Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT | ConfigFlags.FLAG_MSAA_4X_HINT);
Raylib.SetExitKey(KeyboardKey.KEY_ESCAPE);

float cameraDistance = 10;

Camera3D camera = new Camera3D
{
    position = Vector3.One * cameraDistance,
    target = Vector3.Zero,
    up = Vector3.UnitY,
    fovy = 45,
    projection = CameraProjection.CAMERA_PERSPECTIVE
};


Texture2D texture = Raylib.LoadTexture("city-floor_bricks-1.png");
var mesh = MeshGenerator.GenerateMeshes(map);
Model mapModel = new Model();
unsafe
{
    mapModel.materials = (Material*)Raylib.MemAlloc(sizeof(Material));

    mapModel.meshes = (Mesh*)Raylib.MemAlloc(sizeof(Mesh));
    mapModel.meshes[0] = mesh;
    mapModel.materialCount= 0;
}

float modelRotationY = 0f;

Rlgl.rlDisableBackfaceCulling();


while (!Raylib.WindowShouldClose())
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

    Raylib.UpdateCamera(ref camera);

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.DARKBLUE);

    Raylib.BeginMode3D(camera);

    var translation = Matrix4x4.CreateTranslation(Vector3.Zero);
    var yRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, modelRotationY);

    Raylib.DrawCube(Vector3.Zero, 1, 1, 1, Color.BLUE);
    Raylib.DrawModel(mapModel, Vector3.Zero, 1f, Color.WHITE);

    Raylib.EndMode3D();

    Raylib.DrawText($"Camera Position: {camera.position}", 0, 0, 20, Color.WHITE);

    Raylib.EndDrawing();
}

Raylib.UnloadModel(mapModel);

Raylib.CloseWindow();