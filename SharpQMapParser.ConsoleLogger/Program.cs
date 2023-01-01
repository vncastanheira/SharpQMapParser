// See https://aka.ms/new-console-template for more information
using SharpQMapParser;
using System.Diagnostics;

if (args.Length> 0)
{
	if (File.Exists(args[0]))
	{
		var path = args[0];
		var map = new Map();
		try
		{
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            using (StreamReader streamReader = File.OpenText(path))
			{
				map.Parse(streamReader);
			}
			stopWatch.Stop();

            Console.WriteLine("Map parsed sucessfully.");
			Console.WriteLine($"Entities found: {map.Entities.Count}");
			var worldspawn = map.Entities.Find(e => e.ClassName == "worldspawn");
            Console.WriteLine($"Brushes found: {worldspawn.Brushes.Count}");
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
            Console.WriteLine("Parsing Runtime: " + elapsedTime);

        }
		catch (Exception ex)
		{
			Console.Write("\n" + ex.Message);
		}
	}
	else
	{
        Console.WriteLine($"Invalid .map file on ´{args[0]}´.");
    }
}
else
{
    Console.WriteLine(".map file missing.");
}

Console.WriteLine("\nPress anything to close...");
Console.ReadKey();

