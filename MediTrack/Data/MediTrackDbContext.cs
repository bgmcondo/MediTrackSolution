using MediTrack.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediTrack.Data
{
    public class MediTrackDbContext : DbContext
    {

        // Dependency injection ile DbContextOptions alıyoruz. Bu sayede farklı konfigürasyonlarla DbContext oluşturabiliriz.
        // base, kalıtım alınan üst sınıfa erişmek için kullanılır.
        // base(options) → DbContext'in constructor'ını çağırır.
        public MediTrackDbContext(DbContextOptions<MediTrackDbContext> options) : base(options)
        {


        }

        // Fluent API kullanarak model konfigürasyonu yapıyoruz. Bu sayede entity'lerimizin veritabanındaki tablolarla nasıl eşleneceğini belirleyebiliriz.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // base.OnModelCreating(modelBuilder) → DbContext'in
            // OnModelCreating metodunu çağırır.
            base.OnModelCreating(modelBuilder);

            // AppointmentMedication tablosunda iki alan
            // birlikte Primary Key olarak kullanılacak.

            /*
             * new AppointmentMedication { } ile new { } arasındaki fark:
             *
             * 1. new AppointmentMedication { }
             *    - AppointmentMedication tipinde bir Entity nesnesi oluşturur.
             *    - Entity'nin property'lerine değer atamak için kullanılır.
             *    - Yeni kayıt oluştururken kullanılabilir.
             *
             * 2. new { }
             *    - Anonymous Type (anonim tip) oluşturur.
             *    - Seçilen property'leri bir araya getirir.
             *    - LINQ ve Fluent API işlemlerinde sık kullanılır.
             *
             * Örnek:
             * new AppointmentMedication { AppointmentId = 1, ... }
             *    → AppointmentMedication nesnesi oluşturur.
             *
             * new { am.AppointmentId, am.MedicationId }
             *    → İki property'yi anonim tipte bir araya getirir.
             *    → HasKey() içinde Composite Primary Key tanımlamak
             *      için kullanılır, veritabanına kayıt eklemez.
             */

            modelBuilder.Entity<AppointmentMedication>().HasKey(am => new { am.AppointmentId, am.MedicationId });


            // Aynı TC kimlik numarasıyla
            // ikinci bir hasta oluşturulamaz.
            modelBuilder.Entity<Patient>().HasIndex(e => e.Id).IsUnique();



            // Aynı barkodla ikinci bir ilaç oluşturulamaz.

            modelBuilder.Entity<Medication>().HasIndex(e=>e.Id).IsUnique();






        }

    }
}


