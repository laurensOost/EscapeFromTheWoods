using MongoDB.Bson;

namespace EscapeFromTheWoods
{
    public class DBLogRecord
    {
        public ObjectId Id { get; set; } // MongoDB uses ObjectId for the _id field
        public int woodID { get; set; }
        public int monkeyID { get; set; }
        public string message { get; set; }
    }
}