using JJMasterData.SchemaGenerator.Writers;

while (true)
{
    Console.WriteLine("1) Create jjmasterdata.json");
    Console.WriteLine("2) Create a JSchema from assembly class.");
    Console.WriteLine("3) Exit.");

    Console.WriteLine();
    
    Console.Write("Selected Option: ");
    var selectedOption = Console.ReadKey();
    
    Console.WriteLine();
    
    switch (selectedOption.Key)
    {
        case ConsoleKey.NumPad1 or ConsoleKey.D1:
            await new JJMasterDataSettingsWriter().WriteAsync();
            break;
        case ConsoleKey.NumPad2 or ConsoleKey.D2:
            await new AssemblyClassWriter().WriteAsync(GetClassName());
            break;
        case ConsoleKey.NumPad3 or ConsoleKey.D3:
            return 0;
        default:
            Console.WriteLine("Invalid option.\n");
            break;
    }
}

string? GetClassName()
{
    Console.Write("Enter the class name: ");
    return Console.ReadLine();
}