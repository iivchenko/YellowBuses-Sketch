using System.Runtime.Serialization;

namespace YellowBuses_Sketch.App.DataModel
{
    [DataContract]
    public class StopEntity
    {
        public StopEntity(string name, string direction, string stopId, string stopCode)
        {
            Name = name;
            Direction = direction;
            StopId = stopId;
            StopCode = stopCode;
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Direction { get; set; }

        [DataMember]
        public string StopId { get; set; }

        [DataMember]
        public string StopCode { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}