using MediCare.Data.Entities;
using MediCare.Data.Enums;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Common;
using MediCare.Services.DTOs;
using MediCare.Services.Interfaces;

namespace MediCare.Services.Implementations;

/// <summary>Implements <see cref="IPrescriptionService"/>; see the interface for what each member does.</summary>
public class PrescriptionService : IPrescriptionService
{
    private readonly IUnitOfWork _unitOfWork;

    public PrescriptionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> AddAsync(
        int doctorId, int appointmentId,
        string medicationName, string dosage, string frequency, int durationDays)
    {
        medicationName = medicationName?.Trim() ?? string.Empty;
        dosage = dosage?.Trim() ?? string.Empty;
        frequency = frequency?.Trim() ?? string.Empty;

        if (medicationName.Length == 0 || dosage.Length == 0 || frequency.Length == 0)
            return ServiceResult.Fail("Medication name, dosage and frequency are required.");

        if (medicationName.Length > 150 || dosage.Length > 100 || frequency.Length > 100)
            return ServiceResult.Fail("One of the fields is too long.");

        if (durationDays <= 0)
            return ServiceResult.Fail("Duration must be at least one day.");

        var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
        if (appointment is null || appointment.DoctorProfileId != doctorId)
            return ServiceResult.Fail("Appointment not found.");

        if (appointment.Status != AppointmentStatus.Completed)
            return ServiceResult.Fail("Prescriptions can only be added to completed appointments.");

        await _unitOfWork.Prescriptions.AddAsync(new Prescription
        {
            AppointmentId = appointmentId,
            MedicationName = medicationName,
            Dosage = dosage,
            Frequency = frequency,
            DurationDays = durationDays
        });
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<IEnumerable<PrescriptionDto>> GetForAppointmentAsync(int doctorId, int appointmentId)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
        if (appointment is null || appointment.DoctorProfileId != doctorId)
            return Enumerable.Empty<PrescriptionDto>();

        var items = await _unitOfWork.Prescriptions.FindAsync(p => p.AppointmentId == appointmentId);

        return items.Select(p => new PrescriptionDto
        {
            Id = p.Id,
            MedicationName = p.MedicationName,
            Dosage = p.Dosage,
            Frequency = p.Frequency,
            DurationDays = p.DurationDays
        });
    }

    public async Task<IEnumerable<PrescriptionGroupDto>> GetForPatientAsync(string patientId)
    {
        var appointments = (await _unitOfWork.Appointments.FindAsync(
            a => a.PatientId == patientId && a.Status == AppointmentStatus.Completed)).ToList();

        if (appointments.Count == 0)
            return Enumerable.Empty<PrescriptionGroupDto>();

        var appointmentIds = appointments.Select(a => a.Id).ToList();
        var items = (await _unitOfWork.Prescriptions.FindAsync(
            p => appointmentIds.Contains(p.AppointmentId))).ToList();

        var doctorIds = appointments.Select(a => a.DoctorProfileId).Distinct().ToList();
        var doctors = await _unitOfWork.Doctors.FindAsync(d => doctorIds.Contains(d.Id), d => d.User);
        var doctorNames = doctors.ToDictionary(d => d.Id, d => d.User.FullName);

        return appointments
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new PrescriptionGroupDto
            {
                AppointmentId = a.Id,
                DoctorName = doctorNames.GetValueOrDefault(a.DoctorProfileId) ?? string.Empty,
                Date = a.AppointmentDate,
                Items = items
                    .Where(p => p.AppointmentId == a.Id)
                    .Select(p => new PrescriptionDto
                    {
                        Id = p.Id,
                        MedicationName = p.MedicationName,
                        Dosage = p.Dosage,
                        Frequency = p.Frequency,
                        DurationDays = p.DurationDays
                    })
                    .ToList()
            })
            .Where(g => g.Items.Count > 0)
            .ToList();
    }
}