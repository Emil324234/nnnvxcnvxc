using Newtonsoft.Json;
using System.Diagnostics;


class TestNaPechatanie
{
    static void Main()
    {
        bool repeatTest = true;
        while (repeatTest)
        {
            RunTypingTest();
            Console.WriteLine("Хотите пройти тест еще раз? (да/нет):");
            string choice = Console.ReadLine();
            if (choice.ToLower() != "да")
            {
                repeatTest = false;
            }
        }
    }

    static void RunTypingTest()
    {
        Console.Clear(); 
        string text = "Какого размера у слона грусть?";
        Console.WriteLine("Текст для набора: " + text);
        Console.WriteLine("Печатайте:");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ResetColor();

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        string input = string.Empty;
        ManualResetEvent inputFinishedEvent = new ManualResetEvent(false);
        double tochnostb = 0;

        Thread readInputThread = new Thread(() =>
        {
            input = Console.ReadLine();
            tochnostb = CalculateAccuracy(text, input);
            inputFinishedEvent.Set();
        });

        readInputThread.Start();

        while (!inputFinishedEvent.WaitOne(0) && stopwatch.Elapsed.TotalSeconds < 60)
        {
            Console.SetCursorPosition(0, 2);
            Console.WriteLine("Времени осталось: " + (60 - stopwatch.Elapsed.TotalSeconds).ToString("F5") + " сек");
            Thread.Sleep(50);
        }

        if (!inputFinishedEvent.WaitOne(0))
        {
            readInputThread.Abort();
            Console.WriteLine("Время вышло!");
        }

        readInputThread.Join();
        Console.WriteLine("Точность: " + tochnostb.ToString("F5") + "%");

        Console.WriteLine("Введите ваше имя:");
        string name = Console.ReadLine();

        Record record = new Record(name, tochnostb);
        RecordsTable.AddRecord(record);
        Console.WriteLine("Таблица рекордов:");
        RecordsTable.PrintTable();
    }

    static double CalculateAccuracy(string text, string input)
    {
        int errors = 0;
        for (int i = 0; i < text.Length && i < input.Length; i++)
        {
            if (text[i] != input[i])
            {
                errors++;
            }
        }
        return ((double)(text.Length - errors) / text.Length) * 100.0;
    }
}

class RecordsTable
{
    private static List<Record> records = LoadRecords();

    public static void AddRecord(Record record)
    {
        records.Add(record);
        records.Sort((x, y) => y.Tochnostb.CompareTo(x.Tochnostb));
        SohranenieRecordov();
    }

    public static void PrintTable()
    {
        Console.WriteLine("Имя\t\tТочность\tСимволов в минуту\tСимволов в секунду");
        foreach (Record record in records)
        {
            double simvolovvminutu = Math.Round(record.Tochnostb * 12); 
            double simvolovvsekundu = Math.Round(simvolovvminutu / 60);
            Console.WriteLine(record.Name + "\t\t" + record.Tochnostb.ToString("F2") + "%\t\t" + simvolovvminutu.ToString() + "\t\t" + simvolovvsekundu.ToString());
        }
    }

    private static void SohranenieRecordov()
    {
        string json = JsonConvert.SerializeObject(records);
        File.WriteAllText("records.json", json);
    }

    private static List<Record> LoadRecords()
    {
        if (File.Exists("records.json"))
        {
            string json = File.ReadAllText("records.json");
            return JsonConvert.DeserializeObject<List<Record>>(json);
        }
        else
        {
            return new List<Record>();
        }
    }
}

class Record
{
    public string Name { get; }
    public double Tochnostb { get; }

    public Record(string name, double tochnostb)
    {
        Name = name;
        Tochnostb = tochnostb;
    }
}

