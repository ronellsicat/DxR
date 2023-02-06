// Copyright � Joerg Battermann 2014, Matt Hunt 2017

using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GeoJSON.Net.Converters
{
    /// <summary>
    /// Converter to read and write the <see cref="IEnumerable{Point}" /> type.
    /// </summary>
    public class PointEnumerableConverter : JsonConverter
    {
        private static readonly PositionConverter PositionConverter = new PositionConverter();
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var points = (IEnumerable<Point>) value;

            writer.WriteStartArray();
            foreach (var point in points)
            {
                PositionConverter.WriteJson(writer, point.Coordinates, serializer);
            }
            writer.WriteEndArray();
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var coordinates = existingValue as JArray ?? serializer.Deserialize<JArray>(reader);
            return coordinates.Select(position => new Point(position.ToObject<IEnumerable<double>>().ToPosition()));
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IEnumerable<Point>);
        }
    }
}