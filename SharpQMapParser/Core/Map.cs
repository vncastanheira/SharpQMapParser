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

        public void Parse(StreamReader textStream, MapFormat mapFormat = MapFormat.Standard)
        {
            Entity currentEntity = null;
            Brush currentBrush = null;

            string rawLine;
            int lineNumber = 0;
            while ((rawLine = textStream.ReadLine()) != null)
            {
                lineNumber++;

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
                        throw new MapParsingException(string.Format(Resource.ExceptionMessageInvalidCurlyBraces, lineNumber));
                    }
                }

                if (line.StartsWith("\"")) // parameter
                {
                    if (currentEntity == null)
                        throw new MapParsingException(string.Format(Resource.ExceptionMessageCorruptMapFile, lineNumber));

                    try
                    {
                        ReadEntityProperty(line, out string key, out string value);

                        switch (key)
                        {
                            case "classname":
                                currentEntity.ClassName = value; 
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
                                    throw new MapParsingException(string.Format(Resource.ExceptionMessageErrorParsingPoint, lineNumber));
                                }
                            default:
                                currentEntity.Properties.Add(key, value);
                                break;
                        }

                    }
                    catch (MapParsingException ex)
                    {
                        throw new MapParsingException(string.Format(ex.Message, lineNumber));
                    }
                }

                if (line.StartsWith("("))
                {
                    // read brush
                }

                if (line.StartsWith("}")) // close entity or brush
                {
                    if(currentBrush != null)
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
    }

    public enum MapFormat
    {
        Standard,
        Valve,
        Hexen2
    }
}
