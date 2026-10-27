namespace MediTrack.Dtos.AuthDto
{
    public class RegisterResponseDto
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;

        public string Message { get; set; } = null!;

        public bool IsSuccess { get; set; }

        public List<string> Errors { get; set; } = new();
    }
    }

