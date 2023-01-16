using Raylib_cs;
using System.Numerics;

namespace SharpQMapParser.Renderer
{
    public class MapModel
    {
        public Model model;

        public void CreateModel(Mesh mesh)
        {
            model = new Model();
            unsafe
            {
                model.materials = (Material*)Raylib.MemAlloc(sizeof(Material));
                model.meshes = (Mesh*)Raylib.MemAlloc(sizeof(Mesh));
                model.meshes[0] = mesh;
                model.materialCount = 0;
            }
        }

        public void Render()
        {
            Raylib.DrawModel(model, Vector3.Zero, 1f, Color.WHITE);
        }

        public void Unload() => Raylib.UnloadModel(model);
    }
}
