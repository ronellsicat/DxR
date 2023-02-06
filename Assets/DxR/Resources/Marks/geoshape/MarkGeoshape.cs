using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Geometry;
using UnityEngine;

namespace DxR
{
    public class MarkGeoshape : Mark
    {
        /// <summary>
        /// The mesh for our geographic shape
        /// </summary>
        private Mesh geoshapeMesh;
        /// <summary>
        /// The list of vertices in the mesh itself. Vertices are repeated in order to attain proper lighting via vertex normals. The vertex list is stored as follows:
        /// [ {front vertices}{back vertices}{front vertices repeated}{back vertices repeated} , {f}{fr}{b}{br} ,...]
        /// Each large chunk of geoPositions.Length * 4 corresponds to a single polygon as part of the overall multipolygon (if applicable)
        /// </summary>
        private List<Vector3> vertices;
        /// <summary>
        /// A list of triangle indices. Used in conjunction with areTrianglesClockwiseWinding to track whether the trianges are ordered clockwise or anticlockwise. This
        /// is important as the faces are reversed when the depth channel is changed from positive to negative.
        /// </summary>
        private List<int> triangles;
        private bool areTrianglesClockwiseWinding = true;
        /// <summary>
        /// A list of lists of 2D coordinates that defines the border of the geographic regions
        /// </summary>
        private List<List<Vector2>> geoPositions;

        private ChannelEncoding longitudeChannelEncoding;
        private ChannelEncoding latitudeChannelEncoding;


        public MarkGeoshape() : base()
        {
        }

        public override void ResetToDefault()
        {
            base.ResetToDefault();

            if (vertices != null)
            {
                vertices = Enumerable.Repeat(Vector3.zero, vertices.Count).ToList();
                geoshapeMesh.SetVertices(vertices);
            }
        }

        #region Channel value functions

        protected override string GetValueForChannelEncoding(ChannelEncoding channelEncoding, int markIndex)
        {
            if (channelEncoding.fieldDataType != null && channelEncoding.fieldDataType == "spatial")
            {
                // If the channel is either x, y, or z, we can just pass it the centroid of this mark's polygons
                if (channelEncoding.channel == "x" || channelEncoding.channel == "y" || channelEncoding.channel == "z")
                {
                    if (channelEncoding.field.ToLower() == "latitude")
                    {
                        return channelEncoding.scale.ApplyScale(centre.Latitude.ToString());
                    }
                    else if (channelEncoding.field.ToLower() == "longitude")
                    {
                        return channelEncoding.scale.ApplyScale(centre.Longitude.ToString());
                    }
                }
                // Otherwise, if it is a size channel, pass in a value of either just "latitude" or "longituide"
                // A custom function will set the size based on the stored GeoShapeValues array
                else if (channelEncoding.channel == "width" || channelEncoding.channel == "height" || channelEncoding.channel == "depth")
                {
                    SetSpatialChannelEncoding(channelEncoding.field.ToLower(), channelEncoding);
                    return channelEncoding.field.ToLower();
                }
            }

            return base.GetValueForChannelEncoding(channelEncoding, markIndex);
        }

        public override void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "length":
                    throw new Exception("Length for GeoShapes is not yet implemented.");
                case "width":
                    SetSize(value, 0);
                    break;
                case "height":
                    SetSize(value, 1);
                    break;
                case "depth":
                    SetSize(value, 2);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }

        public void SetSpatialChannelEncoding(string field, ChannelEncoding channelEncoding)
        {
            if (field == "longitude")
            {
                longitudeChannelEncoding = channelEncoding;
            }
            else if (field == "latitude")
            {
                latitudeChannelEncoding = channelEncoding;
            }
        }

        private void InitialiseGeoshapeMesh()
        {
            geoshapeMesh = GetComponent<MeshFilter>().mesh;

            vertices = new List<Vector3>();
            triangles = new List<int>();
            geoPositions = new List<List<Vector2>>();
            int vertexIdx = 0;
            areTrianglesClockwiseWinding = true;

            foreach (List<IPosition> polygon in polygons)
            {
                // Create our set of positions along a 2D plane
                List<Vector2> polygonGeoPositions = new List<Vector2>();
                foreach (var position in polygon)
                    polygonGeoPositions.Add(new Vector2((float)position.Longitude, (float)position.Latitude));

                // Store this to our total list of lists of vector2s
                geoPositions.Add(polygonGeoPositions);

                // Use these positions to triangulate triangles for our forward and back faces
                Triangulator triangulator = new Triangulator(polygonGeoPositions.ToArray());
                // Triangulate the triangles on this 2D plane
                int[] tris = triangulator.Triangulate();

                // Draw our triangles for a 3D mesh
                int polygonPositionCount = polygonGeoPositions.Count;

                // Front vertices
                for (int i = 0; i < tris.Length; i += 3)
                {
                    triangles.Add(vertexIdx + tris[i]);
                    triangles.Add(vertexIdx + tris[i + 1]);
                    triangles.Add(vertexIdx + tris[i + 2]);
                }

                // Back vertices
                for (int i = 0; i < tris.Length; i += 3)
                {
                    triangles.Add(vertexIdx + polygonPositionCount + tris[i + 2]);
                    triangles.Add(vertexIdx + polygonPositionCount + tris[i + 1]);
                    triangles.Add(vertexIdx + polygonPositionCount + tris[i]);
                }

                // Side vertices
                for (int i = 0; i < polygonPositionCount - 1; i++)
                {
                    int v1 = (polygonPositionCount * 2) + vertexIdx + i;
                    int v2 = v1 + 1;
                    int v3 = (polygonPositionCount * 2) + vertexIdx + polygonPositionCount + i;
                    int v4 = v3 + 1;

                    triangles.Add(v1);
                    triangles.Add(v4);
                    triangles.Add(v3);
                    triangles.Add(v1);
                    triangles.Add(v2);
                    triangles.Add(v4);
                }
                // Complete the side vertices where they loop back with the start
                {
                    int v1 = (polygonPositionCount * 2) + vertexIdx + polygonPositionCount - 1;
                    int v2 = (polygonPositionCount * 2) + vertexIdx;
                    int v3 = (polygonPositionCount * 2) + vertexIdx + polygonPositionCount + polygonPositionCount - 1;
                    int v4 = (polygonPositionCount * 2) + vertexIdx + polygonPositionCount;

                    triangles.Add(v1);
                    triangles.Add(v4);
                    triangles.Add(v3);
                    triangles.Add(v1);
                    triangles.Add(v2);
                    triangles.Add(v4);
                }

                vertexIdx += (polygonPositionCount * 4);
            }

            // Create our dummy list of vertices which will then be populated based on the geoPosition lists
            vertices = Enumerable.Repeat(Vector3.zero, vertexIdx).ToList();
            geoshapeMesh.Clear();
            geoshapeMesh.SetVertices(vertices);
            geoshapeMesh.SetIndices(triangles, MeshTopology.Triangles, 0);
        }

        /// <summary>
        /// Sets the size of this mark using either the geometric values provided as part of the polygon, or a specified value in a rectangular fashion.
        ///
        /// If value is either "longitude" or "latitude", will do the former
        /// </summary>
        private void SetSize(string value, int dim)
        {
            if (geoPositions == null)
                InitialiseGeoshapeMesh();

            CalculateSizeVertices(value, dim, ref vertices);

            geoshapeMesh.SetVertices(vertices);
            geoshapeMesh.RecalculateNormals();
            geoshapeMesh.RecalculateBounds();
        }

        private void CalculateSizeVertices(string value, int dim, ref List<Vector3> newVertices)
        {
            // If the value is either longitude or latitude, calculate the vertices based on the longitude/latitude channels
            if (value == "longitude" || value == "latitude")
            {
                if (value == "longitude")
                {
                    float longitudeOffset = float.Parse(longitudeChannelEncoding.scale.ApplyScale(centre.Longitude.ToString())) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;

                    int vertexIdx = 0;
                    foreach (List<Vector2> polygonGeoPositions in geoPositions)
                    {
                        int polygonPositionCount = polygonGeoPositions.Count;

                        for (int i = 0; i < polygonPositionCount; i++)
                        {
                            int v1 = vertexIdx + i;
                            int v2 = v1 + polygonPositionCount;
                            int v3 = v2 + polygonPositionCount;
                            int v4 = v3 + polygonPositionCount;

                            Vector3 vertexFront1 = newVertices[v1];
                            Vector3 vertexBack1 = newVertices[v2];
                            Vector3 vertexFront2 = newVertices[v3];
                            Vector3 vertexBack2 = newVertices[v4];

                            float longitudeValue = float.Parse(longitudeChannelEncoding.scale.ApplyScale(polygonGeoPositions[i].x.ToString())) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
                            longitudeValue -= longitudeOffset;

                            vertexFront1[dim] = longitudeValue;
                            vertexBack1[dim] = longitudeValue;
                            vertexFront2[dim] = longitudeValue;
                            vertexBack2[dim] = longitudeValue;

                            newVertices[v1] = vertexFront1;
                            newVertices[v2] = vertexBack1;
                            newVertices[v3] = vertexFront2;
                            newVertices[v4] = vertexBack2;
                        }

                        vertexIdx += (polygonPositionCount * 4);
                    }

                    // Vector3 localPos = gameObject.transform.localPosition;
                    // localPos[dim] = longitudeOffset;
                    // gameObject.transform.localPosition = localPos;
                }
                else if (value == "latitude")
                {
                    float latitudeOffset = float.Parse(latitudeChannelEncoding.scale.ApplyScale(centre.Latitude.ToString())) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;

                    int vertexIdx = 0;
                    foreach (List<Vector2> polygonGeoPositions in geoPositions)
                    {
                        int polygonPositionCount = polygonGeoPositions.Count;

                        for (int i = 0; i < polygonPositionCount; i++)
                        {
                            int v1 = vertexIdx + i;
                            int v2 = v1 + polygonPositionCount;
                            int v3 = v2 + polygonPositionCount;
                            int v4 = v3 + polygonPositionCount;

                            Vector3 vertexFront1 = newVertices[v1];
                            Vector3 vertexBack1 = newVertices[v2];
                            Vector3 vertexFront2 = newVertices[v3];
                            Vector3 vertexBack2 = newVertices[v4];

                            float latitudeValue = float.Parse(latitudeChannelEncoding.scale.ApplyScale(polygonGeoPositions[i].y.ToString())) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
                            latitudeValue -= latitudeOffset;

                            vertexFront1[dim] = latitudeValue;
                            vertexBack1[dim] = latitudeValue;
                            vertexFront2[dim] = latitudeValue;
                            vertexBack2[dim] = latitudeValue;

                            newVertices[v1] = vertexFront1;
                            newVertices[v2] = vertexBack1;
                            newVertices[v3] = vertexFront2;
                            newVertices[v4] = vertexBack2;
                        }

                        vertexIdx += (polygonPositionCount * 4);
                    }
                }
            }
            else
            {
                // Otherwise, calculate the size based on the float given in the value parameter
                float size = float.Parse(value) * DxR.Vis.SIZE_UNIT_SCALE_FACTOR;
                float halfSize = size / 2f;

                if (dim == 0)
                {
                    // Split the different polygons into two
                    int vertexIdx = 0;
                    foreach (List<Vector2> polygonGeoPositions in geoPositions)
                    {
                        int polygonPositionCount = polygonGeoPositions.Count;
                        int startRange = 0;
                        int endRange = Mathf.FloorToInt(polygonPositionCount * 0.5f);

                        for (int i = 0; i < polygonPositionCount; i++)
                        {
                            int v1 = vertexIdx + i;
                            int v2 = v1 + polygonPositionCount;
                            int v3 = v2 + polygonPositionCount;
                            int v4 = v3 + polygonPositionCount;

                            Vector3 vertexFront1 = newVertices[v1];
                            Vector3 vertexBack1 = newVertices[v2];
                            Vector3 vertexFront2 = newVertices[v3];
                            Vector3 vertexBack2 = newVertices[v4];

                            if (startRange < i && i < endRange)
                            {
                                vertexFront1[dim] = halfSize;
                                vertexBack1[dim] = halfSize;
                                vertexFront2[dim] = halfSize;
                                vertexBack2[dim] = halfSize;
                            }
                            else
                            {
                                vertexFront1[dim] = -halfSize;
                                vertexBack1[dim] = -halfSize;
                                vertexFront2[dim] = -halfSize;
                                vertexBack2[dim] = -halfSize;
                            }

                            newVertices[v1] = vertexFront1;
                            newVertices[v2] = vertexBack1;
                            newVertices[v3] = vertexFront2;
                            newVertices[v4] = vertexBack2;
                        }

                        vertexIdx += (polygonPositionCount * 4);
                    }
                }
                else if (dim == 1)
                {
                    // Split the different polygons into two
                    int vertexIdx = 0;
                    foreach (List<Vector2> polygonGeoPositions in geoPositions)
                    {
                        int polygonPositionCount = polygonGeoPositions.Count;
                        int startRange = Mathf.FloorToInt(polygonPositionCount * 0.25f);
                        int endRange = Mathf.FloorToInt(polygonPositionCount * 0.75f);

                        for (int i = 0; i < polygonPositionCount; i++)
                        {
                            int v1 = vertexIdx + i;
                            int v2 = v1 + polygonPositionCount;
                            int v3 = v2 + polygonPositionCount;
                            int v4 = v3 + polygonPositionCount;

                            Vector3 vertexFront1 = newVertices[v1];
                            Vector3 vertexBack1 = newVertices[v2];
                            Vector3 vertexFront2 = newVertices[v3];
                            Vector3 vertexBack2 = newVertices[v4];

                            if (startRange < i && i < endRange)
                            {
                                vertexFront1[dim] = -halfSize;
                                vertexBack1[dim] = -halfSize;
                                vertexFront2[dim] = -halfSize;
                                vertexBack2[dim] = -halfSize;
                            }
                            else
                            {
                                vertexFront1[dim] = halfSize;
                                vertexBack1[dim] = halfSize;
                                vertexFront2[dim] = halfSize;
                                vertexBack2[dim] = halfSize;
                            }

                            newVertices[v1] = vertexFront1;
                            newVertices[v2] = vertexBack1;
                            newVertices[v3] = vertexFront2;
                            newVertices[v4] = vertexBack2;
                        }

                        vertexIdx += (polygonPositionCount * 4);
                    }
                }
                else if (dim == 2)
                {
                    int vertexIdx = 0;
                    foreach (List<Vector2> polygonGeoPositions in geoPositions)
                    {
                        int polygonPositionCount = polygonGeoPositions.Count;

                        for (int i = 0; i < polygonPositionCount; i++)
                        {
                            int v1 = vertexIdx + i;
                            int v2 = v1 + polygonPositionCount;
                            int v3 = v2 + polygonPositionCount;
                            int v4 = v3 + polygonPositionCount;

                            Vector3 vertexFront1 = newVertices[v1];
                            Vector3 vertexBack1 = newVertices[v2];
                            Vector3 vertexFront2 = newVertices[v3];
                            Vector3 vertexBack2 = newVertices[v4];

                            vertexFront1[dim] = halfSize;
                            vertexBack1[dim] = -halfSize;
                            vertexFront2[dim] = halfSize;
                            vertexBack2[dim] = -halfSize;

                            newVertices[v1] = vertexFront1;
                            newVertices[v2] = vertexBack1;
                            newVertices[v3] = vertexFront2;
                            newVertices[v4] = vertexBack2;
                        }

                        vertexIdx += (polygonPositionCount * 4);
                    }

                    // We also need to flip the winding order of the triangles if the depth (dim = 2) is negative or positive
                    if (float.TryParse(value, out float result))
                    {
                        if ((result >= 0 && !areTrianglesClockwiseWinding) ||
                            (result < 0 && areTrianglesClockwiseWinding))
                        {
                            for(int i = 0; i < triangles.Count; i = i + 3)
                            {
                                int tmp = triangles[i + 1];
                                triangles[i + 1] = triangles[i + 2];
                                triangles[i + 2] = tmp;
                            }

                            geoshapeMesh.SetIndices(triangles, MeshTopology.Triangles, 0);
                            geoshapeMesh.RecalculateNormals();
                            geoshapeMesh.RecalculateBounds();

                            areTrianglesClockwiseWinding = !areTrianglesClockwiseWinding;
                        }
                    }
                }
            }
        }

        #endregion ChannelValueFunctions
    }

    public class Triangulator
    {
        private List<Vector2> m_points;

        public Triangulator(Vector2[] points)
        {
            m_points = new List<Vector2>(points);
        }

        public Triangulator(List<Vector2> points)
        {
            m_points = points;
        }

        public Triangulator(Vector3[] points)
        {
            m_points = points.Select(vertex => new Vector2(vertex.x, vertex.y)).ToList();
        }

        public static bool Triangulate(Vector3[] vertices, int[] indices, int indexOffset = 0, int vertexOffset = 0, int numVertices = 0)
        {
            if(numVertices == 0)
                numVertices = vertices.Length;

            if(numVertices < 3)
                return false;

            var workingIndices = new int[numVertices];
            if(Area(vertices, vertexOffset, numVertices) > 0)
            {
                for(int v = 0; v < numVertices; v++)
                    workingIndices[v] = v;
            }
            else
            {
                for(int v = 0; v < numVertices; v++)
                    workingIndices[v] = (numVertices - 1) - v;
            }

            int nv = numVertices;
            int count = 2 * nv;
            int currentIndex = indexOffset;
            for(int m = 0, v = nv - 1; nv > 2;)
            {
                if(count-- <= 0)
                    return false;

                int u = v;
                if(nv <= u)
                    u = 0;

                v = u + 1;
                if(nv <= v)
                    v = 0;

                int w = v + 1;
                if(nv <= w)
                    w = 0;

                if(Snip(vertices, u, v, w, nv, workingIndices))
                {
                    indices[currentIndex++] = workingIndices[u];
                    indices[currentIndex++] = workingIndices[v];
                    indices[currentIndex++] = workingIndices[w];
                    m++;

                    for(int s = v, t = v + 1; t < nv; s++, t++)
                        workingIndices[s] = workingIndices[t];

                    nv--;
                    count = 2 * nv;
                }
            }

            return true;
        }

        public static float Area(Vector3[] vertices, int vertexOffset = 0, int numVertices = 0)
        {
            if(numVertices == 0)
                numVertices = vertices.Length;

            float area = 0.0f;
            for(int p = vertexOffset + numVertices - 1, q = 0; q < numVertices; p = q++)
                area += vertices[p].x * vertices[q].y - vertices[q].x * vertices[p].y;

            return area * 0.5f;
        }

        private static bool Snip(Vector3[] vertices, int u, int v, int w, int n, int[] workingIndices)
        {
            Vector2 A = vertices[workingIndices[u]];
            Vector2 B = vertices[workingIndices[v]];
            Vector2 C = vertices[workingIndices[w]];

            if(Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;

            for(int p = 0; p < n; p++)
            {
                if((p == u) || (p == v) || (p == w))
                    continue;

                Vector2 P = vertices[workingIndices[p]];

                if(InsideTriangle(A, B, C, P))
                    return false;
            }

            return true;
        }

        public int[] Triangulate()
        {
            var indices = new List<int>();

            int n = m_points.Count;
            if(n < 3)
                return indices.ToArray();

            var V = new int[n];
            if(Area() > 0)
            {
                for(int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for(int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for(int m = 0, v = nv - 1; nv > 2;)
            {
                if(count-- <= 0)
                    return indices.ToArray();

                int u = v;
                if(nv <= u)
                    u = 0;

                v = u + 1;
                if(nv <= v)
                    v = 0;

                int w = v + 1;
                if(nv <= w)
                    w = 0;

                if(Snip(u, v, w, nv, V))
                {
                    int a = V[u];
                    int b = V[v];
                    int c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    m++;

                    for(int s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];

                    nv--;
                    count = 2 * nv;
                }
            }

    //		indices.Reverse();
            return indices.ToArray();
        }

        private float Area()
        {
            int n = m_points.Count;
            float A = 0.0f;
            for(int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = m_points [p];
                Vector2 qval = m_points [q];
                A += pval.x * qval.y - qval.x * pval.y;
            }

            return A * 0.5f;
        }

        private bool Snip(int u, int v, int w, int n, int[] V)
        {
            Vector2 A = m_points[V[u]];
            Vector2 B = m_points[V[v]];
            Vector2 C = m_points[V[w]];

            if(Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;

            for(int p = 0; p < n; p++)
            {
                if((p == u) || (p == v) || (p == w))
                    continue;

                Vector2 P = m_points[V[p]];

                if(InsideTriangle(A, B, C, P))
                    return false;
            }

            return true;
        }

        private static bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax = C.x - B.x;
            float ay = C.y - B.y;
            float bx = A.x - C.x;
            float by = A.y - C.y;
            float cx = B.x - A.x;
            float cy = B.y - A.y;
            float apx = P.x - A.x;
            float apy = P.y - A.y;
            float bpx = P.x - B.x;
            float bpy = P.y - B.y;
            float cpx = P.x - C.x;
            float cpy = P.y - C.y;

            float aCROSSbp = ax * bpy - ay * bpx;
            float cCROSSap = cx * apy - cy * apx;
            float bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }
}