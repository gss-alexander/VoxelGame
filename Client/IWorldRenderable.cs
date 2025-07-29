using System.Numerics;

namespace Client;

public interface IWorldRenderable
{
    void Render(Matrix4x4 view, Matrix4x4 projection, Vector3 position, float scale, float rotation = 0f);
}