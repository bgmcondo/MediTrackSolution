using MediTrack.Dtos.AuthDto;

namespace MediTrack.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterPatientAsync(RegisterPatientDto registerPatientDto);
        Task<RegisterResponseDto> RegisterDoctorAsync(RegisterDoctorDto registerDoctorDto);
        Task<LoginResponseDto> LoginAsync(LoginDto loginDto);

        Task<RegisterResponseDto> RegisterAdminAsync(RegisterAdminDto registerAdminDto);

    }
}
