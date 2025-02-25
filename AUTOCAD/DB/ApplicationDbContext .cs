using AUTOCAD.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AUTOCAD.DB
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<AUTOCAD.Models.Kurs> kurs { get; set; }
        public DbSet<AUTOCAD.Models.ApplicationUser> ApplicationUser { get; set; }

        public DbSet<AUTOCAD.Models.AnswerViewModel> AnswerViewModel { get; set; }
        public DbSet<AUTOCAD.Models.QuestionViewModel> QuestionViewModel { get; set; }
        public DbSet<AUTOCAD.Models.QuizViewModel> QuizViewModel { get; set; }


        public DbSet<AUTOCAD.Models.QuizResultViewModel> QuizResultViewModel { get; set; }


        public DbSet<Komentarz> Komentarze { get; set; }


        public DbSet<Chat> Chats { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Chat>()
                .HasOne(u => u.Sender)
                .WithMany()
                .HasForeignKey(u => u.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // <- ZMIENIONO NA RESTRICT

            modelBuilder.Entity<Chat>()
                .HasOne(u => u.Receiver)
                .WithMany()
                .HasForeignKey(u => u.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict); // <- ZMIENIONO NA RESTRICT
        }


    }
}

/*
  dotnet ef database drop --force
  dotnet ef migrations remove
  dotnet ef migrations add Baza1
  dotnet ef database update
*/
