namespace Platibus.SampleConsoleApp
{
    public class Ping
    {
        public string Sender { get; set; }
        public int Counter { get; set; }

        public Ping(string sender, int counter)
        {
            Sender = sender;
            Counter = counter;
        }
    }
}
