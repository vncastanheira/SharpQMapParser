using System.Numerics;

namespace SharpQMapParser.Renderer
{
    public class Poly
    {
        public List<Vector3> Vertices = new List<Vector3>();

        public static Vector3 Normal(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return Vector3.Normalize(Vector3.Cross((v1 - v0), (v2 - v0)));
        }
    }
}
