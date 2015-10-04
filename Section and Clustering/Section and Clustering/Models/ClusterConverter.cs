using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Section_and_Clustering.Models
{
    public class ClusterConverter : JsonConverter
    {
        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var cluster = (Cluster) value;

            writer.WriteStartObject();
            writer.WritePropertyName("cluster");
            writer.WriteStartArray();
            foreach (var point3D in cluster.Points)
            {
                List<double> coordinates = new List<double> {point3D.X, point3D.Y, point3D.Z};
                serializer.Serialize(writer, coordinates);
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Cluster);
        }
    }
}