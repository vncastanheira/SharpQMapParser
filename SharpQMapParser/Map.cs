using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SharpQMapParser.Core;

namespace SharpQMapParser
{
    public class Map
    {
        const string NUMERIC_REGEX_STR = @"-?\d+[\.*\d+]*";                 // matches all numbers
        const string POINTS_REGEX_STR = @"\(\s-?\d+\s-?\d+\s-?\d+\s\)";     // matches all Point values with parentheses
        const string QUOTED_NAME_PATTERN = @"""(.*?)""";                    // matching texture names with quotation
        const string STANDARD_NAME_PATTERN = @"^(.*?)\s";                    // matching texture names without space character

        public List<Entity> Entities = new List<Entity>();
        public MapFormat MapFormat = MapFormat.Standard;

        private Regex NumericReg { get; }
        private Regex PointsReg { get; }
        private int _lineNumber = 0;

        public Map()
        {
            NumericReg = new Regex(NUMERIC_REGEX_STR);
            PointsReg = new Regex(POINTS_REGEX_STR);
        }

        public void Parse(StreamReader textStream)
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
                                MapFormat = value == "220" ? MapFormat.Valve : MapFormat.Standard;
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
                    try
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
                    catch (Exception ex)
                    {
                        throw new MapParsingException(string.Format(Resource.ExceptionMessageCorruptMapFile, _lineNumber), ex);
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

            // Match Point values
            var pointsCollection = PointsReg.Matches(line);

            // Load points
            for (int i = 0; i < pointsCollection.Count; i++)
            {
                var match = pointsCollection[i];
                var axis = NumericReg.Matches(match.Value);
                var x = int.Parse(axis[0].Value);
                var y = int.Parse(axis[1].Value);
                var z = int.Parse(axis[2].Value);
                plane.Points[i] = new Point(x, y, z);
            }

            var parsingString = PointsReg.Replace(line, string.Empty).TrimStart();

            // Load texture name            
            var pattern = parsingString.StartsWith("\"") ? QUOTED_NAME_PATTERN : STANDARD_NAME_PATTERN;
            var results = Regex.Split(parsingString, pattern).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            plane.TextureName = results[0];
            parsingString = results[1];

            // Load offset, rotation and scale
            var textureProperties = NumericReg.Matches(parsingString);
            plane.XOff = int.Parse(textureProperties[0].Value);
            plane.YOff = int.Parse(textureProperties[1].Value);
            plane.Rotation = int.Parse(textureProperties[2].Value);
            plane.XScale = float.Parse(textureProperties[3].Value);
            plane.YScale = float.Parse(textureProperties[4].Value);

            return plane;
        }
    }

    public enum MapFormat
    {
        Standard,
        Valve
    }
}
