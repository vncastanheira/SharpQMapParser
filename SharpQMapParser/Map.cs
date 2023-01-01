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
        const string NUMERIC_REGEX_STR = @"-?\d+[\.*\d+]*";                                     // matches all numbers
        const string POINTS_REGEX_STR = @"\(\s(-?\d*.?\d*)\s(-?\d*.?\d*)\s(-?\d*.?\d*\s)\)";    // matches all Point values with parentheses
        const string QUOTED_TEXT_PATTERN = @"""(.*?)""";                                        // matching texture names with quotation
        const string STANDARD_TEXNAME_PATTERN = @"^(.*?)\s";                                       // matching texture names without space character

        #region Compiled Regex
        private Regex NumericReg { get; }
        private Regex PointsReg { get; }
        private Regex QuotedTextRegex { get; }
        private Regex StandardTexNameRegex { get; }
        #endregion

        public List<Entity> Entities = new List<Entity>();
        public MapFormat MapFormat = MapFormat.Standard;

        private int _lineNumber = 0;

        public Map()
        {
            NumericReg = new Regex(NUMERIC_REGEX_STR, RegexOptions.Compiled);
            PointsReg = new Regex(POINTS_REGEX_STR, RegexOptions.Compiled);
            QuotedTextRegex = new Regex(QUOTED_TEXT_PATTERN, RegexOptions.Compiled);
            StandardTexNameRegex = new Regex(STANDARD_TEXNAME_PATTERN, RegexOptions.Compiled);
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
                            //case "mapversion":
                            //    MapFormat = value == "220" ? MapFormat.Valve : MapFormat.Standard;
                            //    break;
                            case "origin":
                                try
                                {
                                    var coords = Regex.Matches(value, NUMERIC_REGEX_STR).Select(m => m.Value).ToArray();
                                    var point = new Point(float.Parse(coords[0]), float.Parse(coords[1]), float.Parse(coords[2]));
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
            var result = Regex.Matches(line, @""".*?""");
            key = result[0].Value.Replace("\"", string.Empty);
            value = result[1].Value.Replace("\"", string.Empty);
        }

        Plane ParseStandardFormat(string line)
        {
            var plane = new Plane();

            // Match Point values
            var pointsCollection = PointsReg.Matches(line);

            if (pointsCollection.Count != 3)
                throw new MapParsingException(string.Format(Resource.ExceptionMessageErrorParsingPoint, _lineNumber));

            // Load points
            for (int i = 0; i < pointsCollection.Count; i++)
            {
                var match = pointsCollection[i];
                var axis = NumericReg.Matches(match.Value);
                var x = float.Parse(axis[0].Value);
                var y = float.Parse(axis[1].Value);
                var z = float.Parse(axis[2].Value);
                plane.Points[i] = new Point(x, y, z);
            }

            var parsingString = PointsReg.Replace(line, string.Empty).TrimStart();

            // Load texture name            
            var texRegex = parsingString.StartsWith("\"") ? QuotedTextRegex : StandardTexNameRegex;
            var results = texRegex.Split(parsingString).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            plane.TextureName = results[0];
            parsingString = results[1];

            // Load offset, rotation and scale
            var textureProperties = NumericReg.Matches(parsingString);
            plane.XOff = float.Parse(textureProperties[0].Value);
            plane.YOff = float.Parse(textureProperties[1].Value);
            plane.Rotation = float.Parse(textureProperties[2].Value);
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
