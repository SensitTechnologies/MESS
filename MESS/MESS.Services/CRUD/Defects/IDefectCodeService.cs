using MESS.Services.DTOs.Defects;
using MESS.Services.DTOs.FailureAdjectives;
using MESS.Services.DTOs.FailureNouns;

namespace MESS.Services.CRUD.Defects;

/// <summary>
/// Failure noun/adjective catalog and work-instruction scoping for production defect codes.
/// </summary>
public interface IDefectCodeService
{
    /// <summary>Returns failure nouns linked to the work instruction.</summary>
    Task<IReadOnlyList<DefectCodeOptionDto>> GetNounsForWorkInstructionAsync(int workInstructionId, CancellationToken cancellationToken = default);

    /// <summary>Returns adjectives linked to the noun via the noun–adjective association.</summary>
    Task<IReadOnlyList<DefectCodeOptionDto>> GetAdjectivesForNounAsync(int nounId, CancellationToken cancellationToken = default);

    /// <summary>All nouns with adjective id associations (admin).</summary>
    Task<IReadOnlyList<FailureNounAdminDto>> GetAllNounsAsync(CancellationToken cancellationToken = default);

    /// <summary>All adjectives with noun id associations (admin).</summary>
    Task<IReadOnlyList<FailureAdjectiveAdminDto>> GetAllAdjectivesAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a noun and optional adjective links.</summary>
    Task<int> CreateNounAsync(FailureNounCreateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates a noun and replaces adjective links.</summary>
    Task UpdateNounAsync(FailureNounUpdateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a noun if unused critical paths allow.</summary>
    Task DeleteNounAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Creates an adjective and optional noun links.</summary>
    Task<int> CreateAdjectiveAsync(FailureAdjectiveCreateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Updates an adjective and replaces noun links.</summary>
    Task UpdateAdjectiveAsync(FailureAdjectiveUpdateRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deletes an adjective.</summary>
    Task DeleteAdjectiveAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Noun ids currently linked to the work instruction.</summary>
    Task<IReadOnlyList<int>> GetNounIdsForWorkInstructionAsync(int workInstructionId, CancellationToken cancellationToken = default);

    /// <summary>Replaces noun links for the work instruction.</summary>
    Task SetNounsForWorkInstructionAsync(int workInstructionId, IReadOnlyList<int> nounIds, CancellationToken cancellationToken = default);
}
