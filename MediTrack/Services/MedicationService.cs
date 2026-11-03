using MediTrack.Data;
using MediTrack.Dtos.MedicationDto;
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
        public async Task<GetByIdMedicationDto> CreateMedicationAsync(CreateMedicationDto createMedicationDto)
        {
            //eklenecekler listesine koy henüz veritabanına yazılmadı
            var isSameBarcode=await _context.Medications.AnyAsync(e=>e.Barcode==createMedicationDto.Barcode);
            if (isSameBarcode) 
                throw new InvalidOperationException("Bu barkodla kayıtlı bir ilaç zaten var.");
            // 1. Entity oluştur
            var medication = new Medication
            {
                Name = createMedicationDto.Name,
                Barcode = createMedicationDto.Barcode,
                Description = createMedicationDto.Description
            };
            _context.Medications.Add(medication);   
            
            //Veritabanına yaz
            await _context.SaveChangesAsync();
            return new GetByIdMedicationDto
            {
                Id =medication.Id,
                Name=medication.Name,
                Barcode=medication.Barcode,
                Description=medication.Description,
            };

      
        }

        public async Task<List<GetByIdMedicationDto>> GetAllMedicationsAsync()
        {
            var medicationListEntity = await _context.Medications.AsNoTracking().ToListAsync();  // sadece okuyoruz, değiştirmeyeceğiz -> EF Core takip etmesin, daha hızlı
            var medications = new List<GetByIdMedicationDto>();
            foreach (var medication in medicationListEntity)
                medications.Add(new GetByIdMedicationDto
                {
                    Id= medication.Id,
                    Name = medication.Name,
                    Barcode=medication.Barcode,
                    Description=medication.Description,


                }
                    );
          

            return medications; // boşsa [] döner, frontend "Henüz ilaç eklenmemiş" yazar
        }

        public async Task<GetByIdMedicationDto> GetMedicationByIdAsync(int id)
        {
            var medicationEntity = await _context.Medications.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

            if (medicationEntity == null)

                throw new KeyNotFoundException("İlaç bulunamadı");

            var medication = new GetByIdMedicationDto
            {
                Id =medicationEntity.Id,
                Name = medicationEntity.Name,
                Barcode=medicationEntity.Barcode,
                Description=medicationEntity.Description,
            };

           return medication;

            
        }
        public async Task<UpdateMedicationDto> UpdateMedicationAsync(int id, UpdateMedicationDto updateMedicationDto)
        {


            var medicationEntity =await _context.Medications.FirstOrDefaultAsync(e => e.Id == id);

            // GetMedicationByIdAsync: Id URL'den → istenen kaynak yok → 404
            if (medicationEntity == null)
                throw new KeyNotFoundException("İlaç bulunamadı");


         
            var isSameBarcode = await _context.Medications.AnyAsync(e => e.Barcode == updateMedicationDto.Barcode && e.Id != id);
            if (isSameBarcode)
                throw new InvalidOperationException("Aynı barkoda sahip başka bir ilaç daha var");
            // 3. Alanları tek tek güncelle
            medicationEntity.Name = updateMedicationDto.Name;
            medicationEntity.Description = updateMedicationDto.Description;
            medicationEntity.Barcode = updateMedicationDto.Barcode;

            

            // 4. Değişiklikleri veritabanına yaz
            await _context.SaveChangesAsync();


            // 5. Güncellenmiş hâlini döndür
            return new UpdateMedicationDto
            {
                Name = medicationEntity.Name,
                Description = medicationEntity.Description,
                Barcode=medicationEntity.Barcode,


            };

        }


        // Include(am => am.Medication) dönen tipi DEĞİŞTİRMEZ; sorgu yine List<AppointmentMedication> döner.
        // Sadece her AppointmentMedication nesnesinin İÇİNDEKİ Medication property'sini doldurur.
        //
        // Include OLMADAN her nesne şöyle gelir:
        //
        // AppointmentMedication
        // ├── AppointmentId = 1
        // ├── MedicationId  = 100
        // ├── Dosage        = "1 tablet"
        // ├── Medication    = null        ← boş
        // └── Appointment   = null
        //
        // Include(am => am.Medication) İLE:
        //
        // AppointmentMedication
        // ├── AppointmentId = 1
        // ├── MedicationId  = 100
        // ├── Dosage        = "1 tablet"
        // ├── Medication    = { Id = 100, Name = "Parol", Barcode = ..., Description = ... }   ← dolu
        // └── Appointment   = null
        //
        // Appointment'ı Include etmedik çünkü sonuçta kullanmıyoruz.
        // Where içindeki am.Appointment.PatientId için Include gerekmez; EF Core join'i kendisi kurar.

        public async Task <List<GetByIdMedicationDto>> GetPatientMedicationsAsync(int patientId)
        {
            var patientExists = await _context.Patients.AnyAsync(e=>e.Id==patientId);
            if (!patientExists)
                throw new KeyNotFoundException($"{patientId} id'li hasta bulunamadı");
           // 2) Bu hastanın randevularına yazılmış ilaç kayıtlarını, ilaç bilgisiyle birlikte getir
            var appointmentMedicationsEntity=await _context.AppointmentMedications.Include(e=>e.Medication).Where(e=>e.Appointment.PatientId==patientId).ToListAsync();
           
            var medications=new List<GetByIdMedicationDto>();

            foreach (var appointmentMedication in appointmentMedicationsEntity)
            {
                //include sayesinde dolu olan ilaca ulaşıyoruz
                var medication = appointmentMedication.Medication;

                // bu ilaç listeye daha önce eklendiyse atla
                if (medications.Any(e => e.Id == medication.Id))
                    continue;

                //İlacı Dto'ya çevirip listeye ekle
                medications.Add(new GetByIdMedicationDto
                {
                    Id = medication.Id,
                    Name=medication.Name,
                    Barcode=medication.Barcode,
                    Description=medication.Description,

                });

            
              
            }



            return medications;


        }



    }
}
