using Raylib_cs;
using SharpQMapParser.Core;
using System.Numerics;

namespace SharpQMapParser.Renderer
{
    public static class MeshGenerator
    {
        public static List<Mesh> GenerateMeshes(Map map)
        {
            var worldspawn = map.Entities.Find(e => e.ClassName == "worldspawn");
            if (worldspawn != null)
            {
                var polygons = new List<Poly>();
                foreach (var brush in worldspawn.Brushes)
                {
                    Vector3 brushMins = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                    Vector3 brushMax = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

                    for (int i = 0; i < brush.Planes.Count - 3; i++)
                    {
                        var polyA = new Poly();

                        for (int j = 0; j < brush.Planes.Count - 2; j++)
                        {
                            var polyB = new Poly();

                            for (int k = 0; k < brush.Planes.Count - 1; k++)
                            {
                                var polyC = new Poly();

                                if (i != j && i != k && j != k)
                                {

                                    GetPointVectorsAndNormal(brush.Planes[i], out float d1, out Vector3 normal_1);
                                    GetPointVectorsAndNormal(brush.Planes[j], out float d2, out Vector3 normal_2);
                                    GetPointVectorsAndNormal(brush.Planes[k], out float d3, out Vector3 normal_3);

                                    bool legal = true;
                                    GetIntersection(normal_1, normal_2, normal_3, d1, d2, d3, out Vector3 newVertex);

                                    //for (int m = 0; m < brush.Planes.Count - 1; m++)
                                    //{
                                    //    GetPointVectorsAndNormal(brush.Planes[m], out float mDist, out Vector3 mNormal);
                                    //    if (Vector3.Dot(mNormal, newVertex) + mDist > 0)
                                    //        legal = false;
                                    //}

                                    if (legal)
                                    {
                                        polyA.Vertices.Add(newVertex);
                                        polyB.Vertices.Add(newVertex);
                                        polyC.Vertices.Add(newVertex);
                                    }
                                }

                                if (polyC.Vertices.Count > 0)
                                    polygons.Add(polyC);
                            }

                            if (polyB.Vertices.Count > 0)
                                polygons.Add(polyB);
                        }

                        if (polyA.Vertices.Count > 0)
                            polygons.Add(polyA);
                    }
                }

                return GenMeshes(polygons);
            }
            else
            {
                throw new Exception("Map is corrupted.");
            }
        }

        static void GetPointVectorsAndNormal(Core.Plane plane, out float distance, out Vector3 normal)
        {
            Point[] points = plane.Points;
            normal = Vector3.Zero;

            Vector3 v0 = new Vector3(points[0].x, points[0].y, points[0].z);
            Vector3 v1 = new Vector3(points[1].x, points[1].y, points[1].z);
            Vector3 v2 = new Vector3(points[2].x, points[2].y, points[2].z);

            var dir = Vector3.Cross((v1 - v0), (v2 - v0));
            normal = Vector3.Normalize(dir);
            distance = Math.Abs(Vector3.Dot(normal, v0));
        }

        static bool GetIntersection(Vector3 normal_1, Vector3 normal_2, Vector3 normal_3, float d1, float d2, float d3, out Vector3 point)
        {
            float denominator = Vector3.Dot(normal_1, Vector3.Cross(normal_2, normal_3));
            if (denominator == 0)
            {
                point = Vector3.Zero;
                return false;
            }

            point = (-d1 * Vector3.Cross(normal_2, normal_3) - d2 * Vector3.Cross(normal_3, normal_1) - d3 * Vector3.Cross(normal_1, normal_2)) / denominator;
            return true;
        }

        static List<Mesh> GenMeshes(List<Poly> polygons)
        {
            List<Mesh> meshes = new List<Mesh>();
            List<Vector3> allVertices = polygons.SelectMany(p => p.Vertices).ToList();

            for (int i = 0; i < allVertices.Count; i += 3)
            {
                var verticesList = allVertices.Skip(i).Take(3).ToList();
                if (verticesList.Count < 3)
                    continue;

                Mesh newMesh = new Mesh();
                newMesh.triangleCount = 1;
                newMesh.vertexCount = 3;
                unsafe
                {

                    // Vertices
                    fixed (float* vertices = new float[9])
                        newMesh.vertices = vertices;

                    newMesh.vertices[0] = verticesList[0].X;
                    newMesh.vertices[1] = verticesList[0].Y;
                    newMesh.vertices[2] = verticesList[0].Z;

                    newMesh.vertices[3] = verticesList[1].X;
                    newMesh.vertices[4] = verticesList[1].Y;
                    newMesh.vertices[5] = verticesList[1].Z;

                    newMesh.vertices[6] = verticesList[2].X;
                    newMesh.vertices[7] = verticesList[2].Y;
                    newMesh.vertices[8] = verticesList[2].Z;

                    // Normals
                    fixed(float* normals = new float[9])
                        newMesh.normals = normals;

                    var normal = Poly.Normal(verticesList[0], verticesList[1], verticesList[2]);

                    newMesh.normals[0] = normal.X;
                    newMesh.normals[1] = normal.Y;
                    newMesh.normals[2] = normal.Z;
                    newMesh.normals[3] = normal.X;
                    newMesh.normals[4] = normal.Y;
                    newMesh.normals[5] = normal.Z;
                    newMesh.normals[6] = normal.X;
                    newMesh.normals[7] = normal.Y;
                    newMesh.normals[8] = normal.Z;

                    // Tex Coords
                    fixed (float* texcoords = new float[6])
                        newMesh.texcoords = texcoords;

                    newMesh.texcoords[0] = 0;
                    newMesh.texcoords[1] = 0;
                    newMesh.texcoords[2] = 0.5f;
                    newMesh.texcoords[3] = 1.0f;
                    newMesh.texcoords[4] = -1;
                    newMesh.texcoords[5] = 0;

                    Raylib.UploadMesh(&newMesh, false);
                }

                meshes.Add(newMesh);
            }

            return meshes;
        }
    }
}
