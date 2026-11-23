using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RestAPI.Models.Data;

public partial class LibraryDbContext : DbContext
{
    public LibraryDbContext()
    {
    }

    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Loan> Loans { get; set; }

    public virtual DbSet<Publisher> Publishers { get; set; }

    public virtual DbSet<Reader> Readers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=LibraryDB;Username=user;Password=DB_PASSWORD");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookCipher).HasName("Books_pkey");

            entity.Property(e => e.BookCipher).HasColumnName("Book_cipher");
            entity.Property(e => e.BookName).HasColumnName("Book_name");
            entity.Property(e => e.BookPrice).HasColumnName("Book_price");
            entity.Property(e => e.FirstAuthor).HasColumnName("First_author");
            entity.Property(e => e.NumberOfCopies).HasColumnName("Number_of_copies");
            entity.Property(e => e.PublisherId).HasColumnName("Publisher_id");
            entity.Property(e => e.YearOfPublication).HasColumnName("Year_of_publication");

            entity.HasOne(d => d.Publisher).WithMany(p => p.Books)
                .HasForeignKey(d => d.PublisherId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("Books_Publisher_id_fkey");
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(e => e.LoanId).HasName("Loans_pkey");

            entity.Property(e => e.LoanId).HasColumnName("Loan_id");
            entity.Property(e => e.BookCipher).HasColumnName("Book_cipher");
            entity.Property(e => e.DateOfIssue).HasColumnName("Date_of_issue");
            entity.Property(e => e.ReaderId).HasColumnName("Reader_id");

            entity.HasOne(d => d.BookCipherNavigation).WithMany(p => p.Loans)
                .HasForeignKey(d => d.BookCipher)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("Loans_Book_cipher_fkey");

            entity.HasOne(d => d.Reader).WithMany(p => p.Loans)
                .HasForeignKey(d => d.ReaderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("Loans_Reader_id_fkey");
        });

        modelBuilder.Entity<Publisher>(entity =>
        {
            entity.HasKey(e => e.PublisherId).HasName("Publishers_pkey");

            entity.Property(e => e.PublisherId).HasColumnName("Publisher_id");
            entity.Property(e => e.PublisherCity).HasColumnName("Publisher_city");
            entity.Property(e => e.PublisherName).HasColumnName("Publisher_name");
        });

        modelBuilder.Entity<Reader>(entity =>
        {
            entity.HasKey(e => e.ReaderId).HasName("Readers_pkey");

            entity.Property(e => e.ReaderId).HasColumnName("Reader_id");
            entity.Property(e => e.ReaderAddress).HasColumnName("Reader_address");
            entity.Property(e => e.ReaderFullName).HasColumnName("Reader_full_name");
            entity.Property(e => e.ReaderPhone).HasColumnName("Reader_phone");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
