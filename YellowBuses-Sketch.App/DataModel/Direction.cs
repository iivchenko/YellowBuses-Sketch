using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using YellowBuses_Sketch.App.Repositories;

namespace YellowBuses_Sketch.App.DataModel
{
    [DataContract]
    public class Direction : Entity
    {
        public Direction(string name)
        {
            Name = name;
            Stops = new ObservableCollection<StopEntity>();
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public Collection<StopEntity> Stops { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}