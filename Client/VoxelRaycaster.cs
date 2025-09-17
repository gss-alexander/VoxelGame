using System.Numerics;
using Client.Blocks;
using Silk.NET.Maths;

namespace Client;

public class VoxelRaycaster
{
    private readonly Func<Vector3D<int>, bool> _isVoxelSolidFunc;

    public readonly struct Hit
    {
        public Vector3D<int> Position { get; }
        public BlockGeometry.FaceDirection Face { get; }
        
        public Hit(Vector3D<int> position,BlockGeometry.FaceDirection face)
        {
            Position = position;
            Face = face;
        }
    }

    public VoxelRaycaster(Func<Vector3D<int>, bool> isVoxelSolidFunc)
    {
        _isVoxelSolidFunc = isVoxelSolidFunc;
    }

    public Hit? Cast(Vector3 origin, Vector3 direction, float maxDistance)
    {
        direction = Vector3.Normalize(direction);

        var blockOrigin = Block.WorldToBlockPosition(origin);
        
        var x = blockOrigin.X;
        var y = blockOrigin.Y;
        var z = blockOrigin.Z;
        
        var stepX = direction.X > 0 ? 1 : (direction.X < 0 ? -1 : 0);
        var stepY = direction.Y > 0 ? 1 : (direction.Y < 0 ? -1 : 0);
        var stepZ = direction.Z > 0 ? 1 : (direction.Z < 0 ? -1 : 0);
        
        // Calculate tMax values - the t parameter where ray crosses the next voxel boundary
        float tMaxX, tMaxY, tMaxZ;
        
        if (stepX != 0)
        {
            float voxelBoundaryX = stepX > 0 ? x + 0.5f : x - 0.5f;
            tMaxX = (voxelBoundaryX - origin.X) / direction.X;
        }
        else
        {
            tMaxX = float.MaxValue; // Never crosses X boundaries
        }
        
        if (stepY != 0)
        {
            float voxelBoundaryY = stepY > 0 ? y + 0.5f : y - 0.5f;
            tMaxY = (voxelBoundaryY - origin.Y) / direction.Y;
        }
        else
        {
            tMaxY = float.MaxValue;
        }
        
        if (stepZ != 0)
        {
            float voxelBoundaryZ = stepZ > 0 ? z + 0.5f : z - 0.5f;
            tMaxZ = (voxelBoundaryZ - origin.Z) / direction.Z;
        }
        else
        {
            tMaxZ = float.MaxValue;
        }
        
        // Calculate tDelta values
        float tDeltaX = stepX != 0 ? Math.Abs(1.0f / direction.X) : float.MaxValue;
        float tDeltaY = stepY != 0 ? Math.Abs(1.0f / direction.Y) : float.MaxValue;
        float tDeltaZ = stepZ != 0 ? Math.Abs(1.0f / direction.Z) : float.MaxValue;
        
        float tMax = maxDistance;
        
        // Incremental Traversal Phase
        while (true)
        {
            var currentPos = new Vector3D<int>(x, y, z);
            if (_isVoxelSolidFunc(currentPos))
            {
                // For starting voxel hits, we can't determine face from step direction. Default to top
                return new Hit(currentPos, BlockGeometry.FaceDirection.Top);
            }
            
            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    if (tMaxX > tMax) break; // Exceeded max distance
                    
                    x += stepX;
                    tMaxX += tDeltaX; // Update when we'll hit the next X boundary
                    
                    // Face is opposite to step direction
                    if (_isVoxelSolidFunc(new Vector3D<int>(x, y, z)))
                    {
                        var face = -stepX > 0 ? BlockGeometry.FaceDirection.Right : BlockGeometry.FaceDirection.Left;
                        return new Hit(new Vector3D<int>(x, y, z), face);
                    }
                }
                else
                {
                    // step in Z direction
                    if (tMaxZ > tMax) break;
                    
                    z += stepZ;
                    tMaxZ += tDeltaZ;
                    
                    // face is opposite to step direction
                    if (_isVoxelSolidFunc(new Vector3D<int>(x, y, z)))
                    {
                        var face = -stepZ > 0 ? BlockGeometry.FaceDirection.Front : BlockGeometry.FaceDirection.Back;
                        return new Hit(new Vector3D<int>(x, y, z), face);
                    }
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    // step in Y direction
                    if (tMaxY > tMax) break;
                    
                    y += stepY;
                    tMaxY += tDeltaY;
                    
                    //  face is opposite to step direction
                    if (_isVoxelSolidFunc(new Vector3D<int>(x, y, z)))
                    {
                        var face = -stepY > 0 ? BlockGeometry.FaceDirection.Top : BlockGeometry.FaceDirection.Bottom;
                        return new Hit(new Vector3D<int>(x, y, z), face);
                    }
                }
                else
                {
                    // step in Z direction
                    if (tMaxZ > tMax) break;
                    
                    z += stepZ;
                    tMaxZ += tDeltaZ;
                    
                    //face is opposite to step direction
                    if (_isVoxelSolidFunc(new Vector3D<int>(x, y, z)))
                    {
                        var face = -stepZ > 0 ? BlockGeometry.FaceDirection.Front : BlockGeometry.FaceDirection.Back;
                        return new Hit(new Vector3D<int>(x, y, z), face);
                    }
                }
            }
        }
        
        // No solid voxel found within max distance
        return null;
    }
}