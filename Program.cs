using Euroins.Tools.Validators;

var validator = new EgnValidator();

while (true)
{
    Console.WriteLine("Choose:\n1 - Validate EGN\n2 - Generate EGN");
    string? choice = Console.ReadLine();

    if (choice == "1")
    {
        Console.WriteLine("Enter EGN:");
        string? input = Console.ReadLine();

        try
        {
            var info = validator.Parse(input!);
            Console.WriteLine(info);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    else if (choice == "2")
    {
        Console.WriteLine("Enter birth date (dd.MM.yyyy):");
        var dateInput = Console.ReadLine();

        if (!DateTime.TryParseExact(dateInput, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var birthDate))
        {
            Console.WriteLine("Invalid date format.");
            return;
        }

        Console.WriteLine("Enter sex (M/F):");
        var sexInput = Console.ReadLine()?.Trim().ToUpper();

        if (sexInput != "M" && sexInput != "F")
        {
            Console.WriteLine("Invalid sex. Enter 'M' or 'F'.");
            return;
        }

        Console.WriteLine("Enter region name (e.g., Sofia - grad):");
        var regionName = Console.ReadLine();

        var validators = new EgnValidator();
        var allEGNs = validator.GenerateAll(birthDate, sexInput, regionName);

        Console.WriteLine($"\nFound {allEGNs.Count} valid EGNs:");
        foreach (var egn in allEGNs)
        {
            Console.WriteLine(egn);
        }

    }
}
