namespace MediCare.Services.Common;

public static class Specialties
{
    public static readonly IReadOnlyList<string> All = new[]
    {
        "Cardiology",
        "Dermatology",
        "Pediatrics",
        "General Practice",
        "Internal Medicine",
        "Orthopedics",
        "Neurology",
        "Ophthalmology",
        "ENT",
        "Dentistry",
        "Gynecology",
        "Psychiatry",
        "Urology"
    };

    public static bool IsValid(string? value) => value is not null && All.Contains(value);
}