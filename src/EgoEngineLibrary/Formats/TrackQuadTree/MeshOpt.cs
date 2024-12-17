using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EgoEngineLibrary.Formats.TrackQuadTree;

// Based on https://github.com/zeux/meshoptimizer/tree/v0.22

public static class MeshOpt
{
    private const int InvalidIndex = -1;
    private const int CacheSizeMax = 16;
    private const int ValenceMax = 8;

    private static readonly VertexScoreTable ScoreTable = new(
        [
            0.0f, 0.779f, 0.791f, 0.789f, 0.981f, 0.843f, 0.726f, 0.847f, 0.882f, 0.867f, 0.799f, 0.642f, 0.613f,
            0.600f, 0.568f, 0.372f, 0.234f
        ],
        [0.0f, 0.995f, 0.713f, 0.450f, 0.404f, 0.059f, 0.005f, 0.147f, 0.006f]);

    [System.Runtime.CompilerServices.InlineArray(CacheSizeMax + 1)]
    private struct ScoreCacheBuffer
    {
        private float _element;
    }
    [System.Runtime.CompilerServices.InlineArray(ValenceMax + 1)]
    private struct ScoreLiveBuffer
    {
        private float _element;
    }
    private readonly struct VertexScoreTable
    {
        public readonly ScoreCacheBuffer Cache;
        public readonly ScoreLiveBuffer Live;

        public VertexScoreTable(ReadOnlySpan<float> cache, ReadOnlySpan<float> live)
        {
            Cache = new ScoreCacheBuffer();
            cache.CopyTo(Cache);
            Live = new ScoreLiveBuffer();
            live.CopyTo(Live);
        }
    }

    private struct TriangleAdjacency
    {
        public int[] Counts;
        public int[] Offsets;
        public int[] Data;
    }

    private static void BuildTriangleAdjacency(ref TriangleAdjacency adjacency, IReadOnlyList<QuadTreeTriangle> triangles, int vertexCount)
    {
        int indexCount = 3 * triangles.Count;

        // allocate arrays
        adjacency.Counts = new int[vertexCount];
        adjacency.Offsets = new int[vertexCount];
        adjacency.Data = new int[indexCount];

        for (int i = 0; i < triangles.Count; ++i)
        {
            var tri = triangles[i];
            Debug.Assert(tri.A < vertexCount);
            adjacency.Counts[tri.A]++;
            Debug.Assert(tri.B < vertexCount);
            adjacency.Counts[tri.B]++;
            Debug.Assert(tri.C < vertexCount);
            adjacency.Counts[tri.C]++;
        }

        // fill offset table
        int offset = 0;
        for (var i = 0; i < vertexCount; ++i)
        {
            adjacency.Offsets[i] = offset;
            offset += adjacency.Counts[i];
        }

        Debug.Assert(offset == indexCount);

        // fill triangle data
        for (var i = 0; i < triangles.Count; ++i)
        {
            var tri = triangles[i];
            adjacency.Data[adjacency.Offsets[tri.A]++] = i;
            adjacency.Data[adjacency.Offsets[tri.B]++] = i;
            adjacency.Data[adjacency.Offsets[tri.C]++] = i;
        }

        // fix offsets that have been disturbed by the previous pass
        for (var i = 0; i < vertexCount; ++i)
        {
            Debug.Assert(adjacency.Offsets[i] >= adjacency.Counts[i]);
            adjacency.Offsets[i] -= adjacency.Counts[i];
        }
    }
    
    private static int GetNextVertexDeadEnd(
        ReadOnlySpan<int> deadEnd,
        ref int deadEndTop,
        ref int inputCursor,
        ReadOnlySpan<int> liveTriangles,
        int vertexCount)
    {
        // check dead-end stack
        while (deadEndTop != 0)
        {
            var vertex = deadEnd[--deadEndTop];

            if (liveTriangles[vertex] > 0)
                return vertex;
        }

        // input order
        while (inputCursor < vertexCount)
        {
            if (liveTriangles[inputCursor] > 0)
                return inputCursor;

            ++inputCursor;
        }

        return InvalidIndex;
    }
    
    private static int GetNextVertexNeighbor(
        ReadOnlySpan<int> nextCandidates,
        ReadOnlySpan<int> liveTriangles,
        ReadOnlySpan<int> cacheTimestamps,
        int timestamp,
        int cacheSize)
    {
        int bestCandidate = InvalidIndex;
        int bestPriority = -1;

        for (var i = 0; i < nextCandidates.Length; ++i)
        {
            var vertex = nextCandidates[i];

            // otherwise we don't need to process it
            if (liveTriangles[vertex] > 0)
            {
                int priority = 0;

                // will it be in cache after fanning?
                if (2 * liveTriangles[vertex] + timestamp - cacheTimestamps[vertex] <= cacheSize)
                {
                    priority = timestamp - cacheTimestamps[vertex]; // position in cache
                }

                if (priority > bestPriority)
                {
                    bestCandidate = vertex;
                    bestPriority = priority;
                }
            }
        }

        return bestCandidate;
    }
    
    private static float VertexScore(in VertexScoreTable table, int cachePosition, int liveTriangles)
    {
        Debug.Assert(cachePosition >= -1 && cachePosition < CacheSizeMax);

        int liveTrianglesClamped = liveTriangles < ValenceMax ? liveTriangles : ValenceMax;

        return table.Cache[1 + cachePosition] + table.Live[liveTrianglesClamped];
    }
    
    private static int GetNextTriangleDeadEnd(ref int inputCursor, ReadOnlySpan<bool> emittedFlags, int faceCount)
    {
        // input order
        while (inputCursor < faceCount)
        {
            if (!emittedFlags[inputCursor])
                return inputCursor;

            ++inputCursor;
        }

        return InvalidIndex;
    }

    public static void OptimizeVertexCache(
        IList<QuadTreeTriangle> destination,
        IReadOnlyList<QuadTreeTriangle> triangles,
        int vertexCount)
    {
        var table = ScoreTable;

        // guard for empty meshes
        if (triangles.Count <= 0 || vertexCount <= 0)
        {
            return;
        }

        // support in-place optimization
        if (ReferenceEquals(destination, triangles))
        {
            triangles = triangles.ToArray();
        }

        const int cacheSize = 16;
        Debug.Assert(cacheSize <= CacheSizeMax);

        // build adjacency information
        TriangleAdjacency adjacency = new();
        BuildTriangleAdjacency(ref adjacency, triangles, vertexCount);

        // live triangle counts; note, we alias adjacency.counts as we remove triangles after emitting them so the counts always match
        int[] liveTriangles = adjacency.Counts;

        // emitted flags
        bool[] emittedFlags = new bool[triangles.Count];

        // compute initial vertex scores
        float[] vertexScores = new float[vertexCount];
        for (var i = 0; i < vertexCount; ++i)
        {
            vertexScores[i] = VertexScore(in table, -1, liveTriangles[i]);
        }

        // compute triangle scores
        float[] triangleScores = new float[triangles.Count];
        for (var i = 0; i < triangles.Count; ++i)
        {
            var tri = triangles[i];
            triangleScores[i] = vertexScores[tri.A] + vertexScores[tri.B] + vertexScores[tri.C];
        }

        int[] cacheHolder = new int[2 * (CacheSizeMax + 4)];
        Span<int> cache = cacheHolder;
        Span<int> cacheNew = cache[(CacheSizeMax + 4)..];
        int cacheCount = 0;

        int currentTriangle = 0;
        int inputCursor = 1;
        int outputTriangle = 0;
        while (currentTriangle != InvalidIndex)
        {
            Debug.Assert(outputTriangle < triangles.Count);
            var t = triangles[currentTriangle];

            // output indices
            destination[outputTriangle] = t;
            outputTriangle++;

            // update emitted flags
            emittedFlags[currentTriangle] = true;
            triangleScores[currentTriangle] = 0;

            // new triangle
            var cacheWrite = 0;
            cacheNew[cacheWrite++] = t.A;
            cacheNew[cacheWrite++] = t.B;
            cacheNew[cacheWrite++] = t.C;

            // old triangles
            for (var i = 0; i < cacheCount; ++i)
            {
                int index = cache[i];
                cacheNew[cacheWrite] = index;
                cacheWrite += Convert.ToInt32((index != t.A) & (index != t.B) & (index != t.C));
            }

            var cacheTemp = cache;
            cache = cacheNew;
            cacheNew = cacheTemp;
            cacheCount = cacheWrite > cacheSize ? cacheSize : cacheWrite;

            // remove emitted triangle from adjacency data
            // this makes sure that we spend less time traversing these lists on subsequent iterations
            // live triangle counts are updated as a byproduct of these adjustments
            for (var k = 0; k < 3; ++k)
            {
                int index = triangles[currentTriangle][k];

                Span<int> neighbors = adjacency.Data.AsSpan()[adjacency.Offsets[index]..];
                var neighborsSize = adjacency.Counts[index];

                for (var i = 0; i < neighborsSize; ++i)
                {
                    int tri = neighbors[i];
                    if (tri != currentTriangle)
                    {
                        continue;
                    }

                    neighbors[i] = neighbors[neighborsSize - 1];
                    adjacency.Counts[index]--;
                    break;
                }
            }

            var bestTriangle = InvalidIndex;
            float bestScore = 0;

            // update cache positions, vertex scores and triangle scores, and find next best triangle
            for (var i = 0; i < cacheWrite; ++i)
            {
                var index = cache[i];

                // no need to update scores if we are never going to use this vertex
                if (adjacency.Counts[index] == 0)
                {
                    continue;
                }

                int cachePosition = i >= cacheSize ? -1 : i;

                // update vertex score
                float score = VertexScore(table, cachePosition, liveTriangles[index]);
                float scoreDiff = score - vertexScores[index];

                vertexScores[index] = score;

                // update scores of vertex triangles
                var neighborsBegin = adjacency.Offsets[index];
                var neighborsEnd = neighborsBegin + adjacency.Counts[index];
                for (var it = neighborsBegin; it < neighborsEnd; ++it)
                {
                    var tri = adjacency.Data[it];
                    Debug.Assert(!emittedFlags[tri]);

                    var triScore = triangleScores[tri] + scoreDiff;
                    Debug.Assert(triScore > 0);

                    bestTriangle = bestScore < triScore ? tri : bestTriangle;
                    bestScore = bestScore < triScore ? triScore : bestScore;

                    triangleScores[tri] = triScore;
                }
            }

            // step through input triangles in order if we hit a dead-end
            currentTriangle = bestTriangle;
            if (currentTriangle == InvalidIndex)
            {
                currentTriangle = GetNextTriangleDeadEnd(ref inputCursor, emittedFlags, triangles.Count);
            }
        }

        Debug.Assert(inputCursor == triangles.Count);
        Debug.Assert(outputTriangle == triangles.Count);
    }

    public static void OptimizeVertexCacheFifo(
        IList<QuadTreeTriangle> destination,
        IReadOnlyList<QuadTreeTriangle> triangles,
        int vertexCount,
        int cacheSize)
    {
        Debug.Assert(cacheSize >= 3);

        // guard for empty meshes
        if (triangles.Count == 0 || vertexCount == 0)
        {
            return;
        }

        // support in-place optimization
        if (ReferenceEquals(destination, triangles))
        {
            triangles = triangles.ToArray();
        }

        // build adjacency information
        TriangleAdjacency adjacency = new();
        BuildTriangleAdjacency(ref adjacency, triangles, vertexCount);

        // live triangle counts
        var liveTriangles = new int[vertexCount];
        Array.Copy(adjacency.Counts, liveTriangles, vertexCount);

        // cache time stamps
        var cacheTimestamps = new int[vertexCount];

        // dead-end stack
        Span<int> deadEnd = new int[triangles.Count * 3];
        var deadEndTop = 0;

        // emitted flags
        var emittedFlags = new bool[triangles.Count];

        var currentVertex = 0;

        var timestamp = cacheSize + 1;
        var inputCursor = 1; // vertex to restart from in case of dead-end
        var outputTriangle = 0;
        while (currentVertex != InvalidIndex)
        {
            int nextCandidatesBegin = deadEndTop;

            // emit all vertex neighbors
            var neighborsBegin = adjacency.Offsets[currentVertex];
            var neighborsEnd = neighborsBegin + adjacency.Counts[currentVertex];

            for (var it = neighborsBegin; it < neighborsEnd; ++it)
            {
                var triangle = adjacency.Data[it];
                if (!emittedFlags[triangle])
                {
                    var t = triangles[triangle];

                    // output indices
                    destination[outputTriangle] = t;
                    outputTriangle++;

                    // update dead-end stack
                    deadEnd[deadEndTop + 0] = t.A;
                    deadEnd[deadEndTop + 1] = t.B;
                    deadEnd[deadEndTop + 2] = t.C;
                    deadEndTop += 3;

                    // update live triangle counts
                    liveTriangles[t.A]--;
                    liveTriangles[t.B]--;
                    liveTriangles[t.C]--;

                    // update cache info
                    // if vertex is not in cache, put it in cache
                    if (timestamp - cacheTimestamps[t.A] > cacheSize)
                        cacheTimestamps[t.A] = timestamp++;

                    if (timestamp - cacheTimestamps[t.B] > cacheSize)
                        cacheTimestamps[t.B] = timestamp++;

                    if (timestamp - cacheTimestamps[t.C] > cacheSize)
                        cacheTimestamps[t.C] = timestamp++;

                    // update emitted flags
                    emittedFlags[triangle] = true;
                }
            }

            // next candidates are the ones we pushed to dead-end stack just now
            int nextCandidatesEnd = deadEndTop;

            // get next vertex
            currentVertex = GetNextVertexNeighbor(deadEnd[nextCandidatesBegin..nextCandidatesEnd], liveTriangles,
                cacheTimestamps, timestamp, cacheSize);
            if (currentVertex == InvalidIndex)
            {
                currentVertex = GetNextVertexDeadEnd(deadEnd, ref deadEndTop, ref inputCursor, liveTriangles,
                    vertexCount);
            }
        }

        Debug.Assert(outputTriangle == triangles.Count);
    }

    public static int OptimizeVertexFetch<T>(
        IList<T> destination,
        IList<QuadTreeTriangle> triangles,
        IReadOnlyList<T> vertices)
    {
        // support in-place optimization
        if (ReferenceEquals(destination, vertices))
        {
            vertices = vertices.ToArray();
        }

        // build vertex remap table
        int[] vertexRemap = new int[vertices.Count];
        Array.Fill(vertexRemap, InvalidIndex);

        int nextVertex = 0;
        for (var i = 0; i < triangles.Count; ++i)
        {
            var tri = triangles[i];
            for (var j = 0; j < 3; ++j)
            {
                var index = tri[j];
                Debug.Assert(index < vertices.Count);

                ref int remap = ref vertexRemap[index];
                if (remap == InvalidIndex) // vertex was not added to destination VB
                {
                    // add vertex
                    destination[nextVertex] = vertices[index];
                    remap = nextVertex++;
                }

                // modify indices in place
                tri[j] = remap;
            }

            triangles[i] = tri;
        }

        Debug.Assert(nextVertex <= vertices.Count);
        return nextVertex;
    }
}
