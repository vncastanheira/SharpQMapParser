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

float cameraDistance = 300;

Camera3D camera = new Camera3D
{
    position = Vector3.One * cameraDistance,
    target = Vector3.Zero,
    up = Vector3.UnitY,
    fovy = 45,
    projection = CameraProjection.CAMERA_PERSPECTIVE
};

Texture2D texture = Raylib.LoadTexture("city-floor_bricks-1.png");
var meshes = MeshGenerator.GenerateMeshes(map);
List<Model> models;
unsafe
{
    models = meshes.Select(me =>
    {
        var model = Raylib.LoadModelFromMesh(me);
        model.materials[0] = Raylib.LoadMaterialDefault();
        model.materials[0].maps[0].texture = texture;
        return model;
    }).ToList();
}
//var cubeMesh = Raylib.GenMeshCube(1f, 1f, 1f);
//var cubeModel = Raylib.LoadModelFromMesh(cubeMesh);
//unsafe
//{
//    cubeModel.materials[0] = Raylib.LoadMaterialDefault();
//    cubeModel.materials[0].maps[0].texture = texture;
//}
float modelsRotationY = 0f;
float modelsRotationX = 0f;

while (!Raylib.WindowShouldClose())
{
    cameraDistance += Raylib.GetMouseWheelMove();
    camera.position = Vector3.One * cameraDistance;

    if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT))
    {
        modelsRotationY += MathF.PI / 16f;
    }
    if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT))
    {
        modelsRotationY -= MathF.PI / 16f;
    }
    if (Raylib.IsKeyDown(KeyboardKey.KEY_UP))
    {
        modelsRotationX += MathF.PI / 16f;
    }
    if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN))
    {
        modelsRotationX -= MathF.PI / 16f;
    }

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.DARKBLUE);

    Raylib.BeginMode3D(camera);

    Raylib.DrawGrid(10, 1.0f);

    var translation = Matrix4x4.CreateTranslation(Vector3.Zero);
    var yRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, modelsRotationY);
    for (int i = 0; i < models.Count; i++)
    {
        var model = models[i];
        model.transform = Matrix4x4.Transform(translation, yRotation);
        Raylib.DrawModel(models[i], Vector3.Zero, 1f, Color.WHITE);
    }

    Raylib.EndMode3D();

    Raylib.DrawText($"Camera Position: {camera.position}", 0, 0, 20, Color.WHITE);

    Raylib.EndDrawing();
}

for (int i = 0; i < models.Count; i++)
{
    Raylib.UnloadModel(models[i]);
}

Raylib.CloseWindow();