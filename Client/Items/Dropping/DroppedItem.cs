using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.Items.Dropping;

public class DroppedItem
{
    private const float Scale = 0.25f;
    private const float RotationSpeed = 45f;
    private const float BobSpeed = 1.5f;
    private const float BobStrength = 0.05f;
    
    private readonly ItemDropRenderer _renderer;
    private readonly Entity _entity;

    private float _time;

    public DroppedItem(GL gl, Shader shader, ItemTextures textures, ItemData itemData, Vector3 startingPosition, Func<Vector3, bool> isBlockSolidFunc)
    {
        _renderer = new ItemDropRenderer(gl, shader, textures, itemData);
        _entity = new Entity(startingPosition, new Vector3(Scale, Scale, Scale), isBlockSolidFunc);
    }

    public void Update(float deltaTime)
    {
        _entity.Update(deltaTime);
        _time += deltaTime;
    }

    public void Render(Matrix4x4 view, Matrix4x4 projection)
    {
        var yRotation =  MathUtil.DegreesToRadians(_time * RotationSpeed);
        var yOffset = ((MathF.Sin(_time * BobSpeed) + 1f) / 2f) * BobStrength;
        
        _renderer.Render(view, projection, _entity.Position with { Y = _entity.Position.Y + yOffset }, Scale, yRotation);
    }
}