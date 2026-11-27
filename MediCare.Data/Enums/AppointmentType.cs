namespace MediCare.Data.Enums;

/// <summary>The kind of visit a patient books. <c>AppointmentFactory</c> turns each type into its own rules and price.</summary>
public enum AppointmentType
{
    /// <summary>A normal (first) examination. Full consultation fee.</summary>
    FirstVisit,

    /// <summary>A follow-up after a completed visit with the same doctor in the last 60 days. Half the fee.</summary>
    FollowUp,

    /// <summary>A short consultation or second opinion (no examination). 75% of the fee.</summary>
    Consultation
}
