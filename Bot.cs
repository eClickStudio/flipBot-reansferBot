using System;
using System.Collections.Generic;

namespace flipBot
{
    class Bot
    {
        //settings
        private bool isWork;
        private const byte MinWordLength = 1; //минимальная длина слова для бота
        private const byte SubtokenLength = 2; //минимальная длина слова для выделения основной части
        private const float ThresholdWord = 0.45f; // коэффициент равенства слов; от 0 до 1; 0 - все слова распознаются как одинаковые; 1 - слова считаются одинаковыми, если полностью идентичны
        private const float ThresholdSentence = 0.55f; // коэффициент равенства предложений; от 0 до 1; 0 - все предложения распознаются как одинаковые; 1 - предложения считаются одинаковыми, если полностью идентичны
        //keyWords
        private readonly string[] questionKeys = { "где", "зачем", "из-за чего", "изза чего", "из за чего", "какой", "какая", "какие", "когда", "как", "кто", "который", "как часто", "кому", "отчего", "откуда", "почему", "сколько", "чего", "что", "чей" };
        private readonly string[] botKeys = { "bot", "flip bot", "flipbot", "ассистент", "бот", "ботяра", "ботик", "ботяня", "ботя", "ботсман", "помошник", "робот", "флип бот", "чат бот" };
        private readonly string[] organizeCommands = { "задачи", "задания", "дай задания", "организуй", "организуй работу", "распредели", "распредели обязанности" };
        private readonly string[] doneCommands = { "выполнил", "завершил", "закончил", "готово", "сделал" };
        private readonly string[] botAnswers = {"Весь во внимании.", "Да, я слушаю.", "Чем я могу Вам помочь?", "Я Вас слушаю.", "Я здесь.", "Что желаете?" };
        private readonly string[] botNotes = { "добавь заметку", "добавь напоминание", "добавь запись", "запиши заметку", "запиши напоминание", "запиши запись" };
        private readonly string[] printNotes = { "заметки", "напоминание", "запись"};
        private readonly string[] botSettings = { "настройки", "параметры"};
        //reansfers
        private DynamicList<Reansfer> reansfers;
        private bool findAnsferForQuestion = false;
        private string question = "";
        //tasks
        private List<Task> tasks;
        private byte maxTasksCount;
        //notes
        private DynamicList<string> notes;
        private Random rand = new Random();
        public Bot(byte MaxReansfersCount, byte MaxNotesCount, byte maxTasksCount)
        {
            isWork = true;
            reansfers = new DynamicList<Reansfer>(MaxReansfersCount);
            notes = new DynamicList<string>(MaxNotesCount);
            this.maxTasksCount = maxTasksCount;
            tasks = new List<Task>();
        }

        public void Start()
        {
            Console.WriteLine("Бот запущен.");
            while (true)
            {
                string message = Console.ReadLine().ToLower();
                if (message.Length > MinWordLength)
                {
                    if (isWork)
                    {
                        string[] messageWords = ConvertToWords(message);
                        string questionKey = isKeyWord(message, questionKeys);
                        if (questionKey != null && questionKey == messageWords[0] || message.Contains("?")) // именно вопрос
                        {
                            //Поиск готового ответа на готовый вопрос
                            bool flag = false;
                            foreach (Reansfer reans in reansfers.list)
                            {
                                if (QuestionEqualValue(reans, message) >= ThresholdSentence)
                                {
                                    Console.WriteLine("Нашёл ответ: ");
                                    Console.WriteLine(reans.ansfer);
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                continue;
                            }
                            if (!findAnsferForQuestion)//не ищет ответ
                            {
                                //Сохранение вопроса
                                question = message;
                                findAnsferForQuestion = true;
                                continue;
                            }
                        }
                        string botKey = isKeyWord(message, botKeys);
                        if (botKey != null)
                        {
                            string normalizedSentence = NormalizeSentence(message.Replace(botKey, ""));
                            //обращение к боту                        
                            if (isKeyWord(normalizedSentence, organizeCommands) != null)
                            {
                                //организовать работу  
                                if (tasks.Count > 0)
                                {
                                    Console.WriteLine(PrintTasks(tasks));
                                }
                                else
                                {
                                    Console.WriteLine("Введите задачу в таком формате - [имя] [задача]. Если задачи закончились, введите готово или все.");
                                    while (true)
                                    {
                                        string task = Console.ReadLine();
                                        if (isWordsEqual(task, "все") || isWordsEqual(task, "готово"))
                                        {
                                            break;
                                        }
                                        else if (tasks.Count + 1 <= maxTasksCount)
                                        {
                                            string normalizeTask = NormalizeSentence(task);
                                            string[] words = ConvertToWords(normalizeTask);
                                            if (words.Length > 1)
                                            {
                                                string name = words[0];
                                                string currentTask = normalizeTask.Replace(name, "");
                                                tasks.Add(new Task(name, currentTask));
                                            }
                                            else
                                            {
                                                Console.WriteLine("Ошибка при вводе! Введите данные в верном формате.");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Достигнуто максимальное количество задач. (20)");
                                            break;
                                        }
                                    }
                                    Console.WriteLine("Сохранил задачи.");
                                    Console.WriteLine(PrintTasks(tasks));
                                    Console.WriteLine("Если кто-то выполнил задачу, пишите в таком формате - бот [имя] выполнил.");
                                }
                            }
                            else if (isKeyWord(normalizedSentence, doneCommands) != null)
                            {
                                string[] words = ConvertToWords(normalizedSentence);
                                string name = words[0];
                                bool flag = false;
                                for (byte i = 0; i < tasks.Count; i++)
                                {
                                    if (isWordsEqual(tasks[i].name, name))
                                    {
                                        tasks.Remove(tasks[i]);
                                        flag = true;
                                        break;
                                    }
                                }
                                if (flag)
                                {
                                    Console.WriteLine(PrintTasks(tasks));
                                }
                                else
                                {
                                    Console.WriteLine("Задача для этого человека не существует.");
                                }
                            }
                            else
                            {
                                string noteKey = isKeyWord(normalizedSentence, botNotes);
                                if (noteKey != null)
                                {
                                    Console.WriteLine("Напишите заметку:");
                                    string note = Console.ReadLine();
                                    notes.Add(note);
                                }
                                else
                                {
                                    string printNoteKey = isKeyWord(normalizedSentence, printNotes);
                                    if (printNoteKey != null)
                                    {
                                        Console.WriteLine(PrintNotes(notes.list));
                                    }
                                    else if (isKeyWord(normalizedSentence, botSettings) != null)
                                    {
                                        Console.WriteLine("Настройки.");
                                        Console.WriteLine("Введите следующие команды чтобы:");
                                        Console.WriteLine("stop) остановить,");
                                        Console.WriteLine("maxReansCount) задать максимальное число сохранённых ответов,");
                                        Console.WriteLine("maxTasksCount) задать максимальное число задач,");
                                        Console.WriteLine("maxNotesCount) задать максимальное число заметок,");
                                        Console.WriteLine("exit) выйти из настроек.");
                                        string command = "";
                                        while (command != "exit" && command != "stop") 
                                        {
                                            command = Console.ReadLine().ToLower();
                                            switch (command)
                                            {
                                                case "stop":
                                                    isWork = false;
                                                    Console.WriteLine("Бот приостановлен. Чтобы снова запустить его введите start.");
                                                    break;
                                                case "maxreanscount":
                                                    reansfers.maxCount = Settings("сохранённых ответов", reansfers.maxCount);
                                                    break;
                                                case "maxtaskscount":
                                                    maxTasksCount = Settings("задач", maxTasksCount);
                                                    break;
                                                case "maxnotescount":
                                                    notes.maxCount = Settings("заметок", notes.maxCount);
                                                    break;
                                                case "exit":
                                                    break;
                                                default:
                                                    Console.WriteLine("Неверная комнда.");
                                                    break;
                                            }
                                        }
                                        Console.WriteLine("Вы вышли из настроек.");
                                    }
                                    else
                                    {
                                        Random rand = new Random();
                                        Console.WriteLine(botAnswers[rand.Next(0, 6)]);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (findAnsferForQuestion && !message.Contains("?"))
                            {
                                Console.WriteLine("Ответ сохранён.");
                                findAnsferForQuestion = false;
                                reansfers.Add(new Reansfer(question, message, ConvertToWords(NormalizeSentence(question)), questionKey));
                            }
                        }
                    }
                    else
                    {
                        if (message == "start")
                        {
                            Console.WriteLine("Бот запущен.");
                            isWork = true;
                        }
                    }
                }
            }
        }
        private byte Settings(string phraze, byte startValue)
        {
            byte endValue;
            Console.WriteLine($"Максимальное число {phraze} сейчас = {startValue}.");
            Console.WriteLine($"Введите максимальное число {phraze} < 50.");
            Console.WriteLine("Помните, что это влияет на производительность бота.");
            try
            {
                endValue = byte.Parse(Console.ReadLine());
                if (endValue <= 50)
                {
                    Console.WriteLine($"Максимальное число {phraze} = {endValue}");
                    return endValue;
                }
                else
                {
                    Console.WriteLine($"Максимальное число {phraze} должно быть меньше 50!");
                }
            }
            catch
            {
                Console.WriteLine("Неверное число!");
            }
            return startValue;
        }
        private string PrintTasks(List<Task> tasks)
        {
            string result = "";
            if (tasks.Count > 0)
            {
                result += "Текущие задачи: \n";
                foreach (Task task in tasks)
                {
                    result += $"{task.name} - задача:{task.task}\n";
                }
                return result;
            }
            else
            {
                return "Все задачи выполнены!";
            }
        }
        private string PrintNotes(List<string> array)
        {
            string result = "";
            if (array.Count > 0)
            {
                result += "Текущие задачи: \n";
                for (byte i = 0; i < array.Count; i++)
                {
                    result += $"{i+1}) {array[i]}\n";
                }
                return result;
            }
            else
            {
                return "Заметок нет";
            }
        }
        private string isKeyWord(string mainStr, string[] keyWords)
        {
            string[] words = ConvertToWords(mainStr);
            foreach (string key in keyWords)
            {
                foreach (string word in words)
                {
                    if (isWordsEqual(key, word))
                    {
                        return key;
                    }
                }
            }
            return null;
        }
        private string NormalizeSentence(string sentence)
        {
            string resultContainer = "";
            string lowerSentece = sentence.ToLower();
            foreach (char c in lowerSentece)
            {
                if (IsNormalChar(c))
                {
                    resultContainer += c.ToString();
                }
            }
            return resultContainer;
        }
        private bool IsNormalChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == ' ';
        }
        private string[] ConvertToWords(string sentence)
        {
            List<string> tokens = new List<string>();
            string[] words = sentence.Split(' ');
            foreach (string word in words)
            {
                if (word.Length >= MinWordLength && word.Length < 10)
                {
                    tokens.Add(word);
                }
            }
            return tokens.ToArray();
        }
        private bool isWordsEqual(string firstToken, string secondToken)
        {
            byte equalSubtokensCount = 0;
            bool[] usedTokens = new bool[secondToken.Length - SubtokenLength + 1];
            for (byte i = 0; i < firstToken.Length - SubtokenLength + 1; ++i)
            {
                string subtokenFirst = firstToken.Substring(i, SubtokenLength);
                for (byte j = 0; j < secondToken.Length - SubtokenLength + 1; ++j)
                {
                    if (!usedTokens[j])
                    {
                        string subtokenSecond = secondToken.Substring(j, SubtokenLength);
                        if (subtokenFirst.Equals(subtokenSecond))
                        {
                            equalSubtokensCount++;
                            usedTokens[j] = true;
                            break;
                        }
                    }
                }
            }
            byte subtokenFirstCount = (byte)(firstToken.Length - SubtokenLength + 1);
            byte subtokenSecondCount = (byte)(secondToken.Length - SubtokenLength + 1);
            float tanimoto = (1.0f * equalSubtokensCount) / (subtokenFirstCount + subtokenSecondCount - equalSubtokensCount);
            return ThresholdWord <= tanimoto;
        }
        public float QuestionEqualValue(Reansfer reansfer, string second)
        {
            string first = reansfer.question;
            if (string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(second))
            {
                return 1.0f;
            }
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second))
            {
                return 0.0f;
            }
            string normalizedSecond = NormalizeSentence(second);
            string[] tokensSecond = ConvertToWords(normalizedSecond);
            string[] fuzzyEqualsTokens = GetFuzzyEqualsTokens(reansfer.questionWords, tokensSecond);
            byte equalsCount = (byte)(fuzzyEqualsTokens.Length);
            byte firstCount = (byte)(reansfer.questionWords.Length);
            byte secondCount = (byte)(tokensSecond.Length);
            //Console.WriteLine("result value = " + resultValue);
            float resultValue;
            foreach (string word in fuzzyEqualsTokens)
            {
                if (reansfer.questionKey != null && isWordsEqual(word, reansfer.questionKey))
                {
                    resultValue = (1.25f * equalsCount) / (firstCount + secondCount - equalsCount);
                    return resultValue;
                }
            }
            resultValue = (1.0f * equalsCount) / (firstCount + secondCount - equalsCount);
            return resultValue;
        }
        private string[] GetFuzzyEqualsTokens(string[] tokensFirst, string[] tokensSecond)
        {
            List<string> equalsTokens = new List<string>();
            bool[] usedToken = new bool[tokensSecond.Length];
            for (byte i = 0; i < tokensFirst.Length; ++i)
            {
                for (byte j = 0; j < tokensSecond.Length; ++j)
                {
                    if (!usedToken[j])
                    {
                        if (isWordsEqual(tokensFirst[i], tokensSecond[j]))
                        {
                            equalsTokens.Add(tokensFirst[i]);
                            usedToken[j] = true;
                            break;
                        }
                    }
                }
            }
            return equalsTokens.ToArray();
        }
    }
}

