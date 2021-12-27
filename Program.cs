namespace flipBot
{
    class Program
    {
        private static Bot flipBot = new Bot(5, 5, 20);

        static void Main(string[] args)
        {
            flipBot.Start();
        }
    }
}
