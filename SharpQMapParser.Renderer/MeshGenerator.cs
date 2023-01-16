using Raylib_cs;
using SharpQMapParser.Core;
using System.Numerics;

namespace SharpQMapParser.Renderer
{
    public static class MeshGenerator
    {
        public static Mesh GenerateMeshes(Map map)
        {
            var worldspawn = map.Entities.Find(e => e.ClassName == "worldspawn");
            if (worldspawn != null)
            {
                List<Vector3> allVertices = new List<Vector3>();
                foreach (var brush in worldspawn.Brushes)
                {
                    for (int i = 0; i < brush.Planes.Count; i++)
                    {
                        for (int j = i + 1; j < brush.Planes.Count; j++)
                        {
                            for (int k = j + 1; k < brush.Planes.Count; k++)
                            {
                                
                                var nJnK = Vector3.Cross(brush.Planes[j].Plane.Normal, brush.Planes[k].Plane.Normal);
                                var nKnI = Vector3.Cross(brush.Planes[k].Plane.Normal, brush.Planes[i].Plane.Normal);
                                var nInJ = Vector3.Cross(brush.Planes[i].Plane.Normal, brush.Planes[j].Plane.Normal);

                                if (nJnK.LengthSquared() > 0.0001f &&
                                   nKnI.LengthSquared() > 0.0001f &&
                                   nKnI.LengthSquared() > 0.0001f)
                                {
                                    var quotient = Vector3.Dot(brush.Planes[i].Plane.Normal, nJnK);
                                    if (Math.Abs(quotient) > 0.0001f)
                                    {
                                        quotient = -1 / quotient;
                                        nJnK *= brush.Planes[i].Plane.D;
                                        nKnI *= brush.Planes[j].Plane.D;
                                        nInJ *= brush.Planes[k].Plane.D;
                                        var potentialVertex = nJnK;
                                        potentialVertex += nKnI;
                                        potentialVertex += nInJ;
                                        potentialVertex *= quotient;

                                        allVertices.Add(potentialVertex);
                                        //check if inside, and replace supportingVertexOut if needed
                                        //if (IsPointInsidePlanes(brush.Planes, potentialVertex, 0.01f))
                                        //{
                                        //}
                                    }

                                }
                            }
                        }
                    }
                }

                //using (StreamWriter writer = new StreamWriter(Path.Combine(AppContext.BaseDirectory, "output.txt")))
                //{
                //    foreach (var v in allVertices)
                //    {
                //        writer.WriteLine($"( {v.X} {v.Y} {v.Z})");
                //    }
                //}

                return GenMeshes(allVertices);
            }
            else
            {
                throw new Exception("Map is corrupted.");
            }
        }

        private static bool IsPointInsidePlanes(List<QuakeFace> faces, Vector3 potentialVertex, float margin)
        {
            for (int i = 0; i < faces.Count; i++)
            {
                if ((Vector3.Dot(faces[i].Plane.Normal, potentialVertex) + faces[i].Plane.D) > 0)
                    return false;
            }
            return true;
        }

        //static void GetPointVectorsAndNormal(Core.QuakeFace plane, out float distance, out Vector3 normal)
        //{
        //    var points = plane.Points;
        //    normal = Vector3.Zero;

        //    Vector3 v1 = new Vector3(points[0].X, points[0].Y, points[0].Z);
        //    Vector3 v2 = new Vector3(points[1].X, points[1].Y, points[1].Z);
        //    Vector3 v3 = new Vector3(points[2].X, points[2].Y, points[2].Z);

        //    var dir = Vector3.Cross((v2 - v1), (v3 - v1));
        //    normal = Vector3.Normalize(dir);
        //    distance = Math.Abs(Vector3.Dot(normal, v1));
        //}

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

        static Mesh GenMeshes(List<Vector3> allVertices)
        {
            Mesh newMesh = new Mesh();

            newMesh.vertexCount = allVertices.Count;
            unsafe
            {
                // Vertices
                newMesh.vertices = (float*)Raylib.MemAlloc(sizeof(float) * allVertices.Count * 3);

                // Normals
                newMesh.normals = (float*)Raylib.MemAlloc(sizeof(float) * allVertices.Count * 3);

                // Indices
                newMesh.indices = (ushort*)Raylib.MemAlloc(sizeof(ushort) * allVertices.Count * 3);

                for (int i = 0; i < allVertices.Count; i += 3)
                {
                    newMesh.vertices[i * 3] = allVertices[i].X;
                    newMesh.vertices[(i * 3) + 1] = allVertices[i].Y;
                    newMesh.vertices[(i * 3) + 2] = allVertices[i].Z;

                    var normal = Vector3.Normalize(allVertices[i]);
                    newMesh.normals[(i * 3)] = normal.X;
                    newMesh.normals[(i * 3) + 1] = normal.Y;
                    newMesh.normals[(i * 3) + 2] = normal.Z;


                }

                // Tex Coords
                //newMesh.texcoords = (float*)Raylib.MemAlloc(sizeof(float) * 12);

                //newMesh.texcoords[0] = 0;
                //newMesh.texcoords[1] = 0;
                //newMesh.texcoords[2] = 0.5f;
                //newMesh.texcoords[3] = 1.0f;
                //newMesh.texcoords[4] = -1;
                //newMesh.texcoords[5] = 0;
            }
            newMesh.triangleCount = 12; // ??

            return newMesh;

        }
    }
}
