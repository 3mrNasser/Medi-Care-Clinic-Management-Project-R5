using MediCare.Services.Common;
using MediCare.Services.DTOs;

namespace MediCare.Services.Interfaces;

public interface IPrescriptionService
{
    Task<ServiceResult> AddAsync(
        int doctorId, int appointmentId,
        string medicationName, string dosage, string frequency, int durationDays);

    Task<IEnumerable<PrescriptionDto>> GetForAppointmentAsync(int doctorId, int appointmentId);
    Task<IEnumerable<PrescriptionGroupDto>> GetForPatientAsync(string patientId);
}