using Microsoft.EntityFrameworkCore;
namespace StajTakipWeb.Data;
public static class OgrenciPanelSchema
{
    public static Task EnsureAsync(StajTakipContext db) => db.Database.ExecuteSqlRawAsync("""
        IF OBJECT_ID(N'dbo.Duyurular', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[Duyurular] (
                [Id] int IDENTITY PRIMARY KEY, [Baslik] nvarchar(150) NOT NULL,
                [Icerik] nvarchar(max) NOT NULL, [YayinTarihi] datetime2 NOT NULL,
                [Bitis] datetime2 NULL, [YayindaMi] bit NOT NULL
            );
        END;
        IF OBJECT_ID(N'dbo.DuyuruOkumalari', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[DuyuruOkumalari] (
                [DuyuruId] int NOT NULL, [KullaniciId] nvarchar(450) NOT NULL,
                [OkumaTarihi] datetime2 NOT NULL,
                PRIMARY KEY ([DuyuruId], [KullaniciId]),
                FOREIGN KEY ([DuyuruId]) REFERENCES [dbo].[Duyurular] ([Id]) ON DELETE CASCADE,
                FOREIGN KEY ([KullaniciId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
            );
            CREATE INDEX [IX_DuyuruOkumalari_KullaniciId] ON [dbo].[DuyuruOkumalari] ([KullaniciId]);
        END;
        IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.Ogrenciler') AND name = N'CK_Ogrenciler_Sinif')
           OR EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.Ogrenciler') AND name = N'CK_Ogrenciler_Sinif' AND definition LIKE N'%(8)%')
        BEGIN
            BEGIN TRANSACTION;
            IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.Ogrenciler') AND name = N'CK_Ogrenciler_Sinif')
                ALTER TABLE [dbo].[Ogrenciler] DROP CONSTRAINT [CK_Ogrenciler_Sinif];
            -- Eski sınıf verileri korunur; kural yeni ve değiştirilen sınıflarda uygulanır.
            ALTER TABLE [dbo].[Ogrenciler] WITH NOCHECK ADD CONSTRAINT [CK_Ogrenciler_Sinif]
                CHECK ([Sinif] IS NULL OR [Sinif] BETWEEN 9 AND 12);
            COMMIT TRANSACTION;
        END;
        IF COL_LENGTH('dbo.AspNetUsers', 'AktifMi') IS NULL
            ALTER TABLE [dbo].[AspNetUsers] ADD [AktifMi] bit NOT NULL CONSTRAINT [DF_AspNetUsers_AktifMi] DEFAULT 1;
        IF COL_LENGTH('dbo.AspNetUsers', 'SonGiris') IS NULL
            ALTER TABLE [dbo].[AspNetUsers] ADD [SonGiris] datetime2 NULL;
        IF COL_LENGTH('dbo.AspNetUsers', 'HesapNotu') IS NULL
            ALTER TABLE [dbo].[AspNetUsers] ADD [HesapNotu] nvarchar(500) NULL;
        IF OBJECT_ID(N'[dbo].[Odevler]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[Odevler] (
                [Id] int IDENTITY PRIMARY KEY, [OgrenciID] int NOT NULL,
                [AtayanId] nvarchar(450) NOT NULL, [Baslik] nvarchar(150) NOT NULL,
                [Aciklama] nvarchar(max) NOT NULL, [SonTeslim] datetime2 NOT NULL,
                [Olusturma] datetime2 NOT NULL, [TeslimMetni] nvarchar(max) NULL,
                [TeslimBaglantisi] nvarchar(2000) NULL, [TeslimTarihi] datetime2 NULL,
                [Not] int NULL CHECK ([Not] BETWEEN 0 AND 100), [GeriBildirim] nvarchar(4000) NULL,
                FOREIGN KEY ([OgrenciID]) REFERENCES [dbo].[Ogrenciler] ([OgrenciID])
            );
            CREATE INDEX [IX_Odevler_OgrenciID] ON [dbo].[Odevler] ([OgrenciID]);
        END;
        IF COL_LENGTH('dbo.Odevler', 'TeslimDosyasi') IS NULL
            ALTER TABLE [dbo].[Odevler] ADD [TeslimDosyasi] nvarchar(500) NULL;
        IF OBJECT_ID(N'[dbo].[StajBilgileri]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[StajBilgileri] (
                [Id] int IDENTITY PRIMARY KEY, [OgrenciID] int NOT NULL,
                [FirmaAdi] nvarchar(200) NOT NULL, [FirmaAdresi] nvarchar(500) NOT NULL,
                [SorumluAdi] nvarchar(150) NOT NULL, [SorumluEposta] nvarchar(200) NOT NULL,
                [Baslangic] datetime2 NULL, [Bitis] datetime2 NULL,
                FOREIGN KEY ([OgrenciID]) REFERENCES [dbo].[Ogrenciler] ([OgrenciID]),
                UNIQUE ([OgrenciID])
            );
        END;
        IF OBJECT_ID(N'[dbo].[StajRaporlari]', N'U') IS NULL
        BEGIN
            CREATE TABLE [dbo].[StajRaporlari] (
                [Id] int IDENTITY PRIMARY KEY, [OgrenciID] int NOT NULL,
                [Tarih] datetime2 NULL, [Baslik] nvarchar(150) NOT NULL,
                [Icerik] nvarchar(max) NOT NULL, [Durum] nvarchar(30) NOT NULL,
                [Degerlendirme] nvarchar(max) NULL,
                FOREIGN KEY ([OgrenciID]) REFERENCES [dbo].[Ogrenciler] ([OgrenciID]),
                UNIQUE ([OgrenciID], [Tarih])
            );
        END;
        """);
}
