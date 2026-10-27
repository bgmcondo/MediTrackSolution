using MediTrack.Data;
using MediTrack.Entities;
using MediTrack.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly MediTrackDbContext _context;

        public MedicationService(MediTrackDbContext context)
        {
            _context = context;

            
        }
        public async Task<Medication> CreateMedicationAsync(Medication medication)
        {
            //eklenecekler listesine koy henüz veritabanına yazılmadı
            _context.Medications.Add(medication);   
            
            //Veritabanına yaz
            await _context.SaveChangesAsync();
            return medication;
      
        }

        public async Task<List<Medication>> GetAllMedicationsAsync()
        {
            var medicationList = await _context.Medications.AsNoTracking().ToListAsync();  // sadece okuyoruz, değiştirmeyeceğiz -> EF Core takip etmesin, daha hızlı


            return medicationList; // boşsa [] döner, frontend "Henüz ilaç eklenmemiş" yazar
        }

        public async Task<Medication> GetMedicationByIdAsync(int id)
        {
            var medication = await _context.Medications.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

            if (medication == null)
           
                throw new ArgumentException("İlaç bulunamadı");

           return medication;

            
        }
        public async Task<Medication> UpdateMedicationAsync(int id, Medication medication)
        {
            var targetMedication =_context.Medications.FirstOrDefault(e => e.Id == id);
            if (targetMedication == null)
                return null;

            // 3. Alanları tek tek güncelle
            targetMedication.Name = medication.Name;
            targetMedication.Description = medication.Description;
            targetMedication.Barcode = medication.Barcode;

            // 4. Değişiklikleri veritabanına yaz
            await _context.SaveChangesAsync();

            // 5. Güncellenmiş hâlini döndür
            return targetMedication;

        }


    }
}
