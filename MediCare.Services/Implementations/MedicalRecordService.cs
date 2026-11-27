using MediCare.Data.Entities;
using MediCare.Data.Enums;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.Common;
using MediCare.Services.DTOs;
using MediCare.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Services.Implementations;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IUnitOfWork _unitOfWork;

    public MedicalRecordService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> SaveAsync(
        int doctorId, int appointmentId, string diagnosis, string? notes)
    {
        diagnosis = diagnosis?.Trim() ?? string.Empty;
        notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        if (diagnosis.Length == 0)
            return ServiceResult.Fail("Diagnosis is required.");

        if (diagnosis.Length > 500)
            return ServiceResult.Fail("Diagnosis is too long (maximum 500 characters).");

        if (notes is { Length: > 2000 })
            return ServiceResult.Fail("Notes are too long (maximum 2000 characters).");

        var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
        if (appointment is null || appointment.DoctorProfileId != doctorId)
            return ServiceResult.Fail("Appointment not found.");

        if (appointment.Status != AppointmentStatus.Completed)
            return ServiceResult.Fail("Medical records can only be written for completed appointments.");

        var existing = (await _unitOfWork.MedicalRecords
            .FindAsync(m => m.AppointmentId == appointmentId)).FirstOrDefault();

        if (existing is null)
        {
            await _unitOfWork.MedicalRecords.AddAsync(new MedicalRecord
            {
                AppointmentId = appointmentId,
                Diagnosis = diagnosis,
                Notes = notes
            });
        }
        else
        {
            existing.Diagnosis = diagnosis;
            existing.Notes = notes;
            existing.UpdatedAtUtc = DateTime.UtcNow;
            _unitOfWork.MedicalRecords.Update(existing);
        }

        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Fail("The record was changed at the same time. Please reload and try again.");
        }

        return ServiceResult.Ok();
    }

    public async Task<MedicalRecordDto?> GetForDoctorAsync(int doctorId, int appointmentId)
    {
        var appointment = await _unitOfWork.Appointments.GetByIdAsync(appointmentId);
        if (appointment is null || appointment.DoctorProfileId != doctorId)
            return null;

        var record = (await _unitOfWork.MedicalRecords
            .FindAsync(m => m.AppointmentId == appointmentId)).FirstOrDefault();

        if (record is null)
            return null;

        return new MedicalRecordDto
        {
            AppointmentId = appointmentId,
            Date = appointment.AppointmentDate,
            Diagnosis = record.Diagnosis,
            Notes = record.Notes,
            CreatedAtUtc = record.CreatedAtUtc,
            UpdatedAtUtc = record.UpdatedAtUtc
        };
    }

    public async Task<IEnumerable<MedicalRecordDto>> GetForPatientAsync(string patientId)
    {
        var appointments = (await _unitOfWork.Appointments.FindAsync(
            a => a.PatientId == patientId && a.Status == AppointmentStatus.Completed)).ToList();

        if (appointments.Count == 0)
            return Enumerable.Empty<MedicalRecordDto>();

        var appointmentIds = appointments.Select(a => a.Id).ToList();
        var records = (await _unitOfWork.MedicalRecords
            .FindAsync(m => appointmentIds.Contains(m.AppointmentId))).ToList();

        var doctorIds = appointments.Select(a => a.DoctorProfileId).Distinct().ToList();
        var doctors = await _unitOfWork.Doctors.FindAsync(d => doctorIds.Contains(d.Id), d => d.User);
        var doctorNames = doctors.ToDictionary(d => d.Id, d => d.User.FullName);

        return records
            .Select(r =>
            {
                var a = appointments.First(x => x.Id == r.AppointmentId);
                return new MedicalRecordDto
                {
                    AppointmentId = a.Id,
                    DoctorName = doctorNames.GetValueOrDefault(a.DoctorProfileId) ?? string.Empty,
                    Date = a.AppointmentDate,
                    Diagnosis = r.Diagnosis,
                    Notes = r.Notes,
                    CreatedAtUtc = r.CreatedAtUtc,
                    UpdatedAtUtc = r.UpdatedAtUtc
                };
            })
            .OrderByDescending(d => d.Date)
            .ToList();
    }
}