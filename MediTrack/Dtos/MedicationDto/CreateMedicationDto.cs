namespace MediTrack.Dtos.MedicationDto
{
    public class CreateMedicationDto
    {
        public required string Name { get; set; } //required null da olsa bir değer atanmasını zorunlu kılar 

        public required string Barcode { get; set; }

        public required string Description { get; set; }
    }
}
