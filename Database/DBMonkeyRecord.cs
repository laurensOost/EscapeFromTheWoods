using System.Collections.Generic;
using MongoDB.Bson;

namespace EscapeFromTheWoods
{
    public class DBMonkeyRecord
    {
        public ObjectId Id { get; set; }
        public int monkeyID { get; set; }
        public string monkeyName { get; set; }
        public int woodID { get; set; }
        public List<Route> route { get; set; }
    }
}