namespace flipBot
{
    class Reansfer
    {
        public readonly string question;
        public readonly string questionKey;
        public readonly string[] questionWords;
        public readonly string ansfer;
        public Reansfer(string question, string ansfer, string[] questionWords, string questionKey = null)
        {
            this.question = question.ToLower();
            this.questionKey = questionKey;
            this.questionWords = questionWords;
            this.ansfer = ansfer;
            this.questionWords = question.Split(' ');
        }
    }
}
