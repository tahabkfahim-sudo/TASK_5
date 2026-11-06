namespace task_5
{
    public enum QuestionLevel
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }
    public abstract class Question
    {
        public string Header { get; set; }
        public int Marks { get; set; }
        public QuestionLevel Level { get; set; }
        protected Question(string header, int marks, QuestionLevel level)
        {
            Header = header;
            Marks = marks;
            Level = level;
        }
        public abstract void Display();
        public abstract string AnswerHint { get; }
        public abstract bool IsValidAnswer(string input);
        public abstract bool CheckAnswer(string input);
        protected void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[{Level}] {Header}  ({Marks} marks)");
            Console.ResetColor();
        }
    }
    public class TrueFalseQuestion : Question
    {
        public bool CorrectAnswer { get; set; }

        public TrueFalseQuestion(string header, int marks, QuestionLevel level, bool correctAnswer)
            : base(header, marks, level)
        {
            CorrectAnswer = correctAnswer;
        }
        public override string AnswerHint => "Enter T for True or F for False";
        public override void Display()
        {
            PrintHeader();
            Console.WriteLine("  1) True");
            Console.WriteLine("  2) False");
        }
        public override bool IsValidAnswer(string input)
        {
            string a = input.Trim().ToLower();
            return a == "t" || a == "f" || a == "true" || a == "false" || a == "1" || a == "2";
        }
        public override bool CheckAnswer(string input)
        {
            string a = input.Trim().ToLower();
            bool studentAnswer = a == "t" || a == "true" || a == "1";
            return studentAnswer == CorrectAnswer;
        }
    }
    public class ChooseOneQuestion : Question
    {
        public string[] Choices { get; set; }
        public int CorrectChoice { get; set; }

        public ChooseOneQuestion(string header, int marks, QuestionLevel level,
                                 string[] choices, int correctChoice)
            : base(header, marks, level)
        {
            Choices = choices;
            CorrectChoice = correctChoice;
        }

        public override string AnswerHint => "Enter the number of the correct choice (1-4)";

        public override void Display()
        {
            PrintHeader();
            for (int i = 0; i < Choices.Length; i++)
                Console.WriteLine($"  {i + 1}) {Choices[i]}");
        }
        public override bool IsValidAnswer(string input)
        {
            return int.TryParse(input.Trim(), out int n) && n >= 1 && n <= Choices.Length;
        }
        public override bool CheckAnswer(string input)
        {
            return int.TryParse(input.Trim(), out int n) && n == CorrectChoice;
        }
    }
    public class MultipleChoiceQuestion : Question
    {
        public string[] Choices { get; set; }
        public HashSet<int> CorrectChoices { get; set; }

        public MultipleChoiceQuestion(string header, int marks, QuestionLevel level,
                                      string[] choices, HashSet<int> correctChoices)
            : base(header, marks, level)
        {
            Choices = choices;
            CorrectChoices = correctChoices;
        }
        public override string AnswerHint => "Enter the correct choices separated by comma (e.g. 1,3)";

        public override void Display()
        {
            PrintHeader();
            for (int i = 0; i < Choices.Length; i++)
                Console.WriteLine($"  {i + 1}) {Choices[i]}");
        }
        public override bool IsValidAnswer(string input)
        {
            return TryParseChoices(input, Choices.Length, out _);
        }
        public override bool CheckAnswer(string input)
        {
            return TryParseChoices(input, Choices.Length, out var set) && set.SetEquals(CorrectChoices);
        }
        public static bool TryParseChoices(string input, int max, out HashSet<int> result)
        {
            result = new HashSet<int>();
            if (string.IsNullOrWhiteSpace(input)) return false;

            foreach (string part in input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(part.Trim(), out int n) || n < 1 || n > max)
                    return false;
                result.Add(n);
            }
            return result.Count > 0;
        }
    }
    class Program
    {
        static List<Question> questionBank = new List<Question>();

        static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("       EXAMINATION SYSTEM        ");
                Console.WriteLine("1. Doctor Mode");
                Console.WriteLine("2. Student Mode");
                Console.WriteLine("3. Exit");

                int choice = ReadInt("Choose: ", 1, 3);

                switch (choice)
                {
                    case 1: DoctorMode(); break;
                    case 2: StudentMode(); break;
                    case 3:
                        Console.WriteLine("Goodbye!");
                        return;
                }
            }
        }
        static void DoctorMode()
        {
            Console.Clear();
            Console.WriteLine("=========== DOCTOR MODE ===========");

            int count = ReadInt("How many questions do you want to add? ", 1, 100);

            for (int i = 1; i <= count; i++)
            {
                Console.WriteLine($"\n----- Question {i} of {count} -----");

                Console.WriteLine("Question type:");
                Console.WriteLine("1. True/False");
                Console.WriteLine("2. Choose One");
                Console.WriteLine("3. Multiple Choice");
                int type = ReadInt("Select type: ", 1, 3);

                QuestionLevel level = ReadLevel();
                string header = ReadNonEmpty("Question header: ");
                int marks = ReadInt("Marks: ", 1, 1000);

                Question q;

                if (type == 1)
                {
                    Console.WriteLine("Correct answer: 1) True  2) False");
                    bool correct = ReadInt("Answer: ", 1, 2) == 1;
                    q = new TrueFalseQuestion(header, marks, level, correct);
                }
                else if (type == 2)
                {
                    string[] choices = ReadChoices();
                    int correct = ReadInt("Correct choice number (1-4): ", 1, 4);
                    q = new ChooseOneQuestion(header, marks, level, choices, correct);
                }
                else
                {
                    string[] choices = ReadChoices();
                    HashSet<int> correct;
                    while (true)
                    {
                        Console.Write("Correct answers (comma separated, e.g. 1,3): ");
                        string input = Console.ReadLine() ?? "";
                        if (MultipleChoiceQuestion.TryParseChoices(input, 4, out correct))
                            break;
                        Console.WriteLine("Invalid input. Use numbers 1-4 separated by commas.");
                    }
                    q = new MultipleChoiceQuestion(header, marks, level, choices, correct);
                }

                questionBank.Add(q);
                Console.WriteLine("Question added successfully.");
            }

            Console.WriteLine("\nPress any key to return to the Main Menu...");
            Console.ReadKey();
        }
        static void StudentMode()
        {
            Console.Clear();
            Console.WriteLine("=========== STUDENT MODE ===========");

            if (questionBank.Count == 0)
            {
                Console.WriteLine("The question bank is empty. Ask the doctor to add questions first.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Exam type:");
            Console.WriteLine("1. Practical (half of the questions)");
            Console.WriteLine("2. Final (all questions)");
            int examType = ReadInt("Choose: ", 1, 2);

            QuestionLevel level = ReadLevel();

            List<Question> levelQuestions = questionBank.Where(q => q.Level == level).ToList();

            if (levelQuestions.Count == 0)
            {
                Console.WriteLine($"No questions available for level {level}.");
                Console.WriteLine("\nPress any key to return...");
                Console.ReadKey();
                return;
            }

            List<Question> exam;
            if (examType == 1)
            {
                int half = Math.Max(1, levelQuestions.Count / 2);
                exam = levelQuestions.Take(half).ToList();
            }
            else
            {
                exam = levelQuestions;
            }

            int score = 0;
            int totalMarks = exam.Sum(q => q.Marks);
            int number = 1;

            Console.Clear();
            Console.WriteLine($"=== {(examType == 1 ? "Practical" : "Final")} Exam - {level} ===\n");

            foreach (Question q in exam)
            {
                Console.WriteLine($"Q{number}:");
                q.Display();
                Console.WriteLine($"({q.AnswerHint})");

                string answer;
                while (true)
                {
                    Console.Write("Your answer: ");
                    answer = Console.ReadLine() ?? "";
                    if (q.IsValidAnswer(answer)) break;
                    Console.WriteLine("Invalid answer format, try again.");
                }

                if (q.CheckAnswer(answer))
                    score += q.Marks;

                Console.WriteLine();
                number++;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Your Result: {score} / {totalMarks}");
            Console.ResetColor();
            Console.WriteLine("=================================");

            Console.WriteLine("\nPress any key to return to the Main Menu...");
            Console.ReadKey();
        }
        static int ReadInt(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (int.TryParse(Console.ReadLine(), out int n) && n >= min && n <= max)
                    return n;
                Console.WriteLine($"Invalid input. Enter a number between {min} and {max}.");
            }
        }

        static string ReadNonEmpty(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string s = (Console.ReadLine() ?? "").Trim();
                if (s.Length > 0) return s;
                Console.WriteLine("This field cannot be empty.");
            }
        }
        static QuestionLevel ReadLevel()
        {
            Console.WriteLine("Level: 1) Easy  2) Medium  3) Hard");
            return (QuestionLevel)ReadInt("Select level: ", 1, 3);
        }
        static string[] ReadChoices()
        {
            string[] choices = new string[4];
            for (int i = 0; i < 4; i++)
                choices[i] = ReadNonEmpty($"Choice {i + 1}: ");
            return choices;
        }
    }
    
}
