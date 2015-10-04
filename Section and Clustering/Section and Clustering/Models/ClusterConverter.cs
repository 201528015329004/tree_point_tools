using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Section_and_Clustering.Models
{
    public class ClusterConverter : JsonConverter
    {
        public override bool CanRead => true;

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
            JObject jsonObject = JObject.Load(reader);
            var listObject = jsonObject.SelectToken("cluster").ToObject<List<List<double>>>();
            var points = from x in listObject select new Point3D(x[0], x[1], x[2]);
            return new Cluster(points);

        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Cluster);
        }
    }
}