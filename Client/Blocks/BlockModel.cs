﻿using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.Blocks;

public class BlockModel
{
    public int BlockId { get; }
    public Vector3 Position { get; set; }
    public float Velocity { get; set; }
    public float Size { get; set; } = 0.15f;

    private const float Gravity = 14f;
    
    private readonly BlockTextures _blockTextures;
    private readonly Shader _shader;

    private readonly MeshRenderer _meshRenderer;
    private readonly Mesh _mesh;

    private readonly Func<Vector3, bool> _isBlockSolidFunc;

    private float _rotation;
    
    public BlockModel(BlockTextures blockTextures, int blockId, Shader shader, Vector3 worldPos, Func<Vector3, bool> isBlockSolidFunc, float size)
    {
        _blockTextures = blockTextures;
        _shader = shader;
        Position = worldPos;
        _isBlockSolidFunc = isBlockSolidFunc;
        BlockId = blockId;
        Size = size;

        OpenGl.Context.BindVertexArray(0); // todo: maybe remove
        _mesh = GenerateMesh(blockId);
        _meshRenderer = new MeshRenderer(_mesh);
        _meshRenderer.SetVertexAttribute(0, 3, VertexAttribPointerType.Float, 7, 0); // Position
        _meshRenderer.SetVertexAttribute(1, 2, VertexAttribPointerType.Float, 7, 3); // UV
        _meshRenderer.SetVertexAttribute(2, 1, VertexAttribPointerType.Float, 7, 5); // Texture Index
        _meshRenderer.SetVertexAttribute(3, 1, VertexAttribPointerType.Float, 7, 6); // Brightness
        _meshRenderer.Unbind();
    }

    public void Render()
    {
        _shader.Use();
        _shader.SetUniform("uTextureArray", 0);
        var centeredModel = 
                            Matrix4x4.CreateScale(0.15f) * 
                            Matrix4x4.CreateRotationY(MathUtil.DegreesToRadians(_rotation)) * 
                            Matrix4x4.CreateTranslation(Position); 
    
        _shader.SetUniform("uModel", centeredModel); 
        
        _blockTextures.Textures.Bind();
        _meshRenderer.Render();
    }

    public void Update(float deltaTime)
    {
        const float rotationSpeed = 15f;
        _rotation += rotationSpeed * deltaTime;

        Velocity -= Gravity * deltaTime;
        
        MoveWithCollision(deltaTime);
    }

    private Mesh GenerateMesh(int blockId)
    {
        var vertices = new List<float>();
        var indices = new List<uint>();
        
        var faces = BlockGeometry.Faces;
        var indicesOffset = 0u;
        foreach (var face in faces)
        {
            var textureIndex = _blockTextures.GetBlockTextureIndex(blockId, face.Direction);
            for (var vertexIndex = 0; vertexIndex < face.Vertices.Length; vertexIndex += 6)
            {
                var vX = face.Vertices[vertexIndex];
                var vY = face.Vertices[vertexIndex + 1];
                var vZ = face.Vertices[vertexIndex + 2];
                var vU = face.Vertices[vertexIndex + 3];
                var vV = face.Vertices[vertexIndex + 4];
                var brightness = face.Vertices[vertexIndex + 5];
        
                vertices.Add(vX);
                vertices.Add(vY);
                vertices.Add(vZ);
                vertices.Add(vU);
                vertices.Add(vV);
                vertices.Add(textureIndex);
                vertices.Add(brightness); // full brightness - todo: handle this better
            }

            foreach (var index in face.Indices)
            {
                indices.Add(index + indicesOffset);
            }

            indicesOffset += 4;
        }

        return new Mesh(vertices.ToArray(), indices.ToArray());
    }
    
    private void MoveWithCollision(float deltaTime)
    {
        var deltaMovement = Velocity * deltaTime;
        var newPosition = Position;

        newPosition.Y += deltaMovement;
        if (HasCollision(newPosition))
        {
            newPosition.Y -= deltaMovement;
            Velocity = 0f;
        }

        Position = newPosition;
    }
    
    private bool HasCollision(Vector3 position)
    {
        var min = position - ((Vector3.One * Size) / 2f);
        var max = position + ((Vector3.One * Size) / 2f);

        var minX = (int)MathF.Floor(min.X);
        var maxX = (int)MathF.Floor(max.X);
        var minY = (int)MathF.Floor(min.Y);
        var maxY = (int)MathF.Floor(max.Y);
        var minZ = (int)MathF.Floor(min.Z);
        var maxZ = (int)MathF.Floor(max.Z);

        for (var x = minX - 1; x <= maxX + 1; x++)
        {
            for (var y = minY - 1; y <= maxY + 1; y++)
            {
                for (var z = minZ - 1; z <= maxZ + 1; z++)
                {
                    if (_isBlockSolidFunc(new Vector3(x, y, z)))
                    {
                        if (AABBOverlap(min, max, new Vector3(x - 0.5f, y - 0.5f, z - 0.5f), new Vector3(x + 0.5f, y + 0.5f, z + 0.5f)))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private bool AABBOverlap(Vector3 min1, Vector3 max1, Vector3 min2, Vector3 max2)
    {
        return (min1.X < max2.X && max1.X > min2.X &&
                min1.Y < max2.Y && max1.Y > min2.Y &&
                min1.Z < max2.Z && max1.Z > min2.Z);
    }
}