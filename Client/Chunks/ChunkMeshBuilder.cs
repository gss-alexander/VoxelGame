using System.Diagnostics;
using Client.Blocks;
using Client.Diagnostics;
using JetBrains.Profiler.Api;
using Silk.NET.Maths;

namespace Client.Chunks;

public static class ChunkMeshBuilder
{
    public readonly struct ChunkMeshGenerationResult
    {
        public Mesh Opaque { get; }
        public Mesh Transparent { get; }

        public ChunkMeshGenerationResult(Mesh opaque, Mesh transparent)
        {
            Opaque = opaque;
            Transparent = transparent;
        }
    }
    
    private readonly struct FaceVisibility
    {
        public readonly bool Front;
        public readonly bool Back;
        public readonly bool Left;
        public readonly bool Right;
        public readonly bool Top;
        public readonly bool Bottom;

        public FaceVisibility(bool front, bool back, bool left, bool right, bool top, bool bottom)
        {
            Front = front;
            Back = back;
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public bool IsFaceVisible(BlockGeometry.FaceDirection direction) => direction switch
        {
            BlockGeometry.FaceDirection.Front => Front,
            BlockGeometry.FaceDirection.Back => Back,
            BlockGeometry.FaceDirection.Left => Left,
            BlockGeometry.FaceDirection.Right => Right,
            BlockGeometry.FaceDirection.Top => Top,
            BlockGeometry.FaceDirection.Bottom => Bottom,
            _ => false
        };
    }

    private static readonly ObjectPool<ArrayBuffer<float>> _vertexBuffers =
        new(() => new ArrayBuffer<float>(Chunk.Size * Chunk.Height * Chunk.Size * 6 * 24), buffer => buffer.Clear());
    private static readonly ObjectPool<ArrayBuffer<uint>> _indexBuffers =
        new(() => new ArrayBuffer<uint>(Chunk.Size * Chunk.Height * Chunk.Size * 6 * 6), buffer => buffer.Clear());
    
    public static ChunkMeshGenerationResult Create(ChunkData chunkData, BlockDatabase blockDatabase, BlockTextures blockTextures, Func<Vector3D<int>, int> getBlockFunc)
    {
        var sw = Stopwatch.StartNew();
        MeasureProfiler.StartCollectingData();
        
        var opaqueVertices = _vertexBuffers.Get();
        var opaqueIndices = _indexBuffers.Get();
        var transparentIndices = _indexBuffers.Get();
        var transparentVertices = _vertexBuffers.Get();

        var transparentIndicesOffset = 0u;
        var opaqueIndicesOffset = 0u;
        var totalBlocks = Chunk.Size * Chunk.Height * Chunk.Size;
        
        for (var i = 0; i < totalBlocks; i++)
        {
            var x = i % Chunk.Size;
            var y = (i / Chunk.Size) % Chunk.Height;
            var z = i / (Chunk.Size * Chunk.Height);
    
            var blockPos = new Vector3D<int>(x, y, z);
            var blockId = chunkData.GetBlock(blockPos);
            var blockData = blockDatabase.GetById(blockId);
            if (!blockData.IsSolid)
            {
                continue;
            }

            var faceVisibility = CalculateAllFaceVisibility(x, y, z, chunkData, blockDatabase, getBlockFunc);

            var isTransparent = blockData.IsTransparent;
            var vertices = isTransparent ? transparentVertices : opaqueVertices;
            var indices = isTransparent ? transparentIndices : opaqueIndices;

            foreach (var face in BlockGeometry.Faces)
            {
                if (!faceVisibility.IsFaceVisible(face.Direction))
                {
                    continue;
                }

                var textureIndex = blockTextures.GetBlockTextureIndex(blockId, face.Direction);
                        
                for (var vertexIndex = 0; vertexIndex < face.Vertices.Length; vertexIndex += 6)
                {
                    var vX = face.Vertices[vertexIndex] + x + (Chunk.Size * chunkData.Position.X);
                    var vY = face.Vertices[vertexIndex + 1] + y;
                    var vZ = face.Vertices[vertexIndex + 2] + z + (Chunk.Size * chunkData.Position.Y);
                    var vU = face.Vertices[vertexIndex + 3];
                    var vV = face.Vertices[vertexIndex + 4];
                    var brightness = face.Vertices[vertexIndex + 5];
        
                    vertices.Write(vX);
                    vertices.Write(vY);
                    vertices.Write(vZ);
                    vertices.Write(vU);
                    vertices.Write(vV);
                    vertices.Write(textureIndex);
                    vertices.Write(brightness); 
                }

                foreach (var index in face.Indices)
                {
                    var indicesOffset = isTransparent ? transparentIndicesOffset : opaqueIndicesOffset;
                    indices.Write(index + indicesOffset);
                }

                if (isTransparent)
                {
                    transparentIndicesOffset += 4;
                }
                else
                {
                    opaqueIndicesOffset += 4;
                }
            }
        }

        var result = new ChunkMeshGenerationResult(
            new Mesh(opaqueVertices.Read(), opaqueIndices.Read()),
            new Mesh(transparentVertices.Read(), transparentIndices.Read())
        );
        
        _vertexBuffers.Release(opaqueVertices);
        _vertexBuffers.Release(transparentVertices);
        
        _indexBuffers.Release(opaqueIndices);
        _indexBuffers.Release(transparentIndices);


        sw.Stop();
        MeasureProfiler.SaveData();
        Console.WriteLine($"[Chunk Mesh Builder]: Generated mesh in {sw.ElapsedMilliseconds}ms");
        ChunkGenerationTimeTracking.MeshGenerationTime.AddTime((float)sw.Elapsed.TotalSeconds);
        return result;
    }
    
    private static FaceVisibility CalculateAllFaceVisibility(int x, int y, int z, ChunkData chunkData,
        BlockDatabase blockDatabase, Func<Vector3D<int>, int> getBlockFunc)
    {
        var frontVisible = IsFaceVisible(x, y, z + 1, chunkData, blockDatabase, getBlockFunc);
        var backVisible = IsFaceVisible(x, y, z - 1, chunkData, blockDatabase, getBlockFunc);
        var leftVisible = IsFaceVisible(x - 1, y, z, chunkData, blockDatabase, getBlockFunc);
        var rightVisible = IsFaceVisible(x + 1, y, z, chunkData, blockDatabase, getBlockFunc);
        var topVisible = IsFaceVisible(x, y + 1, z, chunkData, blockDatabase, getBlockFunc);
        var bottomVisible = IsFaceVisible(x, y - 1, z, chunkData, blockDatabase, getBlockFunc);

        return new FaceVisibility(frontVisible, backVisible, leftVisible, rightVisible, topVisible, bottomVisible);
    }
    
    private static bool IsFaceVisible(int neighborX, int neighborY, int neighborZ, ChunkData chunkData,
        BlockDatabase blockDatabase, Func<Vector3D<int>, int> getBlockFunc)
    {
        if (neighborX < 0 || neighborX >= Chunk.Size ||
            neighborY < 0 || neighborY >= Chunk.Height ||
            neighborZ < 0 || neighborZ >= Chunk.Size)
        {
            var worldPosition = Chunk.LocalChunkToWorldPosition(chunkData.Position, new Vector3D<int>(neighborX, neighborY, neighborZ));
            var neighborBlockId = getBlockFunc(worldPosition);
            var neighborBlockData = blockDatabase.GetById(neighborBlockId);
            return !neighborBlockData.IsSolid || neighborBlockData.IsTransparent;
        }

        var neighborPos = new Vector3D<int>(neighborX, neighborY, neighborZ);
        var blockId = chunkData.GetBlock(neighborPos);
        var blockData = blockDatabase.GetById(blockId);
        return !blockData.IsSolid || blockData.IsTransparent;
    }
}