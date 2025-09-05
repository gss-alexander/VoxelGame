namespace Client.Blocks;

public class BlockData
{
    public class BlockTexture
    {
        public string? Default { get; set; }
        public string? Top { get; set; }
        public string? Bottom { get; set; }
        public string? Left { get; set; }
        public string? Right { get; set; }
        public string? Front { get; set; }
        public string? Back { get; set; }

        public string GetTextureForFace(BlockGeometry.FaceDirection faceDirection)
        {
            return faceDirection switch
            {
                BlockGeometry.FaceDirection.Top => Top ?? Default ?? "missing.png",
                BlockGeometry.FaceDirection.Bottom => Bottom ?? Default ?? "missing.png",
                BlockGeometry.FaceDirection.Left => Left ?? Default ?? "missing.png",
                BlockGeometry.FaceDirection.Right => Right ?? Default ?? "missing.png",
                BlockGeometry.FaceDirection.Front => Front ?? Default ?? "missing.png",
                BlockGeometry.FaceDirection.Back => Back ?? Default ?? "missing.png",
                _ => "missing.png"
            };
        }
    }

    public class BlockDrop
    {
        public string Item { get; set; } = string.Empty;
        public float Probability { get; set; }
        public int Count { get; set; }
    }
    
    public string ExternalId { get; set; } = string.Empty;
    public string DisplayName { get; set; }
    public BlockTexture Textures { get; set; } = new();
    public float Strength { get; set; }
    public bool IsTransparent { get; set; }
    public bool IsSolid { get; set; }
    public string? PlacementSoundId { get; set; }
    public BlockDrop[]? Drops { get; set; }
}