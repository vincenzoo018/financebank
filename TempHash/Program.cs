using System;
using BCrypt.Net;

class Program {
    static void Main() {
        Console.WriteLine("=== FRESH BCRYPT HASHES ===");
        Console.WriteLine();

        var hash1 = BCrypt.Net.BCrypt.HashPassword("Admin@2024!Secure", 12);
        Console.WriteLine("superadmin2024 / Admin@2024!Secure");
        Console.WriteLine(hash1);
        Console.WriteLine();

        var hash2 = BCrypt.Net.BCrypt.HashPassword("Account#2024$Pass", 12);
        Console.WriteLine("accountant2024 / Account#2024$Pass");
        Console.WriteLine(hash2);
        Console.WriteLine();

        var hash3 = BCrypt.Net.BCrypt.HashPassword("Finance$2024#Mgr!", 12);
        Console.WriteLine("fmanager2024 / Finance$2024#Mgr!");
        Console.WriteLine(hash3);
        Console.WriteLine();

        var hash4 = BCrypt.Net.BCrypt.HashPassword("Teller@2024!Bank#", 12);
        Console.WriteLine("teller2024 / Teller@2024!Bank#");
        Console.WriteLine(hash4);
    }
}
