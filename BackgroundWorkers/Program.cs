namespace BackgroundWorkers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Goober.WebApi.ProgramUtils.RunWebhost<Startup>(args);
        }
    }
}
