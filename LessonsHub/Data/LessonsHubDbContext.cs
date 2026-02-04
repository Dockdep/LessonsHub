using LessonsHub.Entities;
using Microsoft.EntityFrameworkCore;

namespace LessonsHub.Data;

public class LessonsHubDbContext : DbContext
{
    public LessonsHubDbContext(DbContextOptions<LessonsHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<LessonDay> LessonDays { get; set; }
    public DbSet<LessonPlan> LessonPlans { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<ExerciseAnswer> ExerciseAnswers { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<Video> Videos { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Documentation> Documentation { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // LessonDay configuration
        modelBuilder.Entity<LessonDay>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ShortDescription).HasMaxLength(500);
            entity.Property(e => e.Date).IsRequired();
        });

        // LessonPlan configuration
        modelBuilder.Entity<LessonPlan>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Topic).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedDate).IsRequired();
        });

        // Lesson configuration
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LessonNumber).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ShortDescription).HasMaxLength(500);
            entity.Property(e => e.Content);
            entity.Property(e => e.LessonType).HasMaxLength(200);
            entity.Property(e => e.LessonTopic).HasMaxLength(200);
            entity.Property(e => e.KeyPoints).HasColumnType("jsonb");

            // Relationship with LessonPlan
            entity.HasOne(e => e.LessonPlan)
                .WithMany(lp => lp.Lessons)
                .HasForeignKey(e => e.LessonPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship with LessonDay
            entity.HasOne(e => e.LessonDay)
                .WithMany(ld => ld.Lessons)
                .HasForeignKey(e => e.LessonDayId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Exercise configuration
        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExerciseText).IsRequired();
            entity.Property(e => e.Difficulty).HasMaxLength(50);

            // Relationship with Lesson
            entity.HasOne(e => e.Lesson)
                .WithMany(l => l.Exercises)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ExerciseAnswer configuration
        modelBuilder.Entity<ExerciseAnswer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserResponse).IsRequired();
            entity.Property(e => e.SubmittedAt).IsRequired();
            entity.Property(e => e.AccuracyLevel);
            entity.Property(e => e.ReviewText);

            // Relationship with Exercise
            entity.HasOne(e => e.Exercise)
                .WithMany(ex => ex.Answers)
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ChatMessage configuration
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Text).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Optional relationships - one or the other will be set
            entity.HasOne<Lesson>()
                .WithMany(l => l.ChatHistory)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            entity.HasOne<Exercise>()
                .WithMany(ex => ex.ChatHistory)
                .HasForeignKey(e => e.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);
        });

        // Video configuration
        modelBuilder.Entity<Video>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Channel).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);

            entity.HasOne(e => e.Lesson)
                .WithMany(l => l.Videos)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Book configuration
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Author).IsRequired().HasMaxLength(200);
            entity.Property(e => e.BookName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ChapterName).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasOne(e => e.Lesson)
                .WithMany(l => l.Books)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Documentation configuration
        modelBuilder.Entity<Documentation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Section).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);

            entity.HasOne(e => e.Lesson)
                .WithMany(l => l.Documentation)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
