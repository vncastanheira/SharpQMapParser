using System.Dynamic;

namespace SharpQMapParser.Core
{
    public class Plane
    {
        public Point[] Points = new Point[3];
        public string TextureName = string.Empty;
        public int XOff, YOff;
        public int Rotation;
        public float XScale, YScale;
    }
}
