using System.Dynamic;

namespace SharpQMapParser
{
    public class Plane
    {
        public Point[] Points = new Point[3];
        public string TextureName = string.Empty;
        public int XOff, YOff;
        public int Rotation;
        public int XScale, YScale;
    }
}
