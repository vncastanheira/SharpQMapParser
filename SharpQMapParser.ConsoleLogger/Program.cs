// See https://aka.ms/new-console-template for more information
using SharpQMapParser;

if (args.Length> 0)
{
	if (File.Exists(args[0]))
	{
		var path = args[0];
		var map = new Map();
		try
		{
			using (StreamReader streamReader = File.OpenText(path))
			{
				map.Parse(streamReader);
			}

			Console.WriteLine("Map parsed sucessfully.");
			Console.WriteLine($"Entities found: {map.Entities.Count}");
			var worldspawn = map.Entities.Find(e => e.ClassName == "worldspawn");
            Console.WriteLine($"Brushes found: {worldspawn.Brushes.Count}");

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

