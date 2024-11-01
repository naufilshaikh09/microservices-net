using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Play.Catalog.Service.Entities
{

    public class Item : IEntity
    {
        // [BsonId]
        // [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        // [BsonElement("name")]
        public string? Name { get; set; }

        // [BsonElement("description")]
        public string? Description { get; set; }

        // [BsonElement("price")]
        public decimal Price { get; set; }

        // [BsonElement("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }
    }
}