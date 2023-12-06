using MongoDB.Bson;

namespace EscapeFromTheWoods
{
    public class DBWoodRecord
    {
        public ObjectId Id { get; set; }
        public int woodID { get; set; }
        public int treeID { get; set; }
        public Location location { get; set; }
    }
}