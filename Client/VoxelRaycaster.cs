using System.Numerics;
using Silk.NET.Maths;

namespace Client;
public class VoxelRaycaster
{
    private readonly Func<Vector3D<int>, bool> _isVoxelSolidFunc;

    public readonly struct Hit
    {
        public Vector3D<int> Position { get; }
        
        public Hit(Vector3D<int> position)
        {
            Position = position;
        }
    }

    public VoxelRaycaster(Func<Vector3D<int>, bool> isVoxelSolidFunc)
    {
        _isVoxelSolidFunc = isVoxelSolidFunc;
    }

    public Hit? Cast(Vector3 origin, Vector3 direction, float maxDistance)
    {
        // Normalize direction to ensure consistent behavior
        direction = Vector3.Normalize(direction);
        
        // Initialization Phase
        // The algorithm breaks the ray into intervals of t, each spanning one voxel.
        // We need to track our current voxel position and calculate how far we can travel
        // before hitting the next voxel boundary in each dimension.
        
        // 1. Identify the voxel where the ray origin is located
        // This gives us our starting voxel coordinates in the discrete voxel grid
        var blockOrigin = Block.WorldToBlockPosition(origin);
        
        // 2. Current voxel coordinates - these will be updated as we traverse
        var x = blockOrigin.X;
        var y = blockOrigin.Y;
        var z = blockOrigin.Z;
        
        // 3. Step direction for each axis (+1 or -1)
        // This determines whether we increment or decrement coordinates when crossing boundaries
        // The sign of the direction vector tells us which way we're moving along each axis
        var stepX = direction.X > 0 ? 1 : (direction.X < 0 ? -1 : 0);
        var stepY = direction.Y > 0 ? 1 : (direction.Y < 0 ? -1 : 0);
        var stepZ = direction.Z > 0 ? 1 : (direction.Z < 0 ? -1 : 0);
        
        // 4-5. Calculate tMax values - the t parameter where ray crosses the next voxel boundary
        // This represents how far along the ray we can travel before hitting the next grid line
        // If direction component is 0, we never cross boundaries in that dimension
        float tMaxX, tMaxY, tMaxZ;
        
        if (stepX != 0)
        {
            // Calculate which voxel boundary we'll hit next in X direction
            // Voxel boundaries are at half-integer positions: ..., -0.5, 0.5, 1.5, ...
            // If moving positive, we want the right edge; if negative, the left edge
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
        
        // 6. Calculate tDelta values - how much t increases when moving one voxel in each direction
        // This represents the "cost" in t-parameter to move exactly one voxel width/height/depth
        // We precompute this so we can quickly update tMax values during traversal
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
                return new Hit(currentPos);
            }
            
            // Determine which axis has the nearest boundary crossing
            // The axis with minimum tMax is the one we'll cross first
            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    // X boundary is closest - step in X direction
                    if (tMaxX > tMax) break; // Exceeded max distance
                    
                    x += stepX;
                    tMaxX += tDeltaX; // Update when we'll hit the next X boundary
                }
                else
                {
                    // Z boundary is closest - step in Z direction
                    if (tMaxZ > tMax) break;
                    
                    z += stepZ;
                    tMaxZ += tDeltaZ;
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    // Y boundary is closest - step in Y direction
                    if (tMaxY > tMax) break;
                    
                    y += stepY;
                    tMaxY += tDeltaY;
                }
                else
                {
                    // Z boundary is closest - step in Z direction
                    if (tMaxZ > tMax) break;
                    
                    z += stepZ;
                    tMaxZ += tDeltaZ;
                }
            }
        }
        
        // No solid voxel found within max distance
        return null;
    }
}