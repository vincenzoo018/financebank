using System;
using BCrypt.Net;

class Program {
    static void Main() {
        Console.WriteLine(""Testing BCrypt Verification:"");
        Console.WriteLine();
        
        var tests = new[] {
            new { Username = ""superadmin2024"", Password = ""Admin@2024!Secure"", Hash = """" },
            new { Username = ""accountant2024"", Password = ""Account#2024"", Hash = "".x2MoCq.gTVzEfCf42POyDr4d/xu0cKCbjC045pti5MzVOgN5xa"" },
            new { Username = ""fmanager2024"", Password = ""Finance#Mgr!"", Hash = ""/3DF/Fj7CA6ik6SvIyluxZMmRWg/nW.dooH5FcDHvUK8DHUwqKa"" },
            new { Username = ""teller2024"", Password = ""Teller@2024!Bank#"", Hash = ""/NwoClgTJy6Pc.BGGBRIol0OisSlulBVlcLNJcZmsJ6SC"" }
        };
        
        foreach (var test in tests) {
            bool isValid = BCrypt.Net.BCrypt.Verify(test.Password, test.Hash);
            Console.WriteLine($""{test.Username} / {test.Password}"");
            Console.WriteLine($""  Hash: {test.Hash}"");
            Console.WriteLine($""  Valid: {isValid}"");
            Console.WriteLine();
        }
    }
}
