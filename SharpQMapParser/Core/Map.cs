using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SharpQMapParser.Core
{
    public class Map
    {
        public List<Entity> Entities = new List<Entity>();
        public MapFormat MapFormat;

        int _lineNumber = 0;

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
                            ParseStandardFormat(line);
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

            var reg = new Regex(@"\s-?\d+[\.*\d+]*"); // matches all numbers
            var matchCollection = reg.Matches(line);

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

            return plane;
        }

        void ParseValveFormat(MatchCollection matchCollection)
        {

        }
    }

    public enum MapFormat
    {
        Standard,
        Valve
    }
}
