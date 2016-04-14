using System;
using System.Collections.ObjectModel;

namespace YellowBuses_Sketch.App.PresentationModel
{
    public class StopSchedule
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public ObservableCollection<Tuple<string, string>> Routes { get; set; }
    }
}