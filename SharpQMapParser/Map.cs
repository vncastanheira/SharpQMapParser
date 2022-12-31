using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SharpQMapParser.Core;

namespace SharpQMapParser
{
    public class Map
    {
        const string NUMERIC_REGEX_STR = @"\s-?\d+[\.*\d+]*";               // matches all numbers
        const string POINTS_REGEX_STR = @"\(\s-?\d+\s-?\d+\s-?\d+\s\)";     // matches all Point values

        public List<Entity> Entities = new List<Entity>();
        public MapFormat MapFormat;


        private Regex NumericReg { get; }  
        private Regex PointsReg { get; }
        private int _lineNumber = 0;

        public Map()
        {
            NumericReg = new Regex(NUMERIC_REGEX_STR);
            PointsReg = new Regex(POINTS_REGEX_STR);
        }

        public void Parse(StreamReader textStream, MapFormat mapFormat = MapFormat.Standard)
        {
            Entity currentEntity = null;
            Brush currentBrush = null;

            string rawLine;
            _lineNumber = 0;
            while ((rawLine = textStream.ReadLine()) != null)
            {
                _lineNumber++;

                string line = rawLine.TrimStart();
                if (line.StartsWith("//") || string.IsNullOrEmpty(line)) // skip
                    continue;

                if (line.StartsWith("{")) // start entity or brush
                {
                    if (currentEntity == null)
                    {
                        currentEntity = new Entity();
                    }
                    else if (currentBrush == null)
                    {
                        currentBrush = new Brush();
                    }
                    else
                    {
                        throw new MapParsingException(string.Format(Resource.ExceptionMessageInvalidCurlyBraces, _lineNumber));
                    }
                }

                if (line.StartsWith("\"")) // parameter
                {
                    if (currentEntity == null)
                        throw new MapParsingException(string.Format(Resource.ExceptionMessageCorruptMapFile, _lineNumber));

                    try
                    {
                        ReadEntityProperty(line, out string key, out string value);

                        switch (key)
                        {
                            case "classname":
                                currentEntity.ClassName = value;
                                break;
                            case "mapversion":
                                mapFormat = value == "220" ? MapFormat.Valve : MapFormat.Standard;
                                break;
                            case "origin":
                                try
                                {
                                    var coord = value.Split();
                                    var point = new Point(int.Parse(coord[0]), int.Parse(coord[1]), int.Parse(coord[2]));
                                    currentEntity.Origin = point;
                                    break;
                                }
                                catch (FormatException)
                                {
                                    throw new MapParsingException(string.Format(Resource.ExceptionMessageErrorParsingPoint, _lineNumber));
                                }
                            default:
                                currentEntity.Properties.Add(key, value);
                                break;
                        }

                    }
                    catch (MapParsingException ex)
                    {
                        throw new MapParsingException(string.Format(ex.Message, _lineNumber));
                    }
                }

                if (line.StartsWith("(")) // read brush
                {
                    switch (MapFormat)
                    {
                        case MapFormat.Standard:
                            var plane = ParseStandardFormat(line);
                            currentBrush.Planes.Add(plane);
                            break;
                        case MapFormat.Valve:
                            break;
                        default:
                            break;
                    }
                }

                if (line.StartsWith("}")) // close entity or brush
                {
                    if (currentBrush != null)
                    {
                        currentEntity.Brushes.Add(currentBrush);
                        currentBrush = null;
                    }
                    else if (currentEntity != null)
                    {
                        Entities.Add(currentEntity);
                        currentEntity = null;
                    }
                }
            }
        }

        void ReadEntityProperty(string line, out string key, out string value)
        {
            var regex = new Regex(@"("")\s("")");
            var result = regex.Split(line);
            if (result.Length != 4)
                throw new MapParsingException(Resource.ExceptionMessageInvalidEntityProperty);

            key = result[0].Replace("\"", string.Empty);
            value = result[3].Replace("\"", string.Empty);
        }

        Plane ParseStandardFormat(string line)
        {
            var plane = new Plane();

            // Match numeric values

            var matchCollection = NumericReg.Matches(line);

            if (matchCollection.Count == 14)
            {
                try
                {
                    // Load Points 
                    var x1 = int.Parse(matchCollection[0].Value);
                    var y1 = int.Parse(matchCollection[1].Value);
                    var z1 = int.Parse(matchCollection[2].Value);
                    plane.Points[0] = new Point(x1, y1, z1);

                    var x2 = int.Parse(matchCollection[3].Value);
                    var y2 = int.Parse(matchCollection[4].Value);
                    var z2 = int.Parse(matchCollection[5].Value);
                    plane.Points[1] = new Point(x2, y2, z2);

                    var x3 = int.Parse(matchCollection[6].Value);
                    var y3 = int.Parse(matchCollection[7].Value);
                    var z3 = int.Parse(matchCollection[8].Value);
                    plane.Points[2] = new Point(x3, y3, z3);

                    // Load offset, rotation and scale
                    plane.XOff = int.Parse(matchCollection[9].Value);
                    plane.YOff = int.Parse(matchCollection[10].Value);
                    plane.Rotation = int.Parse(matchCollection[11].Value);
                    plane.XScale = float.Parse(matchCollection[11].Value);
                    plane.YScale = float.Parse(matchCollection[12].Value);
                }
                catch (Exception)
                {
                    throw new MapParsingException(string.Format(Resource.ExceptionMessageCorruptMapFile, _lineNumber));
                }
            }
            else
            {
                throw new MapParsingException($"Incorrect numeric values count on line {_lineNumber}");
            }
            
            // Load texture
            var textureName = PointsReg.Replace(line, string.Empty); // remove Point values
            plane.TextureName = NumericReg.Replace(textureName, string.Empty); // remove additional numeric values
            
            return plane;
        }
    }

    public enum MapFormat
    {
        Standard,
        Valve
    }
}
