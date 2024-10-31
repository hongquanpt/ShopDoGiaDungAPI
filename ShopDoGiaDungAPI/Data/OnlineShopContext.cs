using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShopDoGiaDungAPI.Models;

namespace ShopDoGiaDungAPI.Data;

public partial class OnlineShopContext : DbContext
{
    public OnlineShopContext()
    {
    }

    public OnlineShopContext(DbContextOptions<OnlineShopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActionT> ActionTs { get; set; }

    public virtual DbSet<Chitietdonhang> Chitietdonhangs { get; set; }

    public virtual DbSet<ChucVu> ChucVus { get; set; }

    public virtual DbSet<CvQA> CvQAs { get; set; }

    public virtual DbSet<Danhgiasanpham> Danhgiasanphams { get; set; }

    public virtual DbSet<Danhmucsanpham> Danhmucsanphams { get; set; }

    public virtual DbSet<Donhang> Donhangs { get; set; }

    public virtual DbSet<GioHang> GioHangs { get; set; }

    public virtual DbSet<Hangsanxuat> Hangsanxuats { get; set; }

    public virtual DbSet<MigrationHistory> MigrationHistories { get; set; }

    public virtual DbSet<Quyen> Quyens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sanpham> Sanphams { get; set; }

    public virtual DbSet<Taikhoan> Taikhoans { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<Vanchuyen> Vanchuyens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=QUAN\\QUAN;Initial Catalog=OnlineShop;Integrated Security=True;Encrypt=false;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActionT>(entity =>
        {
            entity.HasKey(e => e.MaA).HasName("PK_Action");

            entity.ToTable("ActionT");

            entity.Property(e => e.MaA).ValueGeneratedNever();
            entity.Property(e => e.TenA).HasMaxLength(100);
        });

        modelBuilder.Entity<Chitietdonhang>(entity =>
        {
            entity.HasKey(e => new { e.MaDonHang, e.MaSp });

            entity.ToTable("CHITIETDONHANG");

            entity.Property(e => e.MaSp).HasColumnName("MaSP");

            entity.HasOne(d => d.MaDonHangNavigation).WithMany(p => p.Chitietdonhangs)
                .HasForeignKey(d => d.MaDonHang)
                .HasConstraintName("FK_CHITIETDONHANG_DONHANG");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.Chitietdonhangs)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_CHITIETDONHANG_SANPHAM");
        });

        modelBuilder.Entity<ChucVu>(entity =>
        {
            entity.HasKey(e => e.MaCv);

            entity.ToTable("ChucVu");

            entity.Property(e => e.MaCv)
                .ValueGeneratedNever()
                .HasColumnName("MaCV");
            entity.Property(e => e.Ten).HasMaxLength(100);
        });

        modelBuilder.Entity<CvQA>(entity =>
        {
            entity.HasKey(e => new { e.MaA, e.MaCv, e.MaQ });

            entity.ToTable("CV_Q_A");

            entity.Property(e => e.MaCv).HasColumnName("MaCV");
            entity.Property(e => e.Ten).HasMaxLength(50);

            entity.HasOne(d => d.MaANavigation).WithMany(p => p.CvQAs)
                .HasForeignKey(d => d.MaA)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CV_Q_A_ActionT");

            entity.HasOne(d => d.MaCvNavigation).WithMany(p => p.CvQAs)
                .HasForeignKey(d => d.MaCv)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CV_Q_A_ChucVu");

            entity.HasOne(d => d.MaQNavigation).WithMany(p => p.CvQAs)
                .HasForeignKey(d => d.MaQ)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CV_Q_A_Quyen");
        });

        modelBuilder.Entity<Danhgiasanpham>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("DANHGIASANPHAM");

            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.NoiDungBinhLuan).HasMaxLength(200);

            entity.HasOne(d => d.MaSpNavigation).WithMany()
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_DANHGIASANPHAM_SANPHAM");

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany()
                .HasForeignKey(d => d.MaTaiKhoan)
                .HasConstraintName("FK_DANHGIASANPHAM_TAIKHOAN");
        });

        modelBuilder.Entity<Danhmucsanpham>(entity =>
        {
            entity.HasKey(e => e.MaDanhMuc);

            entity.ToTable("DANHMUCSANPHAM");

            entity.Property(e => e.TenDanhMuc).HasMaxLength(30);
        });

        modelBuilder.Entity<Donhang>(entity =>
        {
            entity.HasKey(e => e.MaDonHang);

            entity.ToTable("DONHANG");

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany(p => p.Donhangs)
                .HasForeignKey(d => d.MaTaiKhoan)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DONHANG_TAIKHOAN");
        });

        modelBuilder.Entity<GioHang>(entity =>
        {
            entity.HasKey(e => new { e.MaTaiKhoan, e.MaSp });

            entity.ToTable("GioHang");

            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.Soluong).HasColumnName("soluong");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.MaSp)
                .HasConstraintName("FK_GioHang_SANPHAM");

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithMany(p => p.GioHangs)
                .HasForeignKey(d => d.MaTaiKhoan)
                .HasConstraintName("FK_GioHang_TAIKHOAN");
        });

        modelBuilder.Entity<Hangsanxuat>(entity =>
        {
            entity.HasKey(e => e.MaHang);

            entity.ToTable("HANGSANXUAT");

            entity.Property(e => e.TenHang).HasMaxLength(20);
        });

        modelBuilder.Entity<MigrationHistory>(entity =>
        {
            entity.HasKey(e => new { e.MigrationId, e.ContextKey }).HasName("PK_dbo.__MigrationHistory");

            entity.ToTable("__MigrationHistory");

            entity.Property(e => e.MigrationId).HasMaxLength(150);
            entity.Property(e => e.ContextKey).HasMaxLength(300);
            entity.Property(e => e.ProductVersion).HasMaxLength(32);
        });

        modelBuilder.Entity<Quyen>(entity =>
        {
            entity.HasKey(e => e.MaQ);

            entity.ToTable("Quyen");

            entity.Property(e => e.MaQ).ValueGeneratedNever();
            entity.Property(e => e.ActionName).HasMaxLength(200);
            entity.Property(e => e.ControllerName).HasMaxLength(200);
            entity.Property(e => e.Ten).HasMaxLength(200);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1AE26BA8FE");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160B39AB150").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Sanpham>(entity =>
        {
            entity.HasKey(e => e.MaSp);

            entity.ToTable("SANPHAM");

            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.Anh1).HasMaxLength(100);
            entity.Property(e => e.Anh2).HasMaxLength(100);
            entity.Property(e => e.Anh3).HasMaxLength(100);
            entity.Property(e => e.Anh4).HasMaxLength(100);
            entity.Property(e => e.Anh5).HasMaxLength(100);
            entity.Property(e => e.Anh6).HasMaxLength(100);
            entity.Property(e => e.MoTa).HasMaxLength(1000);
            entity.Property(e => e.TenSp)
                .HasMaxLength(100)
                .HasColumnName("TenSP");

            entity.HasOne(d => d.MaDanhMucNavigation).WithMany(p => p.Sanphams)
                .HasForeignKey(d => d.MaDanhMuc)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SANPHAM_DANHMUCSANPHAM");

            entity.HasOne(d => d.MaHangNavigation).WithMany(p => p.Sanphams)
                .HasForeignKey(d => d.MaHang)
                .HasConstraintName("FK_SANPHAM_HANGSANXUAT");
        });

        modelBuilder.Entity<Taikhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan);

            entity.ToTable("TAIKHOAN");

            entity.Property(e => e.DiaChi).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(20);
            entity.Property(e => e.MaCv).HasColumnName("MaCV");
            entity.Property(e => e.MatKhau).HasMaxLength(100);
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .IsFixedLength()
                .HasColumnName("SDT");
            entity.Property(e => e.Ten).HasMaxLength(30);

            entity.HasOne(d => d.MaCvNavigation).WithMany(p => p.Taikhoans)
                .HasForeignKey(d => d.MaCv)
                .HasConstraintName("FK_TAIKHOAN_ChucVu");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__UserRole__AF2760ADC2DA84DC");

            entity.Property(e => e.Ten)
                .HasMaxLength(10)
                .IsFixedLength();

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__UserRoles__RoleI__06CD04F7");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserRoles__UserI__05D8E0BE");
        });

        modelBuilder.Entity<Vanchuyen>(entity =>
        {
            entity.HasKey(e => e.MaDonHang);

            entity.ToTable("VANCHUYEN");

            entity.Property(e => e.MaDonHang).ValueGeneratedNever();
            entity.Property(e => e.DiaChi).HasMaxLength(50);
            entity.Property(e => e.HinhThucVanChuyen).HasMaxLength(20);
            entity.Property(e => e.NguoiNhan).HasMaxLength(50);
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .HasColumnName("SDT");

            entity.HasOne(d => d.MaDonHangNavigation).WithOne(p => p.Vanchuyen)
                .HasForeignKey<Vanchuyen>(d => d.MaDonHang)
                .HasConstraintName("FK_VANCHUYEN_DONHANG");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
