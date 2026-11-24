using MediCare.Data.Context;
using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.DoctorProfiles.AnyAsync())
            return;

        var doctors = new[]
        {
            ("Dr. Ahmed Hassan", "ahmed.hassan@medicare.test", "Cardiology", "12 Tahrir St, Cairo", 400m, true),
            ("Dr. Sara Mostafa", "sara.mostafa@medicare.test", "Dermatology", "5 Nile Corniche, Giza", 300m, true),
            ("Dr. Omar Khaled", "omar.khaled@medicare.test", "Pediatrics", "8 El-Geish St, Alexandria", 250m, true),
            ("Dr. Mona Adel", "mona.adel@medicare.test", "Cardiology", "20 Mansoura St, Mansoura", 350m, false)
        };

        foreach (var (fullName, email, specialty, address, fee, approved) in doctors)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                NormalizedUserName = email.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                FullName = fullName
            };

            context.Users.Add(user);
            context.DoctorProfiles.Add(new DoctorProfile
            {
                User = user,
                Specialty = specialty,
                ClinicAddress = address,
                ConsultationFee = fee,
                IsApproved = approved
            });
        }

        await context.SaveChangesAsync();
    }
}