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
                List<Vector3> allVertices = new List<Vector3>();
                foreach (var brush in worldspawn.Brushes)
                {
                    for (int i = 0; i < brush.Planes.Count; i++)
                    {
                        GetPointVectorsAndNormal(brush.Planes[i], out float d_i, out Vector3 normal_i);

                        for (int j = i+1; j < brush.Planes.Count; j++)
                        {
                            GetPointVectorsAndNormal(brush.Planes[j], out float d_j, out Vector3 normal_j);

                            for (int k = j+1; k < brush.Planes.Count; k++)
                            {
                                GetPointVectorsAndNormal(brush.Planes[k], out float d_k, out Vector3 normal_k);

                                var nJnK = Vector3.Cross(normal_j, normal_k);
                                var nKnI = Vector3.Cross(normal_k, normal_i);
                                var nInJ = Vector3.Cross(normal_i, normal_j);

                                if(nJnK.LengthSquared() > 0.0001f &&
                                   nKnI.LengthSquared() > 0.0001f &&
                                   nKnI.LengthSquared() > 0.0001f)
                                {
                                    var quotient = Vector3.Dot(normal_i, nJnK);
                                    if(Math.Abs(quotient) > 0.0001f)
                                    {
                                        quotient = -1 / quotient;
                                        nJnK *= d_i;
                                        nKnI *= d_j;
                                        nInJ *= d_k;
                                        var potentialVertex = nJnK;
                                        potentialVertex += nKnI;
                                        potentialVertex += nInJ;
                                        potentialVertex *= quotient;

                                            allVertices.Add(potentialVertex);
                                        //check if inside, and replace supportingVertexOut if needed
                                        if (IsPointInsidePlanes(brush.Planes, potentialVertex, 0.01f))
                                        {
                                        }
                                    }

                                }
                            }
                        }
                    }
                }

                return GenMeshes(allVertices);
            }
            else
            {
                throw new Exception("Map is corrupted.");
            }
        }

        private static bool IsPointInsidePlanes(List<Core.Plane> planes, Vector3 potentialVertex, float margin)
        {
            for (int i = 0; i < planes.Count; i++)
            {
                GetPointVectorsAndNormal(planes[i], out float dist, out Vector3 normal);
                if ((Vector3.Dot(normal, potentialVertex) + dist) > 0)
                    return false;
            }
            return true;
        }

        static void GetPointVectorsAndNormal(Core.Plane plane, out float distance, out Vector3 normal)
        {
            Point[] points = plane.Points;
            normal = Vector3.Zero;

            Vector3 v1 = new Vector3(points[0].x, points[0].y, points[0].z);
            Vector3 v2 = new Vector3(points[1].x, points[1].y, points[1].z);
            Vector3 v3 = new Vector3(points[2].x, points[2].y, points[2].z);

            var dir = Vector3.Cross((v2 - v1), (v3 - v1));
            normal = Vector3.Normalize(dir);
            distance = Math.Abs(Vector3.Dot(normal, v1));
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

        static List<Mesh> GenMeshes(List<Vector3> allVertices)
        {
            List<Mesh> meshes = new List<Mesh>();

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
                    newMesh.vertices = (float*)Raylib.MemAlloc(sizeof(float) * 9);

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
                    newMesh.normals = (float*)Raylib.MemAlloc(sizeof(float) * 9);

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
                    newMesh.texcoords = (float*)Raylib.MemAlloc(sizeof(float) * 6);

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
