namespace NETworkManager.Models.Network
{
    public class DNSLookupCompleteArgs : System.EventArgs
    {
        public string ServerAndPort { get; set; }
        public int QuestionsCount { get; set; }
        public int AnswersCount { get; set; }
        public int AuthoritiesCount { get; set; }
        public int AdditionalsCount { get; set; }
        public int MessageSize { get; set; }

        public DNSLookupCompleteArgs()
        {

        }

        public DNSLookupCompleteArgs(string serverAndPort, int questionsCount, int answersCount, int authoritiesCount, int additionalsCount, int messageSize)
        {
            ServerAndPort = serverAndPort;
            QuestionsCount = questionsCount;
            AnswersCount = answersCount;
            AuthoritiesCount = authoritiesCount;
            AdditionalsCount = additionalsCount;
            MessageSize = messageSize;
        }
    }
}
