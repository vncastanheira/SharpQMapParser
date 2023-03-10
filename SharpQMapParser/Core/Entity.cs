using System.Collections.Generic;
using System.Numerics;

namespace SharpQMapParser.Core
{
    public class Entity
    {
        /// <summary>
        /// Defines the entity type
        /// </summary>
        public string ClassName;

        /// <summary>
        /// For entities that are not the world
        /// </summary>
        public Vector3 Origin;

        /// <summary>
        /// worldspawn entity
        /// </summary>
        public List<Brush> Brushes = new List<Brush>();

        /// <summary>
        /// List of entity properties.
        /// Varies between entity types and engine implementations.
        /// </summary>
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
    }
}
