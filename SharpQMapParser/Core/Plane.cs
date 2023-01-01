using System.Dynamic;

namespace SharpQMapParser.Core
{
    public class Plane
    {
        public Point[] Points = new Point[3];
        public string TextureName = string.Empty;
        public float XOff, YOff;
        public float Rotation;
        public float XScale, YScale;
    }
}
