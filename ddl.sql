CREATE TABLE [AccionUsuarios] (
    [IdAccion] int NOT NULL IDENTITY,
    [IdUsuario] int NULL,
    [IP] nvarchar(max) NULL,
    [TipoAccion] nvarchar(max) NULL,
    [Fecha] datetime2 NOT NULL,
    CONSTRAINT [PK_AccionUsuarios] PRIMARY KEY ([IdAccion])
);
GO


CREATE TABLE [Estado] (
    [IdEstado] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Estado] PRIMARY KEY ([IdEstado])
);
GO


CREATE TABLE [RecuperacionIntentos] (
    [Id] int NOT NULL IDENTITY,
    [UsuarioNombre] nvarchar(max) NULL,
    [Fecha] datetime2 NOT NULL,
    [Ip] nvarchar(max) NULL,
    [Exitoso] bit NOT NULL,
    [Motivo] nvarchar(max) NULL,
    CONSTRAINT [PK_RecuperacionIntentos] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [RecuperacionOtps] (
    [Id] int NOT NULL IDENTITY,
    [Correo] nvarchar(max) NOT NULL,
    [Codigo] nvarchar(max) NOT NULL,
    [Expira] datetime2 NOT NULL,
    [Usado] bit NOT NULL,
    CONSTRAINT [PK_RecuperacionOtps] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Rol] (
    [IdRol] int NOT NULL IDENTITY,
    [Nombre] nvarchar(30) NOT NULL,
    CONSTRAINT [PK_Rol] PRIMARY KEY ([IdRol])
);
GO


CREATE TABLE [Departamento] (
    [IdDepartamento] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NOT NULL,
    [Id_Estado] int NOT NULL,
    CONSTRAINT [PK_Departamento] PRIMARY KEY ([IdDepartamento]),
    CONSTRAINT [FK_Departamento_Estado_Id_Estado] FOREIGN KEY ([Id_Estado]) REFERENCES [Estado] ([IdEstado]) ON DELETE CASCADE
);
GO


CREATE TABLE [Etiqueta] (
    [IdEtiqueta] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NOT NULL,
    [Id_Estado] int NULL,
    CONSTRAINT [PK_Etiqueta] PRIMARY KEY ([IdEtiqueta]),
    CONSTRAINT [FK_Etiqueta_Estado_Id_Estado] FOREIGN KEY ([Id_Estado]) REFERENCES [Estado] ([IdEstado])
);
GO


CREATE TABLE [Usuario] (
    [IdUsuario] int NOT NULL IDENTITY,
    [Usuario] nvarchar(max) NOT NULL,
    [Nombre] nvarchar(max) NOT NULL,
    [Clave] nvarchar(max) NOT NULL,
    [Email] nvarchar(100) NULL,
    [Id_Estado] int NOT NULL,
    [IdRol] int NOT NULL,
    [RecuperacionToken] nvarchar(max) NULL,
    [RecuperacionTokenExpira] datetime2 NULL,
    CONSTRAINT [PK_Usuario] PRIMARY KEY ([IdUsuario]),
    CONSTRAINT [FK_Usuario_Estado_Id_Estado] FOREIGN KEY ([Id_Estado]) REFERENCES [Estado] ([IdEstado]) ON DELETE CASCADE,
    CONSTRAINT [FK_Usuario_Rol_IdRol] FOREIGN KEY ([IdRol]) REFERENCES [Rol] ([IdRol]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Contacto] (
    [IdContacto] int NOT NULL IDENTITY,
    [Id_Usuario] int NOT NULL,
    [Nombre] nvarchar(max) NOT NULL,
    [Apellido] nvarchar(max) NOT NULL,
    [Telefono] nvarchar(max) NOT NULL,
    [Correo] nvarchar(max) NULL,
    [Direccion] nvarchar(max) NULL,
    [Id_Departamento] int NULL,
    [Id_Estado] int NULL,
    CONSTRAINT [PK_Contacto] PRIMARY KEY ([IdContacto]),
    CONSTRAINT [FK_Contacto_Departamento_Id_Departamento] FOREIGN KEY ([Id_Departamento]) REFERENCES [Departamento] ([IdDepartamento]),
    CONSTRAINT [FK_Contacto_Estado_Id_Estado] FOREIGN KEY ([Id_Estado]) REFERENCES [Estado] ([IdEstado]),
    CONSTRAINT [FK_Contacto_Usuario_Id_Usuario] FOREIGN KEY ([Id_Usuario]) REFERENCES [Usuario] ([IdUsuario]) ON DELETE CASCADE
);
GO


CREATE TABLE [ContactoEtiqueta] (
    [IdContacto] int NOT NULL,
    [IdEtiqueta] int NOT NULL,
    CONSTRAINT [PK_ContactoEtiqueta] PRIMARY KEY ([IdContacto], [IdEtiqueta]),
    CONSTRAINT [FK_ContactoEtiqueta_Contacto_IdContacto] FOREIGN KEY ([IdContacto]) REFERENCES [Contacto] ([IdContacto]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ContactoEtiqueta_Etiqueta_IdEtiqueta] FOREIGN KEY ([IdEtiqueta]) REFERENCES [Etiqueta] ([IdEtiqueta]) ON DELETE NO ACTION
);
GO


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdRol', N'Nombre') AND [object_id] = OBJECT_ID(N'[Rol]'))
    SET IDENTITY_INSERT [Rol] ON;
INSERT INTO [Rol] ([IdRol], [Nombre])
VALUES (1, N'Admin'),
(2, N'Usuario');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'IdRol', N'Nombre') AND [object_id] = OBJECT_ID(N'[Rol]'))
    SET IDENTITY_INSERT [Rol] OFF;
GO


CREATE INDEX [IX_Contacto_Id_Departamento] ON [Contacto] ([Id_Departamento]);
GO


CREATE INDEX [IX_Contacto_Id_Estado] ON [Contacto] ([Id_Estado]);
GO


CREATE INDEX [IX_Contacto_Id_Usuario] ON [Contacto] ([Id_Usuario]);
GO


CREATE INDEX [IX_ContactoEtiqueta_IdEtiqueta] ON [ContactoEtiqueta] ([IdEtiqueta]);
GO


CREATE INDEX [IX_Departamento_Id_Estado] ON [Departamento] ([Id_Estado]);
GO


CREATE INDEX [IX_Etiqueta_Id_Estado] ON [Etiqueta] ([Id_Estado]);
GO


CREATE INDEX [IX_Usuario_Id_Estado] ON [Usuario] ([Id_Estado]);
GO


CREATE INDEX [IX_Usuario_IdRol] ON [Usuario] ([IdRol]);
GO


