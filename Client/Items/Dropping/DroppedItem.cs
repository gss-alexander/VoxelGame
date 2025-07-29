using System.Numerics;
using Silk.NET.OpenGL;

namespace Client.Items.Dropping;

public class DroppedItem
{
    public string ItemId { get; }
    public Vector3 Position => _entity.Position;
    
    private const float Scale = 0.25f;
    private const float RotationSpeed = 45f;
    private const float BobSpeed = 1.5f;
    private const float BobStrength = 0.05f;
    
    private readonly Entity _entity;

    private readonly IWorldRenderable _renderer;

    private float _time;

    public DroppedItem(IWorldRenderable renderer, ItemData itemData, Vector3 startingPosition, Func<Vector3, bool> isBlockSolidFunc)
    {
        _renderer = renderer;
        _entity = new Entity(startingPosition, new Vector3(Scale, Scale, Scale), isBlockSolidFunc);
        ItemId = itemData.ExternalId;
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