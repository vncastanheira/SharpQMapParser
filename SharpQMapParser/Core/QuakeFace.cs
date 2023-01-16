using System.Numerics;

namespace SharpQMapParser.Core
{
    public class QuakeFace
    {
        public Plane Plane = new Plane();
        [System.Obsolete("Replace with PLane")]
        public Vector3[] Points = new Vector3[3];
        public string TextureName = string.Empty;
        public float XOff, YOff;
        public float Rotation;
        public float XScale, YScale;
    }
}
