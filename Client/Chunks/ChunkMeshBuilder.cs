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

            var isTransparent = blockData.IsTransparent;
            var vertices = isTransparent ? transparentVertices : opaqueVertices;
            var indices = isTransparent ? transparentIndices : opaqueIndices;

            foreach (var face in BlockGeometry.Faces)
            {
                if (IsFaceBlockSolid(x, y, z, face.Direction, chunkData, blockDatabase, getBlockFunc))
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
    
    private static bool IsFaceBlockSolid(int x, int y, int z, BlockGeometry.FaceDirection face, ChunkData chunkData,
        BlockDatabase blockDatabase, Func<Vector3D<int>, int> getBlockFunc)
    {
        var facePositionOffset = face switch
        {
            BlockGeometry.FaceDirection.Back => new Vector3D<int>(0, 0, -1),
            BlockGeometry.FaceDirection.Front => new Vector3D<int>(0, 0, 1),
            BlockGeometry.FaceDirection.Left => new Vector3D<int>(-1, 0, 0),
            BlockGeometry.FaceDirection.Right => new Vector3D<int>(1, 0, 0),
            BlockGeometry.FaceDirection.Top => new Vector3D<int>(0, 1, 0),
            BlockGeometry.FaceDirection.Bottom => new Vector3D<int>(0, -1, 0),
            _ => throw new Exception($"No offset defined for face {face}")
        };

        var faceBlockPosition =
            new Vector3D<int>(facePositionOffset.X + x, facePositionOffset.Y + y, facePositionOffset.Z + z);

        var neighbourBlockSolid = IsBlockSolid(faceBlockPosition, chunkData, blockDatabase, getBlockFunc);
        var neighbourIsTransparent = false;
        if (!IsPositionOutOfBounds(faceBlockPosition))
        {
            var block = chunkData.GetBlock(faceBlockPosition);
            neighbourIsTransparent = blockDatabase.GetById(block).IsTransparent;
        }

        return neighbourBlockSolid && !neighbourIsTransparent;
    }
    
    private static bool IsPositionOutOfBounds(Vector3D<int> position)
    {
        return position.X >= Chunk.Size || position.X < 0 ||
               position.Y >= Chunk.Height || position.Y < 0 ||
               position.Z >= Chunk.Size || position.Z < 0;
    }
    
    private static bool IsBlockSolid(Vector3D<int> position, ChunkData chunkData, BlockDatabase blockDatabase,
        Func<Vector3D<int>, int> blockFunc)
    {
        if (IsPositionOutOfBounds(position))
        {
            var worldBlockPosition = Chunk.LocalChunkToWorldPosition(chunkData.Position, position);
            var blockId = blockFunc(worldBlockPosition);
            return blockDatabase.IsBlockSolid(blockId);
        }

        var block = chunkData.GetBlock(position);
        return blockDatabase.IsBlockSolid(block);
    }
}