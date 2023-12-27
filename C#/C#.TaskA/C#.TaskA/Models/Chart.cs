namespace C_.TaskA.Models
{
    public class Chart
    {
        public string Type { get; set; }
        public Data Data { get; set; }
    }

    public class Data
    {
        public List<string> Labels { get; set; }
        public List<Dataset> Datasets { get; set; }
    }

    public class Dataset
    {
        public List<double> Data { get; set; }
        public List<string> BackgroundColor { get; set; }
    }
}
