namespace YellowBuses_Sketch.App.PresentationModel
{
    public class Stop
    {
        public string Name { get; set; }

        public string NaptanCode { get; set; }

        public string StopId { get; set; }

        public string StopCode { get; set; }

        public override string ToString()
        {
            return NaptanCode == null ? Name : $"{Name} ({NaptanCode})";
        }
    }
}